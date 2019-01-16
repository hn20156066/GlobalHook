#pragma once

class _DLLEXPORT CwpHook {

private:

	static HHOOK cHook;
	static HWND parentHwnd;

public:

	static bool Init();
	static bool Fin();
	static LRESULT CALLBACK CwpHookProc(int nCode, WPARAM wParam, LPARAM lParam);

};
