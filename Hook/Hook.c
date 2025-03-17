#include <Windows.h>
#include <stdio.h>
#include <stdbool.h>
#include <stdint.h>

HHOOK hHook = NULL;

#pragma data_seg("Shared")

HHOOK hHookGlobal = NULL;
HWND focusedHwnd = NULL;

#pragma data_seg()
#pragma comment(linker, "/section:Shared,RWS")

const wchar_t* onFocusChangedEvent = L"Global\\OnMouseJoystickFocusChangeEvent";

HMODULE GetCurrentModule()
{
    // https://stackoverflow.com/questions/557081/how-do-i-get-the-hmodule-for-the-currently-executing-code

    HMODULE hModule = NULL;
    GetModuleHandleEx(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
        (LPCTSTR)GetCurrentModule,
        &hModule);

    return hModule;
}

LRESULT CALLBACK CBTProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode == HCBT_SETFOCUS)
    {
        focusedHwnd = (HWND)wParam;

        HANDLE hEvent = OpenEvent(EVENT_MODIFY_STATE, FALSE, onFocusChangedEvent);
        if (hEvent)
        {
            SetEvent(hEvent);
            CloseHandle(hEvent);
        }
    }

    return CallNextHookEx(hHook, nCode, wParam, lParam);
}

__declspec(dllexport) HWND __cdecl GetFocusedHwnd()
{
    return focusedHwnd;
}

__declspec(dllexport) bool __cdecl RegisterHook()
{
    if (hHookGlobal != NULL)
        return true;

    hHook = hHookGlobal = SetWindowsHookEx(WH_CBT, CBTProc, GetCurrentModule(), 0);

    return (hHookGlobal != NULL);
}

__declspec(dllexport) void __cdecl UnregisterHook()
{
    if (hHookGlobal == NULL)
        return;

    UnhookWindowsHookEx(hHookGlobal);
    hHook = hHookGlobal = NULL;
}
