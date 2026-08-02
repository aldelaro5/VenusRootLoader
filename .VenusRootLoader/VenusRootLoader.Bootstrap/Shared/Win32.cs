using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;

namespace VenusRootLoader.Bootstrap.Shared;

/// <summary>
/// The real implementation of <see cref="IWin32"/> that calls the real functions with PInvoke
/// </summary>
internal sealed class Win32 : IWin32
{
    public FARPROC GetProcAddress(HMODULE hModule, PCSTR lpProcName) => PInvoke.GetProcAddress(hModule, lpProcName);
    public HANDLE GetStdHandle(STD_HANDLE nStdHandle) => PInvoke.GetStdHandle(nStdHandle);

    public unsafe HANDLE CreateFile(
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        Pointer<SECURITY_ATTRIBUTES> lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile) => PInvoke.CreateFile(
        lpFileName,
        dwDesiredAccess,
        dwShareMode,
        lpSecurityAttributes.Value,
        dwCreationDisposition,
        dwFlagsAndAttributes,
        hTemplateFile);

    public unsafe BOOL ReadFile(
        HANDLE hFile,
        Pointer<byte> lpBuffer,
        uint nNumberOfBytesToRead,
        Pointer<uint> lpNumberOfBytesRead,
        Pointer<NativeOverlapped> lpOverlapped) => PInvoke.ReadFile(
        hFile,
        lpBuffer.Value,
        nNumberOfBytesToRead,
        lpNumberOfBytesRead.Value,
        lpOverlapped.Value);

    public unsafe BOOL WriteFile(
        HANDLE hFile,
        Pointer<byte> lpBuffer,
        uint nNumberOfBytesToWrite,
        Pointer<uint> lpNumberOfBytesWritten,
        Pointer<NativeOverlapped> lpOverlapped) => PInvoke.WriteFile(
        hFile,
        lpBuffer.Value,
        nNumberOfBytesToWrite,
        lpNumberOfBytesWritten.Value,
        lpOverlapped.Value);

    public BOOL CloseHandle(HANDLE hObject) => PInvoke.CloseHandle(hObject);

    public unsafe BOOL SetFilePointerEx(
        HANDLE hFile,
        long liDistanceToMove,
        Pointer<long> lpNewFilePointer,
        SET_FILE_POINTER_MOVE_METHOD dwMoveMethod) => PInvoke.SetFilePointerEx(
        hFile,
        liDistanceToMove,
        lpNewFilePointer.Value,
        dwMoveMethod);

    public unsafe int sendto(SOCKET s, PCSTR buf, int len, int flags, Pointer<SOCKADDR> to, int toLen) =>
        PInvoke.sendto(s, buf, len, flags, to.Value, toLen);

    public uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize) =>
        PInvoke.GetModuleFileName(hModule, lpFilename, nSize);

    public int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags) => PInvoke.send(s, buf, len, flags);
    public int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags) => PInvoke.recv(s, buf, len, flags);

    public unsafe BOOL GetConsoleMode(HANDLE hConsoleHandle, Pointer<CONSOLE_MODE> lpMode) =>
        PInvoke.GetConsoleMode(hConsoleHandle, lpMode.Value);

    public BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode) =>
        PInvoke.SetConsoleMode(hConsoleHandle, dwMode);

    public BOOL CompareObjectHandles(HANDLE hFirstObjectHandle, HANDLE hSecondObjectHandle) =>
        PInvoke.CompareObjectHandles(hFirstObjectHandle, hSecondObjectHandle);

    public BOOL PathFileExists(PCWSTR pszPath) => PInvoke.PathFileExists(pszPath);

    public unsafe BOOL GetFileAttributesExW(
        PCWSTR lpFileName,
        GET_FILEEX_INFO_LEVELS fInfoLevelId,
        void* lpFileInformation) => PInvoke.GetFileAttributesEx(lpFileName, fInfoLevelId, lpFileInformation);

    public unsafe string? WineGetUnixFileName(string dosW)
    {
        fixed (char* lpProcNameLocal = dosW)
        {
            Marshal.SetLastSystemError(0);
            char* retVal = LocalExternFunction(lpProcNameLocal);
            Marshal.SetLastPInvokeError(Marshal.GetLastSystemError());

            nint ptr = new(retVal);
            string? ptrToStringUTF8 = Marshal.PtrToStringUTF8(ptr);
            if (ptrToStringUTF8 is null)
                return null;

            // The documentation in Wine's source code says the caller needs to free the buffer.
            NativeMemory.Free(retVal);
            return ptrToStringUTF8;
        }

        [DllImport("KERNEL32.dll", ExactSpelling = true, EntryPoint = "wine_get_unix_file_name"),
         DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern char* LocalExternFunction(PCWSTR lpModuleName);
    }
}