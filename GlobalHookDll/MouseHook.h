#pragma once

class MouseHook {

private:

	static HHOOK mHook;

public:

	static bool Init();
	static bool Fin();
	static LRESULT CALLBACK MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam);

};