#pragma once
#include "stdafx.h"

#define _DLLEXPORT __declspec(dllexport)

#include "struct.h"

#include "MouseHook.h"
#include "CwpHook.h"
#include "KeyboardHook.h"
#include "WindowController.h"



class DLL {


public:

	static HINSTANCE hInst;
	static int SetHook();
	static int ResetHook();

};


#include "export.h"
