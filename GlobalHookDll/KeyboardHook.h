#pragma once

class _DLLEXPORT KeyboardHook {

private:

	static HHOOK kHook;
	static HWND configHwnd;

public:

	static bool Init();
	static bool Fin();
	static LRESULT CALLBACK KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam);

};
