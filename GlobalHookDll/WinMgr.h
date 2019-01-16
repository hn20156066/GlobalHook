#pragma once

class WinMgr {

private:


	static Window noFitWindows[255];

public:
	static std::vector<HWND> windows;
	static intptr_t neighbors[255];
	static std::vector<Movement> movement;
	static TCHAR launcherWindowText[255];
	static TCHAR mysetlistWindowText[255];
	static TCHAR itemlistWindowText[255];
	static TCHAR configWindowText[255];
	static BOOL bMoving;
	static UINT nResizing;
	static UINT16 nMoving;
	static POINT ptCurFromLT; // ウィンドウ左上からの位置
	static POINT ptCurFromRB; // ウィンドウ右下からの位置
	static POINT ptDispFromRB;
	static int nGroupKey;
	static int nMoveKey;
	static bool bAddGroupFlag;
	static int nFitRange;
	static int nFitRangeMax;
	static int nFitRangeMin;
	static bool isFitWindows;
	static bool isFitDisplay;
	static bool isFitTaskbar;
	static bool KeyHook;

	static int Magnet(HWND& hwnd, POINT dif, RECT& rect2, POINT srcPos, SIZE& minDiff, SIZE extSize, POINT& tempPos);
	static BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam);
	static int GetWindowIndex(HWND& hwnd);
	static BOOL(__stdcall *GetWindowRect2)(HWND, LPRECT);
	static void SetDwmapi();
	static bool IsAeroEnabled();
	static BOOL __stdcall DwmGetWindowAttribute_(HWND, LPRECT);
	static bool IsRectNull(RECT& rect);
	static void GetMonitorRect(HWND hwnd, RECT& rect);
	static void GetWorkRect(HWND hwnd, RECT& rect);
	static void GetScale(HWND hwnd, DSIZE& scale);
	static void ModifiedRect(HWND hwnd, RECT& rect);
	static void UpdateWindows();
	static void GetWindows(intptr_t* hwnds, int length);
	static bool MatchNeighborWindow(const RECT& rect1, const RECT& rect2);

};
