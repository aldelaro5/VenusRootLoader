using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;
using VenusRootLoader.Bootstrap.Unity;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;

namespace VenusRootLoader.Bootstrap.Tests.Unity;

[Collection(nameof(PlayerConnectionDiscoveryTests))]
public sealed class PlayerConnectionDiscoveryTests
{
    private readonly ILogger<PlayerConnectionDiscovery> _logger = Substitute.For<ILogger<PlayerConnectionDiscovery>>();
    private readonly TestPltHookManager _pltHooksManager = new();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();

    private readonly GameExecutionContext _gameExecutionContext = new()
    {
        GameDir = "",
        DataDir = "",
        UnityPlayerDllFileName = "UnityPlayer.dll",
        IsWine = false
    };

    public PlayerConnectionDiscoveryTests()
    {
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", null);
    }

    [Fact]
    public void StartDiscoveryWithOwnSocket_OpensUdpSocketAndSendCorrectMessageEverySecond_WhenCalled()
    {
        var broadcastIp = IPAddress.Parse("225.0.0.222");
        using var client = new UdpClient(54997);
        client.Client.ReceiveTimeout = 2000;
        client.JoinMulticastGroup(broadcastIp);
        var remoteEp = new IPEndPoint(broadcastIp, 0);
        var ip =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var port = (ushort)Random.Shared.Next();

        using var sut = new PlayerConnectionDiscovery(_logger, _pltHooksManager, _gameExecutionContext, _win32);
        sut.StartDiscoveryWithOwnSocket(ip, port);

        for (int i = 0; i < 3; i++)
        {
            var messageBytes = client.Receive(ref remoteEp);
            messageBytes.Length.Should().BeGreaterThan(0);
            var message = Encoding.UTF8.GetString(messageBytes);

            message.Should().StartWith($"[IP] {ip} ");
            message.Should().MatchRegex($".*{Regex.Escape("[Port]")} 55[0-5][0-9][0-9].* ");
            message.Should().Contain("[Flags] 0 ");
            message.Should().MatchRegex($".*{Regex.Escape("[Guid]")} [0-4]{{0,1}}[0-9]{{1,9}}.* ");
            message.Should().Contain("[EditorId] 0 ");
            message.Should().Contain("[Version] 1048832 ");
            message.Should().Contain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{port} ");
            message.Should().Contain("[Debug] 1 ");
            message.Should().Contain("[PackageName] WindowsPlayer ");
            message.Should().EndWith("[ProjectName] Bug Fables\0");
        }
    }

    [Fact]
    public void StartDiscoveryWithOwnSocket_SendsMessageWithDnSpyDebugConfig_WhenCalledWithDnSpyEnvironmentVariableSet()
    {
        var broadcastIp = IPAddress.Parse("225.0.0.222");
        using var client = new UdpClient(54997);
        client.Client.ReceiveTimeout = 2000;
        client.JoinMulticastGroup(broadcastIp);
        var remoteEp = new IPEndPoint(broadcastIp, 0);
        var ip =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var port = (ushort)Random.Shared.Next();
        var dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var dnSpyPort = (ushort)Random.Shared.Next();
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", $"stuff,address={dnSpyIp}:{dnSpyPort},things");

        using var sut = new PlayerConnectionDiscovery(_logger, _pltHooksManager, _gameExecutionContext, _win32);
        sut.StartDiscoveryWithOwnSocket(ip, port);

        var messageBytes = client.Receive(ref remoteEp);
        messageBytes.Length.Should().BeGreaterThan(0);
        var message = Encoding.UTF8.GetString(messageBytes);

        message.Should().StartWith($"[IP] {dnSpyIp} ");
        message.Should().NotStartWith($"[IP] {ip} ");
        message.Should().Contain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{dnSpyPort} ");
        message.Should().NotContain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{port} ");
    }

