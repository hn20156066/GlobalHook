#pragma once

class KeyboardHook {

private:

	static HHOOK kHook;

public:

	static bool Init();
	static bool Fin();
	static LRESULT CALLBACK KeyboardHookProc(int nCode, WPARAM wParam, LPARAM lParam);

};
