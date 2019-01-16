// GlobalHookDll.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
//#include "GlobalHookDll.h"

HINSTANCE DLL::hInst;

#pragma data_seg(".shareddata")
HHOOK CwpHook::cHook(NULL);
HHOOK MouseHook::mHook(NULL);
HHOOK KeyboardHook::kHook(NULL);
BOOL WinMgr::bMoving(FALSE);
UINT WinMgr::nResizing(0);
UINT16 WinMgr::nMoving(-1);
POINT WinMgr::ptCurFromLT{ 0, 0 }; // ウィンドウ左上からの位置
POINT WinMgr::ptCurFromRB{ 0, 0 }; // ウィンドウ右下からの位置
POINT WinMgr::ptDispFromRB{ 0, 0 };
int WinMgr::nGroupKey(VK_SHIFT);
int WinMgr::nMoveKey(VK_CONTROL);
bool WinMgr::bAddGroupFlag(false);
std::vector<HWND> WinMgr::windows;
//Window WinMgr::noFitWindows[255];
int WinMgr::nFitRange(10);
int WinMgr::nFitRangeMax(100);
int WinMgr::nFitRangeMin(0);
bool WinMgr::isFitWindows(true);
bool WinMgr::isFitDisplay(true);
bool WinMgr::isFitTaskbar(true);
bool WinMgr::KeyHook(false);
intptr_t WinMgr::neighbors[255]{ 0 };
std::vector<Movement> WinMgr::movement;
TCHAR WinMgr::launcherWindowText[255]{ 0 };
TCHAR WinMgr::mysetlistWindowText[255]{ 0 };
TCHAR WinMgr::itemlistWindowText[255]{ 0 };
TCHAR WinMgr::configWindowText[255]{ 0 };
#pragma data_seg()

//HINSTANCE hInst;
HWND configHwnd;
COPYDATASTRUCT cdsMovement;

TCHAR WinAppClassName[] = L"Notepad";
TCHAR ChildWinClassName[] = L"Edit";
HWND hWinAppHandle;


_DLLEXPORT DSIZE GetScale2(intptr_t hwnd) {
	DSIZE scale;
	WinMgr::GetScale((HWND)hwnd, scale);
	return scale;
}

_DLLEXPORT void SetLauncherWindowText(TCHAR windowText[]) {
	memset(WinMgr::launcherWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(WinMgr::launcherWindowText, windowText, 255);
}

_DLLEXPORT void SetSubWindowText(TCHAR mysetlist[], TCHAR itemlist[]) {
	memset(WinMgr::mysetlistWindowText, 0, sizeof(TCHAR) * 255);
	memset(WinMgr::itemlistWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(WinMgr::mysetlistWindowText, mysetlist, 255);
	lstrcpyn(WinMgr::itemlistWindowText, itemlist, 255);
}

_DLLEXPORT void SetConfigWindowText(TCHAR windowText[]) {
	memset(WinMgr::configWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(WinMgr::configWindowText, windowText, 255);
}

_DLLEXPORT void SetFitRange(int range) {
	WinMgr::nFitRange = (WinMgr::nFitRangeMin > range ? WinMgr::nFitRangeMin : WinMgr::nFitRangeMax < range ? WinMgr::nFitRangeMax : range);
}

_DLLEXPORT void SetFitRangeLimit(int min, int max) {
	WinMgr::nFitRangeMin = min;
	WinMgr::nFitRangeMax = max;
}

_DLLEXPORT int GetFitRange() {
	return WinMgr::nFitRange;
}

_DLLEXPORT int GetFitRangeMax() {
	return WinMgr::nFitRangeMax;
}

_DLLEXPORT int GetFitRangeMin() {
	return WinMgr::nFitRangeMin;
}

_DLLEXPORT void SetFitWindows(bool flag) {
	WinMgr::isFitWindows = flag;
}

_DLLEXPORT void SetFitTaskbar(bool flag) {
	WinMgr::isFitTaskbar = flag;
}

_DLLEXPORT void SetFitDisplay(bool flag) {
	WinMgr::isFitDisplay = flag;
}

_DLLEXPORT bool GetFitWindows() {
	return WinMgr::isFitWindows;
}

_DLLEXPORT bool GetFitTaskbar() {
	return WinMgr::isFitTaskbar;
}

_DLLEXPORT bool GetFitDisplay() {
	return WinMgr::isFitDisplay;
}

_DLLEXPORT void SetGroupKey(UINT key) {
	WinMgr::nGroupKey = key;
}

_DLLEXPORT UINT GetGroupKey() {
	return WinMgr::nGroupKey;
}

_DLLEXPORT void SetMoveKey(UINT key) {
	WinMgr::nMoveKey = key;
}

_DLLEXPORT UINT GetMoveKey() {
	return WinMgr::nMoveKey;
}

_DLLEXPORT void GetWindows(intptr_t* arr, int length) {
	WinMgr::GetWindows(arr, length);
}

_DLLEXPORT void SetNoFitWindows(Window* pWindowArray, int size) {
	//for (int i = 0; i < size && i < 255; ++i)
	//{
	//	snprintf(noFitWindows[i].className, 254, "\s", pWindowArray[i].className);
	//	snprintf(noFitWindows[i].text, 254, "\s", pWindowArray[i].text);
	//}
}

_DLLEXPORT void GetNoFitWindows(Window* pWindowArray, int size) {
	//for (int i = 0; i < size && i < 255; ++i)
	//{
	//	if (noFitWindows != NULL)
	//	{
	//		snprintf(pWindowArray[i].className, 254, "\s", noFitWindows[i].className);
	//		snprintf(pWindowArray[i].text, 254, "\s", noFitWindows[i].text);
	//	}
	//}
}

_DLLEXPORT void SetKeyboardHook(bool flag) {
	WinMgr::KeyHook = flag;
}

// エントリポイント
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  reason, LPVOID lpReserved) {
	switch (reason) {
		case DLL_PROCESS_ATTACH:
		{
			// アタッチ
			//hInst = hModule;
			DLL::hInst = hModule;

			WinMgr::SetDwmapi();

			break;
		}
		case DLL_PROCESS_DETACH:
			// デタッチ
			break;
	}

	return TRUE;
}

int DLL::SetHook() {
	if (DLL::hInst == NULL) return -1;
	
	WinMgr::UpdateWindows();
	
	if (CwpHook::Init() &&
		MouseHook::Init() &&
		KeyboardHook::Init()) {
		// フック成功
		return 0;
	}
	else {
		// フック失敗
		return -1;
	}

}

int DLL::ResetHook() {
	
	if (CwpHook::Fin() &&
		MouseHook::Fin() &&
		KeyboardHook::Fin()) {
		// フック解除成功
		return 0;
	}
	else {
		// フック解除失敗
		return -1;
	}

}
