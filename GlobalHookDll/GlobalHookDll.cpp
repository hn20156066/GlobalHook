// GlobalHookDll.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "GlobalHookDll.h"

#pragma data_seg(".shareddata")
HHOOK CwpHook::cHook(NULL);
HHOOK MouseHook::mHook(NULL);
HHOOK KeyboardHook::kHook(NULL);
BOOL WindowController::bMoving(FALSE);
UINT WindowController::nResizing(0);
UINT16 WindowController::nMoving(-1);
POINT WindowController::ptCurFromLT{ 0, 0 }; // ウィンドウ左上からの位置
POINT WindowController::ptCurFromRB{ 0, 0 }; // ウィンドウ右下からの位置
POINT WindowController::ptDispFromRB{ 0, 0 };
int WindowController::nGroupKey(VK_SHIFT);
int WindowController::nMoveKey(VK_CONTROL);
bool WindowController::bAddGroupFlag(false);
std::vector<HWND> WindowController::windows;
Window WindowController::noFitWindows[255];
int WindowController::nFitRange(10);
int WindowController::nFitRangeMax(100);
int WindowController::nFitRangeMin(0);
bool WindowController::isFitWindows(true);
bool WindowController::isFitDisplay(true);
bool WindowController::isFitTaskbar(true);
bool WindowController::KeyHook(false);
intptr_t WindowController::neighbors[255]{ 0 };
std::vector<Movement> WindowController::movement;
TCHAR WindowController::launcherWindowText[255]{ 0 };
TCHAR WindowController::mysetlistWindowText[255]{ 0 };
TCHAR WindowController::itemlistWindowText[255]{ 0 };
TCHAR WindowController::configWindowText[255]{ 0 };
#pragma data_seg()

//HINSTANCE hInst;
HWND parentHwnd;
HWND configHwnd;
COPYDATASTRUCT cdsNeighbor;
COPYDATASTRUCT cdsMovement;

TCHAR WinAppClassName[] = L"Notepad";
TCHAR ChildWinClassName[] = L"Edit";
HWND hWinAppHandle;

BOOL(__stdcall *GetWindowRect2)(HWND, LPRECT);

//static void GetWindowRect3(HWND, RECT&);

static void SetDwmapi();
static bool IsAeroEnabled();
static BOOL __stdcall DwmGetWindowAttribute_(HWND, LPRECT);

static bool IsRectNull(RECT& rect);
//static void NeighborWindowRegister(); // 隣接ウィンドウを登録
//static void BeforeWindowMoving(const CWPSTRUCT* p); // ウィンドウ移動前
//static void BeforeWindowSizing(const CWPSTRUCT* p); // ウィンドウサイズ変更前
//static bool MatchNeighborWindow(const RECT& rect1, const RECT& rect2);
void GetMonitorRect(HWND hwnd, RECT& rect);
void GetWorkRect(HWND hwnd, RECT& rect);
void GetScale(HWND hwnd, DSIZE& scale);
void ModifiedRect(HWND hwnd, RECT& rect);

_DLLEXPORT DSIZE GetScale2(intptr_t hwnd) {
	DSIZE scale;
	GetScale((HWND)hwnd, scale);
	return scale;
}

