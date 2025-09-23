using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Windows.Win32.UI.WindowsAndMessaging;

namespace VenusRootLoader.Bootstrap.Shared;

internal class Win32 : IWin32
{
    public FreeLibrarySafeHandle GetModuleHandle(string lpModuleName) => PInvoke.GetModuleHandle(lpModuleName);
    public FARPROC GetProcAddress(HMODULE hModule, PCSTR lpProcName) => PInvoke.GetProcAddress(hModule, lpProcName);
    public HANDLE GetStdHandle(STD_HANDLE nStdHandle) => PInvoke.GetStdHandle(nStdHandle);
    public unsafe HANDLE CreateFile(PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, Pointer<SECURITY_ATTRIBUTES> lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile) => PInvoke.CreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes.Value, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    public unsafe BOOL ReadFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToRead, Pointer<uint> lpNumberOfBytesRead, Pointer<NativeOverlapped> lpOverlapped) => PInvoke.ReadFile(hFile, lpBuffer.Value, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);
    public unsafe BOOL WriteFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToWrite, Pointer<uint> lpNumberOfBytesWritten, Pointer<NativeOverlapped> lpOverlapped) => PInvoke.WriteFile(hFile, lpBuffer.Value, nNumberOfBytesToWrite, lpNumberOfBytesWritten.Value, lpOverlapped.Value);
    public BOOL CloseHandle(HANDLE hObject) => PInvoke.CloseHandle(hObject);
    public MESSAGEBOX_RESULT MessageBox(HWND hWnd, string lpText, string lpCaption, MESSAGEBOX_STYLE uType) => PInvoke.MessageBox(hWnd, lpText, lpCaption, uType);
    public BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow) => PInvoke.ShowWindow(hWnd, nCmdShow);
    public HWND GetConsoleWindow() => PInvoke.GetConsoleWindow();
    public unsafe BOOL SetFilePointerEx(HANDLE hFile, long liDistanceToMove, Pointer<long> lpNewFilePointer, SET_FILE_POINTER_MOVE_METHOD dwMoveMethod) => PInvoke.SetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer.Value, dwMoveMethod);
    public unsafe int sendto(SOCKET s, PCSTR buf, int len, int flags, Pointer<SOCKADDR> to, int toLen) => PInvoke.sendto(s, buf, len, flags, to.Value, toLen);
    public uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize) => PInvoke.GetModuleFileName(hModule, lpFilename, nSize);
    public int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags) => PInvoke.send(s, buf, len, flags);
    public int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags) => PInvoke.recv(s, buf, len, flags);
    public unsafe BOOL GetConsoleMode(HANDLE hConsoleHandle, Pointer<CONSOLE_MODE> lpMode) => PInvoke.GetConsoleMode(hConsoleHandle, lpMode.Value);
    public BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode) => PInvoke.SetConsoleMode(hConsoleHandle, dwMode);
}