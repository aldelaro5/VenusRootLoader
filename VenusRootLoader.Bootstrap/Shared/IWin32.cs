using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WinSock;
using Windows.Win32.Security;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Windows.Win32.UI.WindowsAndMessaging;

// ReSharper disable InconsistentNaming

namespace VenusRootLoader.Bootstrap.Shared;

public interface IWin32
{
    internal FreeLibrarySafeHandle GetModuleHandle(string lpModuleName);
    internal FARPROC GetProcAddress(HMODULE hModule, PCSTR lpProcName);
    internal HANDLE GetStdHandle(STD_HANDLE nStdHandle);
    internal HANDLE CreateFile(PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, Pointer<SECURITY_ATTRIBUTES> lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile);
    internal BOOL ReadFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToRead, Pointer<uint> lpNumberOfBytesRead, Pointer<NativeOverlapped> lpOverlapped);
    internal BOOL WriteFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToWrite, Pointer<uint> lpNumberOfBytesWritten, Pointer<NativeOverlapped> lpOverlapped);
    internal BOOL CloseHandle(HANDLE hObject);
    internal MESSAGEBOX_RESULT MessageBox(HWND hWnd, string lpText, string lpCaption, MESSAGEBOX_STYLE uType);
    internal BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow);
    internal HWND GetConsoleWindow();
    internal BOOL SetFilePointerEx(HANDLE hFile, long liDistanceToMove, Pointer<long> lpNewFilePointer, SET_FILE_POINTER_MOVE_METHOD dwMoveMethod);
    internal int sendto(SOCKET s, PCSTR buf, int len, int flags, Pointer<SOCKADDR> to, int toLen);
    internal uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize);
    internal int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);
    internal int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);
    internal BOOL GetConsoleMode(HANDLE hConsoleHandle, Pointer<CONSOLE_MODE> lpMode);
    internal BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode);
}