_DLLEXPORT void SetLauncherWindowText(TCHAR windowText[]) {
	memset(launcherWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(launcherWindowText, windowText, 255);
}

_DLLEXPORT void SetSubWindowText(TCHAR mysetlist[], TCHAR itemlist[]) {
	memset(mysetlistWindowText, 0, sizeof(TCHAR) * 255);
	memset(itemlistWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(mysetlistWindowText, mysetlist, 255);
	lstrcpyn(itemlistWindowText, itemlist, 255);
}

_DLLEXPORT void SetConfigWindowText(TCHAR windowText[]) {
	memset(configWindowText, 0, sizeof(TCHAR) * 255);
	lstrcpyn(configWindowText, windowText, 255);
}

_DLLEXPORT void SetFitRange(int range) {
	nFitRange = (nFitRangeMin > range ? nFitRangeMin : nFitRangeMax < range ? nFitRangeMax : range);
}

_DLLEXPORT void SetFitRangeLimit(int min, int max) {
	nFitRangeMin = min;
	nFitRangeMax = max;
}

_DLLEXPORT int GetFitRange() {
	return nFitRange;
}

_DLLEXPORT int GetFitRangeMax() {
	return nFitRangeMax;
}

_DLLEXPORT int GetFitRangeMin() {
	return nFitRangeMin;
}

_DLLEXPORT void SetFitWindows(bool flag) {
	isFitWindows = flag;
}

_DLLEXPORT void SetFitTaskbar(bool flag) {
	isFitTaskbar = flag;
}

_DLLEXPORT void SetFitDisplay(bool flag) {
	isFitDisplay = flag;
}

_DLLEXPORT bool GetFitWindows() {
	return isFitWindows;
}

_DLLEXPORT bool GetFitTaskbar() {
	return isFitTaskbar;
}

_DLLEXPORT bool GetFitDisplay() {
	return isFitDisplay;
}

_DLLEXPORT void SetGroupKey(UINT key) {
	nGroupKey = key;
}

_DLLEXPORT UINT GetGroupKey() {
	return nGroupKey;
}

_DLLEXPORT void SetMoveKey(UINT key) {
	nMoveKey = key;
}

_DLLEXPORT UINT GetMoveKey() {
	return nMoveKey;
}

_DLLEXPORT void GetWindows(intptr_t* arr, int length) {
	for (int i = 0; i < (int)windows.size() && i < length; ++i) {
		if (windows[i] == NULL) {
			arr[i] = 0;
		}
		else {
			arr[i] = (intptr_t)windows[i];
		}
	}
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
	KeyHook = flag;
}

// エントリポイント
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  reason, LPVOID lpReserved) {
	switch (reason) {
		case DLL_PROCESS_ATTACH:
		{
			// アタッチ
			//hInst = hModule;
			DLL::hInst = hModule;

			SetDwmapi();

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
	
	UINT nCount = 0;
	windows.clear();
	EnumWindows(EnumWindowsProc, (LPARAM)&nCount);
	
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


bool IsRectNull(RECT& rect) {
	return ((rect.left | rect.top | rect.right | rect.bottom) == 0x00);
}

void SetDwmapi() {
	if (IsAeroEnabled()) {
		GetWindowRect2 = DwmGetWindowAttribute_;
	}
	else {
		GetWindowRect2 = GetWindowRect;
	}
}

// Aeroが有効か判定 / 有効=TRUE
bool IsAeroEnabled() {
	BOOL bAero = FALSE;

	DwmIsCompositionEnabled(&bAero);

	return bAero != FALSE;
}

// GetWindowsRect の引数と戻り値の型を合わせる
BOOL __stdcall DwmGetWindowAttribute_(HWND hWnd, LPRECT lpRect) {
	HRESULT h = DwmGetWindowAttribute(hWnd, DWMWA_EXTENDED_FRAME_BOUNDS, lpRect, sizeof(RECT));
	return SUCCEEDED(h);
}

bool MatchNeighborWindow(const RECT& rect1, const RECT& rect2) {
	//bool w = (rect1.left == rect2.left || rect1.left == rect2.right ||
	//	rect1.right == rect2.left || rect1.right == rect1.right);

	//bool h = (rect1.top == rect2.top || rect1.top == rect2.bottom ||
	//	rect1.bottom == rect2.top || rect1.bottom || rect2.bottom);

	if (rect1.left == rect2.left || rect1.left == rect2.right ||
		rect1.right == rect2.left || rect1.right == rect2.right) {
		if (rect2.bottom >= rect1.top && rect1.bottom >= rect2.top) {
			return true;
		}
	}
	if (rect1.top == rect2.top || rect1.top == rect2.bottom ||
		rect1.bottom == rect2.top || rect1.bottom == rect2.bottom) {
		if (rect2.right >= rect1.left && rect1.right >= rect2.left) {
			return true;
		}
	}

	return false;
}

// スケールの取得
void GetScale(HWND hwnd, DSIZE& dSize) {
	HMONITOR hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
	MONITORINFOEX monInfo;
	monInfo.cbSize = sizeof(MONITORINFOEX);
	GetMonitorInfo(hMonitor, &monInfo);

	// モニター物理座標
	DEVMODE devMode;
	devMode.dmSize = sizeof(DEVMODE);
	devMode.dmDriverExtra = sizeof(POINTL);
	devMode.dmFields = DM_POSITION;
	EnumDisplaySettings(monInfo.szDevice, ENUM_CURRENT_SETTINGS, &devMode);

	// ワーク物理座標
	SIZE logicalDesktopSize;
	logicalDesktopSize.cx = monInfo.rcMonitor.right - monInfo.rcMonitor.left;
	logicalDesktopSize.cy = monInfo.rcMonitor.bottom - monInfo.rcMonitor.top;

	dSize.cx = (double)devMode.dmPelsWidth / (double)logicalDesktopSize.cx;
	dSize.cy = (double)devMode.dmPelsHeight / (double)logicalDesktopSize.cy;
}

// モニターの物理座標を取得
void GetMonitorRect(HWND hwnd, RECT& rect) {
	HMONITOR hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
	MONITORINFOEX monInfo;
	monInfo.cbSize = sizeof(MONITORINFOEX);
	GetMonitorInfo(hMonitor, &monInfo);

	DEVMODE devMode;
	devMode.dmSize = sizeof(DEVMODE);
	devMode.dmDriverExtra = sizeof(POINTL);
	devMode.dmFields = DM_POSITION;
	EnumDisplaySettings(monInfo.szDevice, ENUM_CURRENT_SETTINGS, &devMode);

	rect.left = devMode.dmPosition.x;
	rect.top = devMode.dmPosition.y;
	rect.right = devMode.dmPosition.x + devMode.dmPelsWidth;
	rect.bottom = devMode.dmPosition.y + devMode.dmPelsHeight;

	ModifiedRect(hwnd, rect);
}

// ワークの論理座標を取得
void GetWorkRect(HWND hwnd, RECT& rect) {
	HMONITOR hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
	MONITORINFOEX monInfo;
	monInfo.cbSize = sizeof(MONITORINFOEX);
	GetMonitorInfo(hMonitor, &monInfo);

	// 物理座標系のワーク矩形
	rect = monInfo.rcWork;
}

void ModifiedRect(HWND hwnd, RECT& rect) {
	DSIZE scale;
	GetScale(hwnd, scale);
	rect.left = (LONG)round((double)rect.left / scale.cx);
	rect.top = (LONG)round((double)rect.top / scale.cy);
	rect.right = (LONG)round((double)rect.right / scale.cx);
	rect.bottom = (LONG)round((double)rect.bottom / scale.cy);
}
