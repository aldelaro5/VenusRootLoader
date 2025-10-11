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
    FreeLibrarySafeHandle GetModuleHandle(string lpModuleName);
    FARPROC GetProcAddress(HMODULE hModule, PCSTR lpProcName);
    HANDLE GetStdHandle(STD_HANDLE nStdHandle);

    HANDLE CreateFile(
        PCWSTR lpFileName,
        uint dwDesiredAccess,
        FILE_SHARE_MODE dwShareMode,
        Pointer<SECURITY_ATTRIBUTES> lpSecurityAttributes,
        FILE_CREATION_DISPOSITION dwCreationDisposition,
        FILE_FLAGS_AND_ATTRIBUTES dwFlagsAndAttributes,
        HANDLE hTemplateFile);

    BOOL WriteFile(
        HANDLE hFile,
        Pointer<byte> lpBuffer,
        uint nNumberOfBytesToWrite,
        Pointer<uint> lpNumberOfBytesWritten,
        Pointer<NativeOverlapped> lpOverlapped);

    BOOL CloseHandle(HANDLE hObject);
    MESSAGEBOX_RESULT MessageBox(HWND hWnd, string lpText, string lpCaption, MESSAGEBOX_STYLE uType);
    BOOL ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow);
    HWND GetConsoleWindow();

    int sendto(SOCKET s, PCSTR buf, int len, int flags, Pointer<SOCKADDR> to, int toLen);
    uint GetModuleFileName(HMODULE hModule, PWSTR lpFilename, uint nSize);
    int send(SOCKET s, PCSTR buf, int len, SEND_RECV_FLAGS flags);
    int recv(SOCKET s, PSTR buf, int len, SEND_RECV_FLAGS flags);
    BOOL GetConsoleMode(HANDLE hConsoleHandle, Pointer<CONSOLE_MODE> lpMode);
    BOOL SetConsoleMode(HANDLE hConsoleHandle, CONSOLE_MODE dwMode);
    BOOL CompareObjectHandles(HANDLE hFirstObjectHandle, HANDLE hSecondObjectHandle);
}