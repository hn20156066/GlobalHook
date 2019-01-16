#include "GlobalHookDll.h"

KeyboardHook::KeyboardHook(HINSTANCE hInst) {
	this->kHook = new Hook(hInst);
}

KeyboardHook::~KeyboardHook() {
	delete this->kHook;
}



