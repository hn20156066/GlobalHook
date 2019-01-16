#include "GlobalHookDll.h"

BOOL CALLBACK WindowController::EnumWindowsProc(HWND hWnd, LPARAM lParam) {
	INT* lpCount = (INT *)lParam;
	LONG style = GetWindowLong(hWnd, GWL_STYLE);
	LONG exstyle = GetWindowLong(hWnd, GWL_EXSTYLE);
	TCHAR windowText[255];

	GetWindowText(hWnd, windowText, 255);

	TCHAR* excludeWindowText[4] = {
		launcherWindowText,
		mysetlistWindowText,
		itemlistWindowText,
		configWindowText
	};

	for (int i = 0; i < 4; ++i) {
		if (_tcsncmp(excludeWindowText[i], windowText, 255) == 0) {
			return TRUE;
		}
	}

	// 可視かつ親のウィンドウを表示＆格納
	if ((style & WS_VISIBLE) != 0) {
		if ((exstyle & WS_EX_NOREDIRECTIONBITMAP) == 0) {
			if ((exstyle & WS_EX_TOOLWINDOW) == 0) {
				windows.push_back(hWnd);
				*lpCount += 1;
			}
		}
	}

	return TRUE;
}

int WindowController::Magnet(HWND& hwnd, POINT dif, RECT& rect2, POINT srcPos, SIZE& minDiff, SIZE extSize, POINT& tempPos) {
	DSIZE scale;
	GetScale(hwnd, scale);

	POINT fit;
	fit.x = (LONG)round((double)nFitRange / scale.cx);
	fit.y = (LONG)round((double)nFitRange / scale.cy);
	//int dif = (int)((double)7 / scale.cx);

	int dir = 0;
	// 移動中のウィンドウの左側
	// 他のウィンドウの左側
	if (rect2.left - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.left - dif.x + fit.x) {
		if (rect2.top - extSize.cy - dif.y - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.bottom - dif.y + fit.y) {
			if (abs(rect2.left - dif.x - srcPos.x - ptCurFromLT.x) < minDiff.cx) {
				minDiff.cx = abs(rect2.left - dif.x - srcPos.x - ptCurFromLT.x);
				tempPos.x = rect2.left + ptCurFromLT.x - dif.x;
			}
			dir |= 0x01;
		}
	}
	// 他のウィンドウの右側
	if (rect2.right - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
		if (rect2.top - extSize.cy - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.bottom + fit.y) {
			if (abs(rect2.right - dif.x - srcPos.x - ptCurFromLT.x) < minDiff.cx) {
				minDiff.cx = abs(rect2.right - dif.x - srcPos.x - ptCurFromLT.x);
				tempPos.x = rect2.right + ptCurFromLT.x - dif.x;
			}
			dir |= 0x01;
		}
	}
	// 移動中のウィンドウの右側
	// 他のウィンドウの右側
	if (rect2.right - dif.x + fit.x > srcPos.x - ptCurFromLT.x + extSize.cx && srcPos.x - ptCurFromLT.x + extSize.cx > rect2.right - dif.x - fit.x) {
		if (rect2.top - extSize.cy - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.bottom + fit.y) {
			if (abs(rect2.right - dif.x - srcPos.x - ptCurFromLT.x) < minDiff.cx) {
				minDiff.cx = abs(rect2.right - dif.x - srcPos.x - ptCurFromLT.x);
				tempPos.x = rect2.right - extSize.cx + ptCurFromLT.x - dif.x;
			}
			dir |= 0x04;
		}
	}
	// 他のウィンドウの左側
	if (rect2.left - dif.x + fit.x > srcPos.x - ptCurFromLT.x + extSize.cx && srcPos.x - ptCurFromLT.x + extSize.cx > rect2.left - dif.x - fit.x) {
		if (rect2.top - extSize.cy - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.bottom + fit.y) {
			if (abs(rect2.left - dif.x - srcPos.x - ptCurFromLT.x) < minDiff.cx) {
				minDiff.cx = abs(rect2.left - dif.x - srcPos.x - ptCurFromLT.x);
				tempPos.x = rect2.left - extSize.cx + ptCurFromLT.x - dif.x;
			}
			dir |= 0x04;
		}
	}
	// 移動中のウィンドウの上
	// 他のウィンドウの上
	if (rect2.top - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.top + fit.y) {
		if (rect2.left - extSize.cx - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
			if (abs(rect2.top - srcPos.y - ptCurFromLT.y) < minDiff.cy) {
				minDiff.cy = abs(rect2.top - srcPos.y - ptCurFromLT.y);
				tempPos.y = rect2.top + ptCurFromLT.y;
			}
			dir |= 0x02;
		}
	}
	// 他のウィンドウの下
	if (rect2.bottom - fit.y < srcPos.y - ptCurFromLT.y && srcPos.y - ptCurFromLT.y < rect2.bottom + fit.y) {
		if (rect2.left - extSize.cx - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
			if (abs(rect2.bottom - srcPos.y - ptCurFromLT.y) < minDiff.cy) {
				minDiff.cy = abs(rect2.bottom - srcPos.y - ptCurFromLT.y);
				tempPos.y = rect2.bottom + ptCurFromLT.y;
			}
			dir |= 0x02;
		}
	}
	// 移動中のウィンドウの下
	// 他のウィンドウの下
	if (rect2.bottom + fit.y > srcPos.y - ptCurFromLT.y + extSize.cy && srcPos.y - ptCurFromLT.y + extSize.cy > rect2.bottom - fit.y) {
		if (rect2.left - extSize.cx - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
			if (abs(rect2.bottom - srcPos.y - ptCurFromLT.y) < minDiff.cy) {
				minDiff.cy = abs(rect2.bottom - srcPos.y - ptCurFromLT.y);
				tempPos.y = rect2.bottom - extSize.cy + ptCurFromLT.y;
			}
			dir |= 0x08;
		}
	}
	// 他のウィンドウの上
	if (rect2.top + fit.y > srcPos.y - ptCurFromLT.y + extSize.cy && srcPos.y - ptCurFromLT.y + extSize.cy > rect2.top - fit.y) {
		if (rect2.left - extSize.cx - dif.x - fit.x < srcPos.x - ptCurFromLT.x && srcPos.x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
			if (abs(rect2.top - srcPos.y - ptCurFromLT.y) < minDiff.cy) {
				minDiff.cy = abs(rect2.top - srcPos.y - ptCurFromLT.y);
				tempPos.y = rect2.top - extSize.cy + ptCurFromLT.y;
			}
			dir |= 0x08;
		}
	}

	return dir;
}

int WindowController::GetWindowIndex(HWND& hwnd) {
	if (hwnd == NULL) return -1;

	for (int i = 0; i < (int)windows.size(); i++) {
		if (windows[i] == hwnd) {
			return i;
		}
	}

	return -1;
}