    [Fact]
    public unsafe void StartDiscoveryWithSendToHook_OverridesMessageToSend_WhenSendToIsCalled()
    {
        var ip =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var port = (ushort)Random.Shared.Next();
        PCSTR receivedBuffer = default;
        int receivedLength = 0;
        _win32.sendto(
                Arg.Any<SOCKET>(),
                Arg.Any<PCSTR>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<Pointer<SOCKADDR>>(),
                Arg.Any<int>())
            .ReturnsForAnyArgs(1)
            .AndDoes(c =>
            {
                receivedBuffer = c.ArgAt<PCSTR>(1);
                receivedLength = c.ArgAt<int>(2);
            });

        using var sut = new PlayerConnectionDiscovery(_logger, _pltHooksManager, _gameExecutionContext, _win32);
        sut.StartDiscoveryWithSendToHook(ip, port);
        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.sendto),
            default(SOCKET),
            default(PCSTR),
            0,
            0,
            null,
            0)!;

        result.Should().Be(1);
        receivedLength.Should().NotBe(0);
        receivedBuffer.Should().NotBe(default(PCSTR));
        var message = Marshal.PtrToStringAnsi((nint)receivedBuffer.Value, receivedLength);

        message.Should().StartWith($"[IP] {ip} ");
        message.Should().MatchRegex($".*{Regex.Escape("[Port]")} 55[0-5][0-9][0-9].* ");
        message.Should().Contain("[Flags] 0 ");
        message.Should().MatchRegex($".*{Regex.Escape("[Guid]")} [0-4]{{0,1}}[0-9]{{1,9}}.* ");
        message.Should().Contain("[EditorId] 0 ");
        message.Should().Contain("[Version] 1048832 ");
        message.Should().Contain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{port} ");
        message.Should().Contain("[Debug] 1 ");
        message.Should().Contain("[PackageName] WindowsPlayer ");
        message.Should().EndWith("[ProjectName] Bug Fables\0");
    }

    [Fact]
    public unsafe void
        StartDiscoveryWithSendToHook_OverridesMessageToSendWithDnSpyDebugConfig_WhenCalledWithDnSpyEnvironmentVariableSet()
    {
        var ip =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var port = (ushort)Random.Shared.Next();
        var dnSpyIp =
            $"{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}.{(byte)Random.Shared.Next()}";
        var dnSpyPort = (ushort)Random.Shared.Next();
        Environment.SetEnvironmentVariable("DNSPY_UNITY_DBG2", $"stuff,address={dnSpyIp}:{dnSpyPort},things");
        PCSTR receivedBuffer = default;
        int receivedLength = 0;
        _win32.sendto(
                Arg.Any<SOCKET>(),
                Arg.Any<PCSTR>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<Pointer<SOCKADDR>>(),
                Arg.Any<int>())
            .ReturnsForAnyArgs(1)
            .AndDoes(c =>
            {
                receivedBuffer = c.ArgAt<PCSTR>(1);
                receivedLength = c.ArgAt<int>(2);
            });

        using var sut = new PlayerConnectionDiscovery(_logger, _pltHooksManager, _gameExecutionContext, _win32);
        sut.StartDiscoveryWithSendToHook(ip, port);
        var result = (int)_pltHooksManager.SimulateHook(
            _gameExecutionContext.UnityPlayerDllFileName,
            nameof(_win32.sendto),
            default(SOCKET),
            default(PCSTR),
            0,
            0,
            null,
            0)!;

        result.Should().Be(1);
        receivedLength.Should().NotBe(0);
        receivedBuffer.Should().NotBe(default(PCSTR));
        var message = Marshal.PtrToStringAnsi((nint)receivedBuffer.Value, receivedLength);

        message.Should().StartWith($"[IP] {dnSpyIp} ");
        message.Should().NotStartWith($"[IP] {ip} ");
        message.Should().Contain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{dnSpyPort} ");
        message.Should().NotContain($"[Id] WindowsPlayer({Dns.GetHostName().Replace(' ', '_')}):{port} ");
    }
}