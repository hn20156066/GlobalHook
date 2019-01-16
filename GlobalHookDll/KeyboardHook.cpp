#include "GlobalHookDll.h"

bool KeyboardHook::Init() {
	KeyboardHook::kHook = SetWindowsHookEx(WH_MOUSE, KeyboardHookProc, DLL::hInst, 0);
	return KeyboardHook::kHook != NULL;
}

bool KeyboardHook::Fin() {
	return UnhookWindowsHookEx(KeyboardHook::kHook) != 0;
}

LRESULT CALLBACK KeyboardHook::KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
	if (KeyHook) {
		if (nCode == HC_ACTION) {
			configHwnd = FindWindowEx(NULL, NULL, NULL, configWindowText);

			KBDLLHOOKSTRUCT* pk = (KBDLLHOOKSTRUCT*)lParam;
			Key key;

			if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) {
				key.keyflag = 1;
			}
			else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP) {
				byte allkey[256] = { 0 };
				int i;
				GetKeyboardState(allkey);
				for (i = 0; i < 256; ++i) {
					if (allkey[i] & 0x8000) break;
				}

				key.keyflag = i <= 255 ? 2 : 3;
			}
			else {
				return TRUE;
			}

			key.keycode = (unsigned int)pk->vkCode;

			COPYDATASTRUCT cdsHook;
			cdsHook.dwData = 0;
			cdsHook.cbData = sizeof(Key);
			cdsHook.lpData = &key;

			SendMessage(configHwnd, WM_COPYDATA, 0, (LPARAM)&cdsHook);


			return TRUE;

		}
	}

	return CallNextHookEx(hookKeyboard, nCode, wParam, lParam);
}
