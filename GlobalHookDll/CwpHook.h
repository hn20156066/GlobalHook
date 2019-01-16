#pragma once

class CwpHook {

private:

	static HHOOK cHook;

public:

	static bool Init();
	static bool Fin();
	static LRESULT CALLBACK CwpHookProc(int nCode, WPARAM wParam, LPARAM lParam);

};
