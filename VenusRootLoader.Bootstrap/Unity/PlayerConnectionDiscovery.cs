using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Shared;
using PltHook = VenusRootLoader.Bootstrap.Shared.PltHook;
using Timer = System.Timers.Timer;

namespace VenusRootLoader.Bootstrap.Unity;

/// <summary>
/// <para>
/// This service implements the ability for IDEs to discover the connection information of the Mono debugger without having
/// to input the IP address / port manually. It does this by replicating what a dev UnityPlayer.dll does to enable this
/// feature: by sending a UDP packet to a specific IP / port every second. This packet contains a message that informs
/// the IDE who the player is and how it can connect to it. It offers enough flexibility for us to control the message
/// in such a way that the IP / port can match the ones in the configuration and therefore have IDEs connect with the correct
/// information.
/// </para>
/// <para>
/// The method of implementation depends on if the UnityPlayer.dll is a dev build or not. If it's not one, we simply send
/// the UDP packet ourselves every second. If it is a dev (which is assessed by mono_debug_init being called by UnityPlayer.dll),
/// we need to edit the message Unity wants to send via a sendto PltHook so it contains the correct connection information
/// and offers a similar experience than a non dev build. The format of the message isn't officially documented by Unity,
/// but the Resharper-Unity source code contains various comments that documents it and everything mentioned was found
/// to be mostly true using Ghidra.
/// The source file containing these comments can be consulted here: https://github.com/JetBrains/resharper-unity/blob/0ef394cb50c4cffda3cde3c3f881fce05dad602b/rider/src/main/kotlin/com/jetbrains/rider/plugins/unity/run/UnityPlayerListener.kt
/// </para>
/// </summary>
public class PlayerConnectionDiscovery
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

    // While Resharper-Unity seems to indicate other ports could be used, this port number was found to be hardcoded for
    // Unity 2018.4.12f1 so it's fine to only use it. 
    private const string IpMessageDestination = "225.0.0.222";
    private const int PortMessageDestination = 54997;
    private const string FlagsTakeIpFromMessage = "8";
    private const string FlagsTakeIpFromSource = "0";

    private readonly ILogger<PlayerConnectionDiscovery> _logger;
    private readonly PltHook _pltHook;
    private readonly GameExecutionContext _gameExecutionContext;

    private Socket? _socket;
    private IPEndPoint? _endPoint;
    private string _message = string.Empty;
    private byte[] _messageBuffer = [];
    private unsafe byte* _messagePtr = null;

    public unsafe PlayerConnectionDiscovery(ILogger<PlayerConnectionDiscovery> logger, PltHook pltHook, GameExecutionContext gameExecutionContext)
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
        // For consistency, we still want to honor Dnspy's wishes to override the debug server connection, just like a manual connection
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
        // This wouldn't be used unless [Flags] has its 4th bit set (8 in decimal) so we can set both to force the IP. This
        // is used in the loopback IP case explained below
        sb.Append($"[IP] {ipAddressString} ");
        // The exact method of randomness wasn't documented by Resharper-Unity, but it was confirmed by checking in Ghidra.
        // Just like mentioned in Resharper-Unity's sources, this doesn't appear to matter for debugger connection
        sb.Append($"[Port] {Random.Shared.Next(55000, 55512)} ");
        // Loopback is an interesting case because it only works if the IDE explicitly tries to connect to this address
        // so the LAN IP wouldn't work while it otherwise would. We special case it so we force the IP instead of letting
        // the IDE assume it can take the same IP the message originated from. Interestingly, bit 8 isn't used by this
        // Unity version, but IDEs can still support it
        var flags = ipAddressString == IPAddress.Loopback.ToString()
            ? FlagsTakeIpFromMessage
            : FlagsTakeIpFromSource;
        // A dev build of UnityPlayer.dll normally sends 2 which according to Resharper-Unity means that the player supports
        // profiling. We can't get that unless the game uses an actual dev build of UnityPlayer.dll so we always want to send
        // 0 (or 8 in the case of loopback mentioned above). It does not appear that bit 1 and 4 are used by this Unity version
        // in any meaningful ways
        sb.Append($"[Flags] {flags} ");
        // This method of randomness was confirmed from checking in Ghidra. It does seem to be used by the Unity Editor,
        // but doesn't change much as far as the debugger connection goes
        sb.Append($"[Guid] {(uint)Random.Shared.Next()} ");
        // This normally comes from boot.config, but there's not much point to generate a value because it appears to just
        // be a way for the Unity editor to identify a version of a build which doesn't really matter to us because we will
        // always deal with one version: the vanilla release one
        sb.Append("[EditorId] 0 ");
        // Hardcoded by Unity as confirmed with Ghidra and Resharper-Unity
        sb.Append("[Version] 1048832 ");
        // This replicates exactly what was seen in Ghidra including the underscore replacements with one exception: Unity
        // normally doesn't put the port here, but rather, it derives it from the [Guid] as explained in Resharper-UNity.
        // While we could do that, it's much simpler to just specify it here
        sb.Append($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{portToUse} ");
        // Since we obviously want IDE to know debugging works, we always want to send 1 here
        sb.Append("[Debug] 1 ");
        // This is hardcoded from Unity as checked in Ghidra
        sb.Append("[PackageName] WindowsPlayer ");
        // This field is interesting because it is not sent in Unity 2018.4.12f1 as it was added later, but IDEs are still
        // made to support this in case. We can exploit this by improving the user experience and advertise the correct product name
        sb.Append("[ProjectName] Bug Fables\0");
        return sb.ToString();
    }

    internal void StartDiscoveryWithOwnSocket(string ipAddress, ushort port)
    {
        _message = ConstructWhoAmIString(IPAddress.Parse(ipAddress), port);
        _messageBuffer = Encoding.ASCII.GetBytes(_message);

        // All these configurations are made to exactly match the configuration of the socket Unity does as confirmed with
        // Ghidra and tcpdump on a dev UnityPlayer.dll
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