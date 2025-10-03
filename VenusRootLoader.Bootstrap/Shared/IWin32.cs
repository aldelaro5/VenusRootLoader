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
    public FreeLibrarySafeHandle GetModuleHandle(string lpModuleName);
    public FARPROC GetProcAddress(HMODULE hModule, PCSTR lpProcName);
    public HANDLE GetStdHandle(STD_HANDLE nStdHandle);
    public HANDLE CreateFile(PCWSTR lpFileName, uint dwDesiredAccess, FILE_SHARE_MODE dwShareMode, Pointer<SECURITY_ATTRIBUTES> lpSecurityAttributes, FILE_CREATION_DISPOSITION dwCreationDisposition, FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes, HANDLE hTemplateFile);
    public BOOL ReadFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToRead, Pointer<uint> lpNumberOfBytesRead, Pointer<NativeOverlapped> lpOverlapped);
    public BOOL WriteFile(HANDLE hFile, Pointer<byte> lpBuffer, uint nNumberOfBytesToWrite, Pointer<uint> lpNumberOfBytesWritten, Pointer<NativeOverlapped> lpOverlapped);
    public BOOL CloseHandle(HANDLE hObject);
    public MESSAGEBOX_RESULT MessageBox(HWND hWnd, string lpText, string lpCaption, MESSAGEBOX_STYLE uType);
    public BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow);
    public HWND GetConsoleWindow();
    public BOOL SetFilePointerEx(HANDLE hFile, long liDistanceToMove, Pointer<long> lpNewFilePointer, SET_FILE_POINTER_MOVE_METHOD dwMoveMethod);
    public int sendto(SOCKET s, PCSTR buf, int len, int flags, Pointer<SOCKADDR> to, int toLen);
    public uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize);
    public int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);
    public int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);
    public BOOL GetConsoleMode(HANDLE hConsoleHandle, Pointer<CONSOLE_MODE> lpMode);
    public BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode);
}