#pragma once
//#include "stdafx.h"

#define _DLLEXPORT __declspec(dllexport)



class DLL {


public:

	static HINSTANCE hInst;
	static int SetHook();
	static int ResetHook();

};


#include "export.h"
