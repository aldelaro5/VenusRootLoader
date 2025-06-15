#include <windows.h>
#include "../external/plthook/plthook.h"
#include "proxy.h"

extern "C"
{
    void EntryPoint(HMODULE hModule);
}

bool hooked = false;
HMODULE thisModuleHandle = nullptr;
UINT (*orig)(HINSTANCE, HINSTANCE, LPWSTR, INT) = nullptr;

UINT UnityMainHook(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, INT nShowCmd)
{
    EntryPoint(thisModuleHandle);
    return orig(hInstance, hPrevInstance, lpCmdLine, nShowCmd);
}

BOOL InstallHook()
{
    plthook_t *plthook = nullptr;

    if (plthook_open_by_handle(&plthook, GetModuleHandleA("Bug Fables.exe")) != 0)
    {
        MessageBoxA(nullptr, plthook_error(), "plthook_open error", MB_OK | MB_ICONERROR);
        return FALSE;
    }

    if (plthook_replace(plthook, "UnityMain",
        reinterpret_cast<void*>(UnityMainHook), reinterpret_cast<void**>(&orig)) != 0)
    {
        MessageBoxA(nullptr, plthook_error(), "plthook_replace error", MB_OK | MB_ICONERROR);
        plthook_close(plthook);
        return FALSE;
    }

    plthook_close(plthook);
    hooked = true;
    return TRUE;
}

BOOL APIENTRY DllMain(HMODULE hinstDLL, DWORD fdwReason, LPVOID lpReserved)
{
    thisModuleHandle = hinstDLL;
	if (fdwReason == DLL_PROCESS_ATTACH && !hooked)
	{
		char path[MAX_PATH];
		GetSystemDirectory(path, sizeof(path));
		strcat_s(path, "\\winhttp.dll");
		SetupProxy(LoadLibrary(path));

		return InstallHook();
	}
	return TRUE;
}
