using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using VenusRootLoader.Bootstrap.Shared;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;

namespace VenusRootLoader.Bootstrap.Mono;

public interface ISdbWinePathTranslator
{
    void Setup(string monoModuleFilename);
}

/// <summary>
/// <para>
/// This service addresses a problem specific to Mono's debugger (aka SDB) when the game is running under Wine. Specifically,
/// it aims to translate Wine file paths of assemblies sent by Mono to the IDE into valid Linux paths. This solves issues
/// on some client such as Rider's Unity support plugin which concludes that the assemblies it receives don't exist on disk
/// because it cannot interpret Wine paths on Linux.
/// </para>
/// <para>
/// To achieve this, 2 plthooks are installed on Mono's module: send and recv which are the calls done to send and receive
/// data from a socket. We are essentially acting as a middleman between Mono and the IDE since we can see what Mono receives
/// and accordingly edit the packets sent as reply. The aim is to modify the reply of 2 commands since they are the only ones
/// that contains assembly file paths: ASSEMBLY(GET_LOCATION) and MODULE(GET_INFO), but keep everything else intact. Since
/// we need to truncate the drive letter and its colon, it implies that we need to modify the packet length in the header,
/// and it also implies we need to change the length of the string in the packet.
/// </para>
/// <para>
/// For more information on the SDB protocol, consult its documentation available here: https://www.mono-project.com/docs/advanced/runtime/docs/soft-debugger-wire-format/
/// </para>
/// </summary>
public sealed class SdbWinePathTranslator : ISdbWinePathTranslator
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate int SendFn(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);

    private static SendFn _hookSendFnDelegate = null!;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private delegate int RecvFn(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);

    private static RecvFn _hookRecvFnDelegate = null!;

    private readonly IWin32 _win32;
    private readonly IPltHooksManager _pltHooksManager;
    private readonly ILogger<SdbWinePathTranslator> _logger;

    private const int MessageHeaderLength = 11;
    private const int PacketIdIndex = 4;
    private const int CommandSetByteIndex = 9;
    private const int CommandIdByteIndex = 10;
    private const byte AssemblyCommandSet = 21;
    private const byte SdbModuleCommandSet = 24;

    private record struct SdbSetCommand(byte Set, byte Id);

    private static readonly SdbSetCommand CommandAssemblyGetLocation = new(AssemblyCommandSet, 1);
    private static readonly SdbSetCommand CommandModuleGetInfo = new(SdbModuleCommandSet, 1);

    private readonly Dictionary<int, SdbSetCommand> _setCommandsToTranslateReply = new();

    public SdbWinePathTranslator(
        ILogger<SdbWinePathTranslator> logger,
        IPltHooksManager pltHooksManager,
        IWin32 win32)
    {
        _pltHooksManager = pltHooksManager;
        _win32 = win32;
        _logger = logger;
        _hookSendFnDelegate = HookSendFnDelegate;
        _hookRecvFnDelegate = HookRecvFnDelegate;
    }

    public void Setup(string monoModuleFilename)
    {
        _pltHooksManager.InstallHook(monoModuleFilename, nameof(_win32.send), _hookSendFnDelegate);
        _pltHooksManager.InstallHook(monoModuleFilename, nameof(_win32.recv), _hookRecvFnDelegate);
    }

    private unsafe int HookRecvFnDelegate(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags)
    {
        int length = _win32.recv(s, buf, len, flags);
        if (length < MessageHeaderLength)
            return length;

        SdbSetCommand ret = new(buf.Value[CommandSetByteIndex], buf.Value[CommandIdByteIndex]);
        if (ret != CommandAssemblyGetLocation && ret != CommandModuleGetInfo)
            return length;

        int id = BinaryPrimitives.ReverseEndianness(Marshal.ReadInt32((nint)buf.Value, PacketIdIndex));
        _setCommandsToTranslateReply.Add(id, ret);

        if (!_logger.IsEnabled(LogLevel.Trace))
            return length;

        byte[] bytes = new byte[length];
        Marshal.Copy((nint)buf.Value, bytes, 0, length);
        PrintPacket("RECV", bytes);

        return length;
    }

    private unsafe int HookSendFnDelegate(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags)
    {
        if (_setCommandsToTranslateReply.Count == 0 || len < MessageHeaderLength)
            return _win32.send(s, buf, len, flags);
        int id = BinaryPrimitives.ReverseEndianness(Marshal.ReadInt32((nint)buf.Value, PacketIdIndex));
        if (!_setCommandsToTranslateReply.Remove(id, out SdbSetCommand sdbSetCommand))
            return _win32.send(s, buf, len, flags);

        bool isGetInfo = sdbSetCommand == CommandModuleGetInfo;

        // For an assembly get location, the path is the only data while for a module get info, it's the third string
        // in the packet (it is preceded by the image basename and the scope name which we don't need to touch)
        (int Length, int StartIndex) fullNameStringInfo = GetStringInfoInPacketData(buf, isGetInfo ? 2 : 0);

        // Converts the Wine path into a Unix path.
        string winePath = Encoding.ASCII.GetString(
            buf.Value + fullNameStringInfo.StartIndex,
            fullNameStringInfo.Length);

        string? unixPath = _win32.WineGetUnixFileName(winePath);
        if (unixPath == null)
            return _win32.send(s, buf, len, flags);

        int lengthNewPacket = len - winePath.Length + unixPath.Length;
        nint modifiedBytesPtr = Marshal.AllocHGlobal(lengthNewPacket);

        // All int in the packet must be in big endian and since we are changing the length of the packet, we need to write
        // that new length at the start of the header
        Marshal.WriteInt32(modifiedBytesPtr, BinaryPrimitives.ReverseEndianness(lengthNewPacket));

        // Copies all data we don't need to touch that sits between the packet length and the path we want to edit
        Buffer.MemoryCopy(
            buf.Value + sizeof(int),
            (void*)(modifiedBytesPtr + sizeof(int)),
            fullNameStringInfo.StartIndex - sizeof(int) * 2,
            fullNameStringInfo.StartIndex - sizeof(int) * 2);

        // Write the new length of the string first which is needed because all strings are its length followed by the content
        // (without a null termination needed)
        Marshal.WriteInt32(
            modifiedBytesPtr + fullNameStringInfo.StartIndex - sizeof(int),
            BinaryPrimitives.ReverseEndianness(unixPath.Length));

        // Finally, write the new string
        Marshal.Copy(
            Encoding.ASCII.GetBytes(unixPath),
            0,
            modifiedBytesPtr + fullNameStringInfo.StartIndex,
            unixPath.Length);

        // Since module get info contains data after the path, we need to copy the data over so we don't touch it
        if (isGetInfo)
        {
            Buffer.MemoryCopy(
                buf.Value + fullNameStringInfo.StartIndex + fullNameStringInfo.Length,
                (void*)(modifiedBytesPtr + fullNameStringInfo.StartIndex + unixPath.Length),
                len - fullNameStringInfo.Length - fullNameStringInfo.StartIndex,
                len - fullNameStringInfo.Length - fullNameStringInfo.StartIndex);
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            byte[] bytes = new byte[len];
            Marshal.Copy((nint)buf.Value, bytes, 0, len);
            byte[] modifiedBytes = new byte[lengthNewPacket];
            Marshal.Copy(modifiedBytesPtr, modifiedBytes, 0, lengthNewPacket);
            PrintPacket("SEND-ORIG", bytes);
            PrintPacket("SEND-EDIT", modifiedBytes);
        }

        int result = _win32.send(s, new((byte*)modifiedBytesPtr), lengthNewPacket, flags);
        Marshal.FreeHGlobal(modifiedBytesPtr);
        return result;
    }

    /// <summary>
    /// Reads the contents of the packet assuming it is a sequence of strings and returns the length / index of the
    /// string in the packet given its position in the sequence
    /// </summary>
    /// <param name="packet">The pointer to the buffer of the entire packet</param>
    /// <param name="stringIndexInData">The 0 based index of the string in the sequence</param>
    /// <returns>A tuple containing the length and index of the string</returns>
    private static unsafe (int Length, int StartIndex) GetStringInfoInPacketData(PCSTR packet, int stringIndexInData)
    {
        int index = MessageHeaderLength;
        int length = 0;
        for (int i = 0; i <= stringIndexInData; i++)
        {
            index += length;
            // All int in the packet must be in big endian and all strings are its length followed by the content
            // (without a null termination needed)
            length = BinaryPrimitives.ReverseEndianness(Marshal.ReadInt32((nint)packet.Value, index));
            index += 4;
        }

        return (length, index);
    }

    private void PrintPacket(string prefix, byte[] modifiedBytes)
    {
        StringBuilder sbBin = new();
        foreach (byte b in modifiedBytes)
        {
            sbBin.Append(b.ToString("X2"));
        }

        _logger.LogTrace("{prefix}(BIN): {packetBin}", prefix, sbBin.ToString());
        string ascii = Encoding.ASCII.GetString(modifiedBytes);
        StringBuilder sbAscii = new();
        foreach (char b in ascii)
        {
            sbAscii.Append(' ');
            sbAscii.Append(b);
        }

        _logger.LogTrace("{prefix}(STR): {packet}", prefix, sbAscii.ToString());
    }
}