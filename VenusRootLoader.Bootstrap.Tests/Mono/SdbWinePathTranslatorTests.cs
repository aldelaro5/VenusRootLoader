using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using VenusRootLoader.Bootstrap.Mono;
using VenusRootLoader.Bootstrap.Shared;
using VenusRootLoader.Bootstrap.Tests.TestHelpers;

namespace VenusRootLoader.Bootstrap.Tests.Mono;

public class SdbWinePathTranslatorTests
{
    private readonly FakeLogger<SdbWinePathTranslator> _logger = new();
    private readonly IWin32 _win32 = Substitute.For<IWin32>();
    private readonly TestPltHookManager _pltHooksManager = new();

    private readonly SdbWinePathTranslator _sut;

    public SdbWinePathTranslatorTests() => _sut = new(_logger, _pltHooksManager, _win32);

    private const int MessageHeaderLength = 11;
    private const byte AssemblyCommandSet = 21;
    private const byte SdbModuleCommandSet = 24;
    private const byte ReplyFlags = 0x80;

    [Fact]
    public void Setup_InstallsHooks_WhenCalled()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";

        _sut.Setup(monoModuleFilename);

        _pltHooksManager.Hooks.Should().ContainKey((monoModuleFilename, nameof(_win32.send)));
        _pltHooksManager.Hooks.Should().ContainKey((monoModuleFilename, nameof(_win32.recv)));
    }

    [Fact]
    public unsafe void HookSend_CallsOriginal_WhenHookRecvWasNotCalled()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";
        string assemblyLocationString = @"Z:\Bug Fables_Data\Managed\Assembly-CSharp.dll";

        byte[] message = BuildAssemblyGetLocationSendPacket(assemblyLocationString);
        byte* messagePtr = stackalloc byte[message.Length];
        PCSTR messagePcStr = new(messagePtr);
        Marshal.Copy(message, 0, (nint)messagePtr, message.Length);

        _sut.Setup(monoModuleFilename);
        _win32.send(default, default, 0, default).ReturnsForAnyArgs(message.Length);

        int result = (int)_pltHooksManager.SimulateHook(
            monoModuleFilename,
            nameof(_win32.send),
            default(SOCKET),
            messagePcStr,
            message.Length,
            default(SEND_RECV_FLAGS))!;

        result.Should().Be(message.Length);
        _win32.Received(1).send(
            default,
            Arg.Is(messagePcStr),
            message.Length,
            default);
    }

    [Fact]
    public unsafe void HookSend_CallsOriginal_WhenPacketReceivedIsTooSmall()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";
        string assemblyLocationString = @"Z:\Bug Fables_Data\Managed\Assembly-CSharp.dll";
        int packetLengthRecv = Random.Shared.Next(1, MessageHeaderLength);

        using var streamRecv = new MemoryStream();
        BinaryWriter writerRecv = new(streamRecv);
        for (int i = 0; i < packetLengthRecv; i++)
            writerRecv.Write((byte)Random.Shared.Next());
        byte[] messageRecv = streamRecv.ToArray();
        byte* messageRecvPtr = stackalloc byte[messageRecv.Length];
        PSTR messageRecvPStr = new(messageRecvPtr);
        Marshal.Copy(messageRecv, 0, (nint)messageRecvPtr, messageRecv.Length);

        byte[] messageSend = BuildAssemblyGetLocationSendPacket(assemblyLocationString);
        byte* messageSendPtr = stackalloc byte[messageSend.Length];
        PCSTR messageSendPStr = new(messageSendPtr);
        Marshal.Copy(messageSend, 0, (nint)messageSendPtr, messageSend.Length);

        _sut.Setup(monoModuleFilename);
        _win32.recv(default, default, 0, default).ReturnsForAnyArgs(packetLengthRecv);
        _win32.send(default, default, 0, default).ReturnsForAnyArgs(messageSend.Length);

        var (resultRecv, resultSend) = SimulateRecvSendHooks(
            monoModuleFilename,
            messageRecvPStr,
            messageRecv,
            messageSendPStr,
            messageSend);

        resultRecv.Should().Be(packetLengthRecv);
        resultSend.Should().Be(messageSend.Length);
        _win32.Received(1).recv(
            default,
            Arg.Is(messageRecvPStr),
            messageRecv.Length,
            default);
        _win32.Received(1).send(
            default,
            Arg.Is(messageSendPStr),
            messageSend.Length,
            default);
    }

    [Fact]
    public unsafe void HookSend_CallsOriginal_WhenPacketReceivedDoesNotHaveCommandOfInterest()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";
        string assemblyLocationString = @"Z:\Bug Fables_Data\Managed\Assembly-CSharp.dll";

        byte[] messageRecv = BuildReceivePacket((byte)Random.Shared.Next(AssemblyCommandSet));
        byte* messageRecvPtr = stackalloc byte[messageRecv.Length];
        PSTR messageRecvPStr = new(messageRecvPtr);
        Marshal.Copy(messageRecv, 0, (nint)messageRecvPtr, messageRecv.Length);

        byte[] messageSend = BuildAssemblyGetLocationSendPacket(assemblyLocationString);
        byte* messageSendPtr = stackalloc byte[messageSend.Length];
        PCSTR messageSendPStr = new(messageSendPtr);
        Marshal.Copy(messageSend, 0, (nint)messageSendPtr, messageSend.Length);

        _sut.Setup(monoModuleFilename);
        _win32.recv(default, default, 0, default).ReturnsForAnyArgs(messageRecv.Length);
        _win32.send(default, default, 0, default).ReturnsForAnyArgs(messageSend.Length);

        var (resultRecv, resultSend) = SimulateRecvSendHooks(
            monoModuleFilename,
            messageRecvPStr,
            messageRecv,
            messageSendPStr,
            messageSend);

        resultRecv.Should().Be(messageRecv.Length);
        resultSend.Should().Be(messageSend.Length);
        _win32.Received(1).recv(
            default,
            Arg.Is(messageRecvPStr),
            messageRecv.Length,
            default);
        _win32.Received(1).send(
            default,
            Arg.Is(messageSendPStr),
            messageSend.Length,
            default);
    }

    [Fact]
    public unsafe void HookSend_CallsOriginalWithModifiedPacket_WhenPacketReceivedAssemblyGetLocation()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";
        string assemblyLocationOriginal = @"Z:\Bug Fables_Data\Managed\Assembly-CSharp.dll";
        string assemblyLocationModified = "/Bug Fables_Data/Managed/Assembly-CSharp.dll";
        
        var messageRecv = BuildReceivePacket(AssemblyCommandSet);
        byte* messageRecvPtr = stackalloc byte[messageRecv.Length];
        PSTR messageRecvPStr = new(messageRecvPtr);
        Marshal.Copy(messageRecv, 0, (nint)messageRecvPtr, messageRecv.Length);

        var messageSendOriginal = BuildAssemblyGetLocationSendPacket(assemblyLocationOriginal);
        byte* messageSendOriginalPtr = stackalloc byte[messageSendOriginal.Length];
        PCSTR messageSendOriginalPStr = new(messageSendOriginalPtr);
        Marshal.Copy(messageSendOriginal, 0, (nint)messageSendOriginalPtr, messageSendOriginal.Length);

        var messageSendModified = BuildAssemblyGetLocationSendPacket(assemblyLocationModified);
        var bytes = new byte[messageSendModified.Length];

        _sut.Setup(monoModuleFilename);
        _win32.recv(default, default, 0, default).ReturnsForAnyArgs(messageRecv.Length);
        _win32.send(default, default, 0, default).ReturnsForAnyArgs(messageSendModified.Length)
            .AndDoes(c => Marshal.Copy((nint)c.ArgAt<PCSTR>(1).Value, bytes, 0, messageSendModified.Length));

        var (resultRecv, resultSend) = SimulateRecvSendHooks(
            monoModuleFilename,
            messageRecvPStr,
            messageRecv,
            messageSendOriginalPStr,
            messageSendOriginal);

        resultRecv.Should().Be(messageRecv.Length);
        resultSend.Should().Be(messageSendModified.Length);
        _win32.Received(1).recv(
            default,
            Arg.Is(messageRecvPStr),
            messageRecv.Length,
            default);
        _win32.Received(1).send(
            default,
            Arg.Any<PCSTR>(),
            messageSendModified.Length,
            default);
        bytes.Should().Equal(messageSendModified);
    }

    [Fact]
    public unsafe void HookSend_CallsOriginalWithModifiedPacket_WhenPacketReceivedModuleGetInfo()
    {
        string monoModuleFilename = "mono-2.0-bdwgc.dll";
        string assemblyLocationOriginal = @"Z:\Bug Fables_Data\Managed\Assembly-CSharp.dll";
        string assemblyLocationModified = "/Bug Fables_Data/Managed/Assembly-CSharp.dll";
        string baseName = "Assembly-CSharp.dll";
        string scopeName = "Assembly-CSharp.dll";
        string guid = Guid.NewGuid().ToString();
        int id = Random.Shared.Next();

        var messageRecv = BuildReceivePacket(SdbModuleCommandSet);
        byte* messageRecvPtr = stackalloc byte[messageRecv.Length];
        PSTR messageRecvPStr = new(messageRecvPtr);
        Marshal.Copy(messageRecv, 0, (nint)messageRecvPtr, messageRecv.Length);

        var messageSendOriginal = BuildModuleGetInfoSendPacket(baseName, scopeName, assemblyLocationOriginal, guid, id);
        byte* messageSendOriginalPtr = stackalloc byte[messageSendOriginal.Length];
        PCSTR messageSendOriginalPStr = new(messageSendOriginalPtr);
        Marshal.Copy(messageSendOriginal, 0, (nint)messageSendOriginalPtr, messageSendOriginal.Length);

        var messageSendModified = BuildModuleGetInfoSendPacket(baseName, scopeName, assemblyLocationModified, guid, id);
        var bytes = new byte[messageSendModified.Length];

        _sut.Setup(monoModuleFilename);
        _win32.recv(default, default, 0, default).ReturnsForAnyArgs(messageRecv.Length);
        _win32.send(default, default, 0, default).ReturnsForAnyArgs(messageSendModified.Length)
            .AndDoes(c => Marshal.Copy((nint)c.ArgAt<PCSTR>(1).Value, bytes, 0, messageSendModified.Length));

        var (resultRecv, resultSend) = SimulateRecvSendHooks(
            monoModuleFilename,
            messageRecvPStr,
            messageRecv,
            messageSendOriginalPStr,
            messageSendOriginal);

        resultRecv.Should().Be(messageRecv.Length);
        resultSend.Should().Be(messageSendModified.Length);
        _win32.Received(1).recv(
            default,
            Arg.Is(messageRecvPStr),
            messageRecv.Length,
            default);
        _win32.Received(1).send(
            default,
            Arg.Any<PCSTR>(),
            messageSendModified.Length,
            default);
        bytes.Should().Equal(messageSendModified);
    }

    private static byte[] BuildReceivePacket(byte commandSet)
    {
        using var streamRecv = new MemoryStream();
        BinaryWriter writerRecv = new(streamRecv);
        writerRecv.Write(BinaryPrimitives.ReverseEndianness(MessageHeaderLength));
        writerRecv.Write(0);
        writerRecv.Write((byte)0);
        writerRecv.Write(commandSet);
        writerRecv.Write((byte)1);
        byte[] messageRecv = streamRecv.ToArray();
        return messageRecv;
    }

    private static byte[] BuildAssemblyGetLocationSendPacket(string fullPath)
    {
        using var streamSend = new MemoryStream();
        BinaryWriter writerSend = new(streamSend);
        writerSend.Write(BinaryPrimitives.ReverseEndianness(MessageHeaderLength + sizeof(int) + fullPath.Length));
        writerSend.Write(0);
        writerSend.Write(ReplyFlags);
        writerSend.Write((byte)0);
        writerSend.Write((byte)0);
        writerSend.Write(BinaryPrimitives.ReverseEndianness(fullPath.Length));
        writerSend.Write(Encoding.ASCII.GetBytes(fullPath));
        byte[] messageSend = streamSend.ToArray();
        return messageSend;
    }

    private static byte[] BuildModuleGetInfoSendPacket(string baseName, string scopeName, string fullName, string guid, int id)
    {
        using var streamSend = new MemoryStream();
        BinaryWriter writerSend = new(streamSend);
        int lenghtPacket = MessageHeaderLength +
                           sizeof(int) +
                           baseName.Length +
                           sizeof(int) +
                           scopeName.Length + 
                           sizeof(int) +
                           fullName.Length + 
                           sizeof(int) +
                           guid.Length +
                           sizeof(int);
        writerSend.Write(BinaryPrimitives.ReverseEndianness(lenghtPacket));
        writerSend.Write(0);
        writerSend.Write(ReplyFlags);
        writerSend.Write((byte)0);
        writerSend.Write((byte)0);
        writerSend.Write(BinaryPrimitives.ReverseEndianness(baseName.Length));
        writerSend.Write(Encoding.ASCII.GetBytes(baseName));
        writerSend.Write(BinaryPrimitives.ReverseEndianness(scopeName.Length));
        writerSend.Write(Encoding.ASCII.GetBytes(scopeName));
        writerSend.Write(BinaryPrimitives.ReverseEndianness(fullName.Length));
        writerSend.Write(Encoding.ASCII.GetBytes(fullName));
        writerSend.Write(BinaryPrimitives.ReverseEndianness(guid.Length));
        writerSend.Write(Encoding.ASCII.GetBytes(guid));
        writerSend.Write(BinaryPrimitives.ReverseEndianness(id));
        byte[] messageSend = streamSend.ToArray();
        return messageSend;
    }

    private (int resultRecv, int resultSend) SimulateRecvSendHooks(
        string monoModuleFilename,
        PSTR messageRecvPStr,
        byte[] messageRecv,
        PCSTR messageSendPStr,
        byte[] messageSend)
    {
        int resultRecv = (int)_pltHooksManager.SimulateHook(
            monoModuleFilename,
            nameof(_win32.recv),
            default(SOCKET),
            messageRecvPStr,
            messageRecv.Length,
            default(SEND_RECV_FLAGS))!;
        int resultSend = (int)_pltHooksManager.SimulateHook(
            monoModuleFilename,
            nameof(_win32.send),
            default(SOCKET),
            messageSendPStr,
            messageSend.Length,
            default(SEND_RECV_FLAGS))!;
        return (resultRecv, resultSend);
    }
}