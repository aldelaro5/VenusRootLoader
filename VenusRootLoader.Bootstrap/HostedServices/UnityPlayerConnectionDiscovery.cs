using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VenusRootLoader.Bootstrap.Settings;
using Timer = System.Timers.Timer;

namespace VenusRootLoader.Bootstrap.HostedServices;

public class UnityPlayerConnectionDiscovery : IHostedService
{
    private readonly ILogger<UnityPlayerConnectionDiscovery> _logger;
    private readonly MonoDebuggerSettings _monoDebuggerSettings;

    private readonly Socket? _socket;
    private readonly IPEndPoint? _endPoint;
    private readonly string? _message;
    private readonly byte[]? _messageBuffer;

    private const string FlagsTakeIpFromMessage = "8";
    private const string FlagsTakeIpFromSource = "0";

    public UnityPlayerConnectionDiscovery(ILogger<UnityPlayerConnectionDiscovery> logger, IOptions<MonoDebuggerSettings> monoDebuggerSettings)
    {
        _logger = logger;
        _monoDebuggerSettings = monoDebuggerSettings.Value;
        if (!_monoDebuggerSettings.Enable!.Value)
            return;

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socket.Blocking = false;
        _socket.EnableBroadcast = true;
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 31);
        _socket.Ttl = 31;
        _socket.MulticastLoopback = true;

        var broadcastIp = IPAddress.Parse("225.0.0.222");
        _endPoint = new IPEndPoint(broadcastIp, 54997);

        _message = ConstructWhoAmIString(IPAddress.Parse(_monoDebuggerSettings.IpAddress), (ushort)_monoDebuggerSettings.Port);
        _messageBuffer = Encoding.ASCII.GetBytes(_message);
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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_monoDebuggerSettings.Enable!.Value)
            return Task.CompletedTask;

        _logger.LogInformation("TestSocketStuff starting...");
        var timer = new Timer(1000);
        timer.Elapsed += TimerOnElapsed;
        timer.Start();
        return Task.CompletedTask;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var bytesSent = _socket?.SendTo(_messageBuffer!, SocketFlags.None, _endPoint!);
        _logger.LogTrace("Sent message to socket of length {bytesSent}: {message}", bytesSent, _message);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}