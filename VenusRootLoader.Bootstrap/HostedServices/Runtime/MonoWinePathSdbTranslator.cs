using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Microsoft.Extensions.Logging;
using VenusRootLoader.Bootstrap.Services;

namespace VenusRootLoader.Bootstrap.HostedServices.Runtime;

public class MonoWinePathSdbTranslator
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate int SendFn(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);
    private static SendFn _hookSendFnDelegate = null!;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate int RecvFn(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);
    private static RecvFn _hookRecvFnDelegate = null!;

    private readonly PltHook _pltHook;
    private readonly ILogger<MonoWinePathSdbTranslator> _logger;

    private const int SdbMessageHeaderLength = 11;
    private const int SdbCommandSetByteIndex = 9;
    private const int SdbCommandIdByteIndex = 10;
    private const byte SdbAssemblyCommandSet = 21;
    private const byte SdbDebuggerModuleCommandSet = 24;

    private record struct SdbSetCommand(byte Set, byte Id);

    private static readonly SdbSetCommand AssemblyGetLocation = new(SdbAssemblyCommandSet, 1);
    private static readonly SdbSetCommand ModuleGetInfo = new(SdbDebuggerModuleCommandSet, 1);
    private SdbSetCommand _lastSetCommandWithFilePath = new(byte.MaxValue, byte.MaxValue);

    public MonoWinePathSdbTranslator(
        ILogger<MonoWinePathSdbTranslator> logger,
        PltHook pltHook)
    {
        _pltHook = pltHook;
        _logger = logger;
        _hookSendFnDelegate = HookSendFnDelegate;
        _hookRecvFnDelegate = HookRecvFnDelegate;
    }

    public void Setup(string monoModuleFilename)
    {
        _pltHook.InstallHook(monoModuleFilename, nameof(PInvoke.send), Marshal.GetFunctionPointerForDelegate(_hookSendFnDelegate));
        _pltHook.InstallHook(monoModuleFilename, nameof(PInvoke.recv), Marshal.GetFunctionPointerForDelegate(_hookRecvFnDelegate));
    }

    private unsafe int HookRecvFnDelegate(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags)
    {
        var length = PInvoke.recv(s, buf, len, flags);
        if (length < SdbMessageHeaderLength)
            return length;

        SdbSetCommand ret = new(buf.Value[SdbCommandSetByteIndex], buf.Value[SdbCommandIdByteIndex]);
        if (ret != AssemblyGetLocation && ret != ModuleGetInfo)
            return length;

        _lastSetCommandWithFilePath = ret;

        if (!_logger.IsEnabled(LogLevel.Trace))
            return length;

        var bytes = new byte[length];
        Marshal.Copy((nint)buf.Value, bytes, 0, length);
        PrintPacket("RECV", bytes);

        return length;
    }

    private unsafe int HookSendFnDelegate(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags)
    {
        if (_lastSetCommandWithFilePath.Set == byte.MaxValue)
            return PInvoke.send(s, buf, len, flags);

        var modifiedBytesPtr = Marshal.AllocHGlobal(len - 2);
        if (_lastSetCommandWithFilePath == ModuleGetInfo)
        {
            var fullNameStringInfo = GetStringInfoInPacketData(buf, 2);
            var chars = Encoding.ASCII.GetString(buf.Value + fullNameStringInfo.Index, fullNameStringInfo.Length);
            chars = chars.Substring(2).Replace('\\', '/');

            Marshal.WriteInt32(modifiedBytesPtr, BinaryPrimitives.ReverseEndianness(len - 2));
            Buffer.MemoryCopy(buf.Value + 4, (void*)(modifiedBytesPtr + 4), fullNameStringInfo.Index - 4 - 4, fullNameStringInfo.Index - 4 - 4);
            Marshal.WriteInt32(modifiedBytesPtr + fullNameStringInfo.Index - 4, BinaryPrimitives.ReverseEndianness(chars.Length));
            Marshal.Copy(Encoding.ASCII.GetBytes(chars), 0, modifiedBytesPtr + fullNameStringInfo.Index, chars.Length);
            Buffer.MemoryCopy(
                buf.Value + fullNameStringInfo.Index + chars.Length + 2,
                (void*)(modifiedBytesPtr + fullNameStringInfo.Index + chars.Length),
                len - fullNameStringInfo.Length - fullNameStringInfo.Index,
                len - fullNameStringInfo.Length - fullNameStringInfo.Index);
        }
        else
        {
            var lengthFullName = GetStringInfoInPacketData(buf, 0).Length;
            var chars = Encoding.ASCII.GetString(buf.Value + SdbMessageHeaderLength + 4, lengthFullName);
            chars = chars.Substring(2).Replace('\\', '/');

            Marshal.WriteInt32(modifiedBytesPtr, BinaryPrimitives.ReverseEndianness(len - 2));
            Buffer.MemoryCopy(buf.Value + 4, (void*)(modifiedBytesPtr + 4), SdbMessageHeaderLength - 4, SdbMessageHeaderLength - 4);
            Marshal.WriteInt32(modifiedBytesPtr + SdbMessageHeaderLength, BinaryPrimitives.ReverseEndianness(chars.Length));
            Marshal.Copy(Encoding.ASCII.GetBytes(chars), 0, modifiedBytesPtr + SdbMessageHeaderLength + 4, chars.Length);
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            var bytes = new byte[len];
            Marshal.Copy((nint)buf.Value, bytes, 0, len);
            var modifiedBytes = new byte[len - 2];
            Marshal.Copy(modifiedBytesPtr, modifiedBytes, 0, len - 2);
            PrintPacket("SEND-ORIG", bytes);
            PrintPacket("SEND-EDIT", modifiedBytes);
        }

        _lastSetCommandWithFilePath.Set = byte.MaxValue;
        _lastSetCommandWithFilePath.Id = byte.MaxValue;
        var result = PInvoke.send(s, new((byte*)modifiedBytesPtr), len - 2, flags);

        Marshal.FreeHGlobal(modifiedBytesPtr);
        return result;
    }

    private static unsafe (int Length, int Index) GetStringInfoInPacketData(PCSTR packet, uint stringIndexInData)
    {
        var index = SdbMessageHeaderLength;
        var length = 0;
        for (var i = 0; i <= stringIndexInData; i++)
        {
            index += length;
            length = BinaryPrimitives.ReverseEndianness(Marshal.ReadInt32((nint)packet.Value, index));
            index += 4;
        }
        return (length, index);
    }

    private void PrintPacket(string prefix, byte[] modifiedBytes)
    {
        var stringBuilder = new StringBuilder();
        foreach (var b in modifiedBytes)
        {
            stringBuilder.Append(b.ToString("X2"));
        }
        _logger.LogTrace("{prefix}(BIN): {packet}", prefix, stringBuilder.ToString());
        var chars2 = Encoding.ASCII.GetString(modifiedBytes);
        var stringBuilder2 = new StringBuilder();
        foreach (var b in chars2)
        {
            stringBuilder2.Append(' ');
            stringBuilder2.Append(b);
        }
        _logger.LogTrace("{prefix}(STR): {packet}", prefix, stringBuilder2.ToString());
    }
}