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
        var bytes = new byte[len];
        Marshal.Copy((nint)buf.Value, bytes, 0, len);

        if (_lastSetCommandWithFilePath.Set == byte.MaxValue)
            return PInvoke.send(s, buf, len, flags);

        if (_lastSetCommandWithFilePath == ModuleGetInfo)
        {
            var modifiedBytes = new byte[len - 2];

            var fullNameIndex = SdbMessageHeaderLength;
            var lengthBaseName = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(bytes, fullNameIndex));
            fullNameIndex += 4;
            fullNameIndex += lengthBaseName;
            var lengthScopeName = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(bytes, fullNameIndex));
            fullNameIndex += 4;
            fullNameIndex += lengthScopeName;
            var lengthFullName = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(bytes, fullNameIndex));
            fullNameIndex += 4;
            var chars = Encoding.ASCII.GetString(bytes, fullNameIndex, lengthFullName);
            chars = chars.Substring(2).Replace('\\', '/');

            BinaryPrimitives.WriteInt32BigEndian(modifiedBytes.AsSpan(0, 4), modifiedBytes.Length);
            Buffer.BlockCopy(bytes, 4, modifiedBytes, 4, fullNameIndex - 4 - 4);
            BinaryPrimitives.WriteInt32BigEndian(modifiedBytes.AsSpan(fullNameIndex - 4, 4), chars.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(chars), 0, modifiedBytes, fullNameIndex, chars.Length);
            Buffer.BlockCopy(bytes, fullNameIndex + chars.Length + 2, modifiedBytes, fullNameIndex + chars.Length,
                bytes.Length - lengthFullName - fullNameIndex);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                PrintPacket("SEND", bytes);
                PrintPacket("SEND", modifiedBytes);
            }

            _lastSetCommandWithFilePath.Set = byte.MaxValue;
            _lastSetCommandWithFilePath.Id = byte.MaxValue;
            fixed (byte* bufPtr = modifiedBytes)
            {
                return PInvoke.send(s, new(bufPtr), len - 2, flags);
            }
        }

        if (_lastSetCommandWithFilePath == AssemblyGetLocation)
        {
            var modifiedBytes = new byte[len - 2];

            var lengthFullName = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(bytes, 11));
            var chars = Encoding.ASCII.GetString(bytes, SdbMessageHeaderLength + 4, lengthFullName);
            chars = chars.Substring(2).Replace('\\', '/');

            BinaryPrimitives.WriteInt32BigEndian(modifiedBytes.AsSpan(0, 4), modifiedBytes.Length);
            Buffer.BlockCopy(bytes, 4, modifiedBytes, 4, SdbMessageHeaderLength - 4);
            BinaryPrimitives.WriteInt32BigEndian(modifiedBytes.AsSpan(SdbMessageHeaderLength, 4), chars.Length);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(chars), 0, modifiedBytes, SdbMessageHeaderLength + 4, chars.Length);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                PrintPacket("SEND", bytes);
                PrintPacket("SEND", modifiedBytes);
            }

            _lastSetCommandWithFilePath.Set = byte.MaxValue;
            _lastSetCommandWithFilePath.Id = byte.MaxValue;
            fixed (byte* bufPtr = modifiedBytes)
                return PInvoke.send(s, new(bufPtr), len - 2, flags);
        }
        _lastSetCommandWithFilePath.Set = byte.MaxValue;
        _lastSetCommandWithFilePath.Id = byte.MaxValue;
        return PInvoke.send(s, buf, len, flags);
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