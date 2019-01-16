#pragma once

class MouseHook {

private:
	Hook* mHook;
	const int id;

public:

	MouseHook(HINSTANCE hInst);
	~MouseHook();


};