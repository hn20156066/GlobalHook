#pragma once
#include "stdafx.h"

#define _DLLEXPORT __declspec(dllexport)

struct Window
{
	char* className;
	char* text;
};

typedef struct {
	double cx;
	double cy;
} DSIZE;

extern "C" {
	_DLLEXPORT LRESULT CALLBACK CwpProc(int nCode, WPARAM wParam, LPARAM lParam);
	_DLLEXPORT LRESULT CALLBACK MouseProc(int nCode, WPARAM wParam, LPARAM lParam);
	_DLLEXPORT LRESULT CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam);
	//_DLLEXPORT intptr_t GetParentWindow();
	//_DLLEXPORT intptr_t GetChildWindow();
	_DLLEXPORT DSIZE GetScale2(intptr_t hwnd);
	_DLLEXPORT void SetLauncherWindowText(TCHAR windowText[]);
	_DLLEXPORT void SetSubWindowText(TCHAR mysetlist[], TCHAR itemlist[]);
	_DLLEXPORT void SetConfigWindowText(TCHAR windowText[]);
	_DLLEXPORT int SetHook();
	_DLLEXPORT int ResetHook();
	_DLLEXPORT void SetFitRange(int range);
	_DLLEXPORT void SetFitRangeLimit(int min, int max);
	_DLLEXPORT int GetFitRange();
	_DLLEXPORT int GetFitRangeMax();
	_DLLEXPORT int GetFitRangeMin();
	_DLLEXPORT void SetFitWindows(bool flag);
	_DLLEXPORT void SetFitTaskbar(bool flag);
	_DLLEXPORT void SetFitDisplay(bool flag);
	_DLLEXPORT bool GetFitWindows();
	_DLLEXPORT bool GetFitTaskbar();
	_DLLEXPORT bool GetFitDisplay();
	_DLLEXPORT void SetGroupKey(UINT key);
	_DLLEXPORT UINT GetGroupKey();
	_DLLEXPORT void SetMoveKey(UINT key);
	_DLLEXPORT UINT GetMoveKey();
	_DLLEXPORT void GetWindows(intptr_t* arr, int length);
	_DLLEXPORT void SetNoFitWindows(Window* pWindowArray, int size);
	_DLLEXPORT void GetNoFitWindows(Window* pWindowArray, int size);
	_DLLEXPORT void SetKeyboardHook(bool flag);
}
