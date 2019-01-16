#pragma once

class Hook {
private:
	HINSTANCE hInst;
	HHOOK hook;
public:
	Hook(HINSTANCE hInst) : hook(NULL), hInst(hInst) {}
	int SetHook(int idHook, HOOKPROC& hProc) {
		hook = SetWindowsHookEx(idHook, hProc, hInst, 0);
		return hook == NULL ? -1 : 0;
	}
};
