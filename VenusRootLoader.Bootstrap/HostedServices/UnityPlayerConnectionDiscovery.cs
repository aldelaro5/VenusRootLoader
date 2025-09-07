using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Services;
using Timer = System.Timers.Timer;

namespace VenusRootLoader.Bootstrap.HostedServices;

public class UnityPlayerConnectionDiscovery
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private unsafe delegate int SendToFn(
        SOCKET s,
        PCSTR buf,
        int len,
        int flags,
        SOCKADDR* to,
        int toLen);
    private static SendToFn _sendToDelegate = null!;

    private const string IpMessageDestination = "225.0.0.222";
    private const int PortMessageDestination = 54997;
    private const string FlagsTakeIpFromMessage = "8";
    private const string FlagsTakeIpFromSource = "0";

    private readonly ILogger<UnityPlayerConnectionDiscovery> _logger;
    private readonly PltHook _pltHook;
    private readonly GameExecutionContext _gameExecutionContext;

    private Socket? _socket;
    private IPEndPoint? _endPoint;
    private string _message = string.Empty;
    private byte[] _messageBuffer = [];
    private unsafe byte* _messagePtr = null;

    public unsafe UnityPlayerConnectionDiscovery(ILogger<UnityPlayerConnectionDiscovery> logger, PltHook pltHook, GameExecutionContext gameExecutionContext)
    {
        _logger = logger;
        _pltHook = pltHook;
        _gameExecutionContext = gameExecutionContext;
        _sendToDelegate = SendToHook;
    }

    private string ConstructWhoAmIString(IPAddress ipAddress, ushort port)
    {
        IPAddress addressToUse = ipAddress;
        ushort portToUse = port;
        string? dnSpyEnv = Environment.GetEnvironmentVariable("DNSPY_UNITY_DBG2");
        if (dnSpyEnv is not null)
        {
            var arguments = dnSpyEnv.Split(',');
            var addressArgument = arguments.Single(x => x.StartsWith("address=")).TrimStart("address=").ToString();
            var addressParts  = addressArgument.Split(':');
            addressToUse = IPAddress.Parse(addressParts[0]);
            portToUse = ushort.Parse(addressParts[1]);
            _logger.LogInformation("Overriding the IP address to {ipAddress}:{port} from the DNSPY_UNITY_DBG2 environment variable",
                addressToUse, portToUse);
        }

        StringBuilder sb = new StringBuilder();
        var ipAddressString = addressToUse.ToString();
        sb.Append($"[IP] {ipAddressString} ");
        sb.Append($"[Port] {Random.Shared.Next(55000, 55512)} ");
        var flags = ipAddressString == IPAddress.Loopback.ToString()
            ? FlagsTakeIpFromMessage
            : FlagsTakeIpFromSource;
        sb.Append($"[Flags] {flags} ");
        sb.Append($"[Guid] {(uint)Random.Shared.Next()} ");
        sb.Append("[EditorId] 0 ");
        sb.Append("[Version] 1048832 ");
        sb.Append($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{portToUse} ");
        sb.Append("[Debug] 1 ");
        sb.Append("[PackageName] WindowsPlayer ");
        sb.Append("[ProjectName] Bug Fables\0");
        return sb.ToString();
    }

    internal void StartDiscoveryWithOwnSocket(string ipAddress, ushort port)
    {
        _message = ConstructWhoAmIString(IPAddress.Parse(ipAddress), port);
        _messageBuffer = Encoding.ASCII.GetBytes(_message);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socket.Blocking = false;
        _socket.EnableBroadcast = true;
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 31);
        _socket.Ttl = 31;
        _socket.MulticastLoopback = true;

        var broadcastIp = IPAddress.Parse(IpMessageDestination);
        _endPoint = new IPEndPoint(broadcastIp, PortMessageDestination);

        var timer = new Timer(1000);
        timer.Elapsed += TimerOnElapsed;
        timer.Start();

        _logger.LogInformation("Sending {message} to {ipAddress}:{port} every second",
            _message, IpMessageDestination, PortMessageDestination);
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var bytesSent = _socket?.SendTo(_messageBuffer, SocketFlags.None, _endPoint!);
        _logger.LogTrace("Sent message to socket of length {bytesSent}: {message}", bytesSent, _message);
    }

    internal unsafe void StartDiscoveryWithSendToHook(string ipAddress, ushort port)
    {
        _message = ConstructWhoAmIString(IPAddress.Parse(ipAddress), port);
        _messagePtr = (byte*)Marshal.StringToHGlobalAnsi(_message);

        _pltHook.InstallHook(_gameExecutionContext.UnityPlayerDllFileName,
            nameof(PInvoke.sendto),
            Marshal.GetFunctionPointerForDelegate(_sendToDelegate));
    }

    private unsafe int SendToHook(SOCKET s, PCSTR buf, int len, int flags, SOCKADDR* to, int toLen)
    {
        _logger.LogTrace("Overriding message to send via sendto of length {bytesSent}: {message}", _message.Length, _message);
        return PInvoke.sendto(s, new(_messagePtr), _message.Length, flags, to, toLen);
    }
}