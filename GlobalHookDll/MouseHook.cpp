#include "GlobalHookDll.h"

MouseHook::MouseHook(HINSTANCE hInst) : id(WH_MOUSE) {
	this->mHook = new Hook(hInst);
}

MouseHook::~MouseHook() {
	delete this->mHook;
}
