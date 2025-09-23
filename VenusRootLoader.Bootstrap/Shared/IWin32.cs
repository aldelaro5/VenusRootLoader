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
    internal unsafe HANDLE CreateFile(PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, SECURITY_ATTRIBUTES* lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile);
    internal unsafe BOOL ReadFile(HANDLE hFile, byte* lpBuffer, uint nNumberOfBytesToRead, uint* lpNumberOfBytesRead, NativeOverlapped* lpOverlapped);
    internal unsafe BOOL WriteFile(HANDLE hFile, byte* lpBuffer, uint nNumberOfBytesToWrite, uint* lpNumberOfBytesWritten, NativeOverlapped* lpOverlapped);
    internal BOOL CloseHandle(HANDLE hObject);
    internal MESSAGEBOX_RESULT MessageBox(HWND hWnd, string lpText, string lpCaption, MESSAGEBOX_STYLE uType);
    internal BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow);
    internal HWND GetConsoleWindow();
    internal unsafe BOOL SetFilePointerEx(HANDLE hFile, long liDistanceToMove, long* lpNewFilePointer, SET_FILE_POINTER_MOVE_METHOD dwMoveMethod);
    internal unsafe int sendto(SOCKET s, PCSTR buf, int len, int flags, SOCKADDR* to, int toLen);
    internal uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize);
    internal int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);
    internal int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);
    internal unsafe BOOL GetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE* lpMode);
    internal BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode);
}
