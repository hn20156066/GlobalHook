// GlobalHookDll.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "GlobalHookDll.h"


typedef struct {
	HWND hwnd;
	POINT temp;

	bool operator==(const HWND& hwnd) {
		return this->hwnd == hwnd;
	}
} Movement;

//typedef struct {
//	intptr_t parent;
//	intptr_t child;
//} Neighbor;

typedef struct {
	unsigned int keyflag;
	unsigned int keycode;
} Key;

#pragma data_seg(".shareddata")
HHOOK hookCwp(NULL);
HHOOK hookKeyboard(NULL);
HHOOK hookMouse(NULL);
BOOL bMoving(FALSE);
UINT nResizing(0);
UINT16 nMoving(-1);
POINT ptCurFromLT{ 0, 0 }; // ウィンドウ左上からの位置
POINT ptCurFromRB{ 0, 0 }; // ウィンドウ右下からの位置
POINT ptDispFromRB{ 0, 0 };
int nGroupKey(VK_SHIFT);
int nMoveKey(VK_CONTROL);
bool bAddGroupFlag(false);
std::vector<HWND> windows;
Window noFitWindows[255];
int nFitRange(10);
int nFitRangeMax(100);
int nFitRangeMin(0);
bool isFitWindows(true);
bool isFitDisplay(true);
bool isFitTaskbar(true);
bool KeyHook(false);
//Neighbor neighbor{ 0 };
intptr_t neighbors[255]{ 0 };
std::vector<Movement> movement;
TCHAR launcherWindowText[255]{ 0 };
TCHAR mysetlistWindowText[255]{ 0 };
TCHAR itemlistWindowText[255]{ 0 };
TCHAR configWindowText[255]{ 0 };
#pragma data_seg()

HINSTANCE hInst;
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
static int Magnet(HWND& hwnd, POINT dif, RECT& rect2, POINT srcPos, SIZE& minDiff, SIZE extSize, POINT& tempPos);
static BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam);
static int GetWindowIndex(HWND& hwnd);
static bool IsRectNull(RECT& rect);
static void NeighborWindowRegister(); // 隣接ウィンドウを登録
static void BeforeWindowMoving(const CWPSTRUCT* p); // ウィンドウ移動前
static void BeforeWindowSizing(const CWPSTRUCT* p); // ウィンドウサイズ変更前
static bool MatchNeighborWindow(const RECT& rect1, const RECT& rect2);
void GetMonitorRect(HWND hwnd, RECT& rect);
void GetWorkRect(HWND hwnd, RECT& rect);
void GetScale(HWND hwnd, DSIZE& scale);
void ModifiedRect(HWND hwnd, RECT& rect);
//void GetWindowRect_(HWND hwnd, RECT rect[2]);

//_DLLEXPORT intptr_t GetParentWindow()
//{
//	intptr_t parent = neighbor[0];
//	neighbor[0] = 0;
//	return parent;
//}
//
//_DLLEXPORT intptr_t GetChildWindow()
//{
//	intptr_t child = neighbor[1];
//	neighbor[1] = 0;
//	return child;
//}

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
			hInst = hModule;

			SetDwmapi();

			break;
		}
		case DLL_PROCESS_DETACH:
			// デタッチ
			break;
	}

	return TRUE;
}

_DLLEXPORT LRESULT CALLBACK CwpProc(int nCode, WPARAM wParam, LPARAM lParam) {
	CWPSTRUCT* const p = (CWPSTRUCT*)lParam;
	long wp = p->wParam & 0xFFF0;
	DSIZE scale;

	if (nCode == HC_ACTION) {
		switch (p->message) {
			case WM_SYSCOMMAND:
				GetScale(p->hwnd, scale);

				switch (wp) {
					case SC_MOVE: // 移動開始時
					{
						UINT nCount = 0;
						windows.clear();
						parentHwnd = FindWindowEx(NULL, NULL, NULL, launcherWindowText);
						EnumWindows(EnumWindowsProc, (LPARAM)&nCount);

						RECT rect[2];

						GetWindowRect(p->hwnd, &rect[0]);
						GetWindowRect2(p->hwnd, &rect[1]);
						ModifiedRect(p->hwnd, rect[1]);

						// 移動するウィンドウの番号を取得
						nMoving = GetWindowIndex(p->hwnd);

						// 番号取得に失敗した場合終了
						if (nMoving < 0) break;
						if (nMoving >= windows.size()) break;

						movement.clear();
						Movement move;
						move.hwnd = windows[nMoving];
						movement.push_back(move);

						if (GetKeyState(nMoveKey) & 0x8000) {
							RECT src, ref;

							int added = 0;

							do {
								// 1ループ内で追加した数
								added = 0;

								for (int q = 0; q < 2; ++q) {
									// すべてのウィンドウ
									for (int i = 0; i < (int)windows.size(); ++i) {
										// 表示されているウィンドウのみ
										if (!IsWindowVisible(windows[i])) continue;

										// すでに登録されていたら次へ
										auto itr = std::find(movement.begin(), movement.end(), windows[i]);
										size_t index = std::distance(movement.begin(), itr);
										if (index != movement.size())
											continue;

										// ウィンドウサイズ取得
										GetWindowRect2(windows[i], &ref);
										ModifiedRect(windows[i], ref);

										// 登録ウィンドウすべて
										for (int j = 0; j < (int)movement.size(); ++j) {
											// ウィンドウサイズ取得
											GetWindowRect2(movement[j].hwnd, &src);
											ModifiedRect(movement[j].hwnd, src);

											if (MatchNeighborWindow(src, ref)) {
												move.hwnd = windows[i];
												movement.push_back(move);
												added++;

												break;
											}
										}
									}
								}
							} while (added != 0);

							for (int i = 0; i < (int)movement.size(); ++i) {
								if (movement[i].hwnd == p->hwnd) continue;
								SetWindowPos(movement[i].hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
								SetWindowPos(movement[i].hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
							}
							SetWindowPos(p->hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
							SetWindowPos(p->hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
						}
						SendMessage(p->hwnd, WM_CANCELMODE, 0, 0);
						bMoving = true;
						SetCapture(p->hwnd);
						GetWindowRect(p->hwnd, &rect[0]);
						GetWindowRect2(p->hwnd, &rect[1]);
						ModifiedRect(p->hwnd, rect[1]);
	
						POINT dif = { rect[1].left - rect[0].left, rect[1].top - rect[0].top };
						LONG x = (LONG)((double)LOWORD(p->lParam));
						LONG y = (LONG)((double)HIWORD(p->lParam));
						ptCurFromLT.x = x + dif.x - rect[1].left;
						ptCurFromLT.y = y + dif.y - rect[1].top;

						TCHAR buf[1024] = { 0 };
						swprintf_s(buf, TEXT("%d,%d "), dif.x, dif.y);
						hWinAppHandle = FindWindow(WinAppClassName, NULL);
						if (hWinAppHandle != NULL) {
							HWND hChildWinHandle = FindWindowEx(hWinAppHandle, NULL, ChildWinClassName, NULL);
							if (hChildWinHandle != NULL) {
								SendMessage(hChildWinHandle, WM_SETTEXT, 0, (LPARAM)buf);
							}
						}

						CallNextHookEx(hookCwp, nCode, wParam, lParam);
						break;
					}
					case SC_SIZE:
					{
						//int dir = (p->wParam & ~SC_SIZE);

						//if (1 <= dir && dir <= 8)
						//{
						//	RECT rect;
						//	nResizing = dir;
						//	//INT nCount = 0;
						//	//EnumWindows(EnumWindowsProc, (LPARAM)&nCount);
						//	SendMessage(p->hwnd, WM_CANCELMODE, 0, 0);
						//	SetCapture(p->hwnd);
						//	GetWindowRect2(p->hwnd, &rect);
						//	ptCurFromLT.x = LOWORD(p->lParam) - rect.left + 7;
						//	ptCurFromLT.y = HIWORD(p->lParam) - rect.top;
						//	GetWindowRect(p->hwnd, &rect);
						//	ptCurFromRB.x = LOWORD(p->lParam) - rect.right;
						//	ptCurFromRB.y = HIWORD(p->lParam) - rect.bottom;
						//	ptDispFromRB.x = rect.right;
						//	ptDispFromRB.y = rect.bottom;
						//	CallNextHookEx(hookCwp, nCode, wParam, lParam);
						//	break;
						//}
					}
				}

		}
	}

	return CallNextHookEx(hookCwp, nCode, wParam, lParam);
}

_DLLEXPORT LRESULT CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam) {
	if (KeyHook) {
		if (nCode == HC_ACTION) {
			configHwnd = FindWindowEx(NULL, NULL, NULL, configWindowText);

			KBDLLHOOKSTRUCT* pk = (KBDLLHOOKSTRUCT*)lParam;
			Key key;

			if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) {
				key.keyflag = 1;
			}
			else if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP) {
				byte allkey[256] = { 0 };
				int i;
				GetKeyboardState(allkey);
				for (i = 0; i < 256; ++i) {
					if (allkey[i] & 0x8000) break;
				}

				key.keyflag = i <= 255 ? 2 : 3;
			}
			else {
				return TRUE;
			}

			key.keycode = (unsigned int)pk->vkCode;

			COPYDATASTRUCT cdsHook;
			cdsHook.dwData = 0;
			cdsHook.cbData = sizeof(Key);
			cdsHook.lpData = &key;

			SendMessage(configHwnd, WM_COPYDATA, 0, (LPARAM)&cdsHook);


			return TRUE;

		}
	}

	return CallNextHookEx(hookKeyboard, nCode, wParam, lParam);
}

_DLLEXPORT LRESULT CALLBACK MouseProc(int nCode, WPARAM wParam, LPARAM lParam) {
	MOUSEHOOKSTRUCTEX* const mhex = (MOUSEHOOKSTRUCTEX*)lParam; // マウスデータ

	int width, height; // ウィンドウサイズ
	SIZE extSize;// , aero;  // 拡張ウィンドウサイズ
	RECT rect[2]; // 移動中ウィンドウのサイズ
	//RECT rect2; // 他のウィンドウのサイズ
	POINT srcPos = { 0, 0 };
	POINT tempPos = { 0, 0 };
	SIZE minDiff = { LONG_MAX, LONG_MAX };
	int dir = 0;
	POINT dif = { 0, 0 };
	bool bMagnetflag = false;
	//// 最大化ウィンドウを無視
	//if (IsZoomed(mhex->hwnd))
	//	return CallNextHookEx(hookMouse, nCode, wParam, lParam);

	DSIZE scale;
	GetScale(mhex->hwnd, scale);

	if (nMoving < 0) return CallNextHookEx(hookMouse, nCode, wParam, lParam);

	switch (wParam) {

		case WM_MOUSEMOVE:
		{
			srcPos.x = (LONG)((double)mhex->pt.x/* / scale.cx*/);
			srcPos.y = (LONG)((double)mhex->pt.y/* / scale.cy*/);
			tempPos = srcPos;

			if (bMoving) {
				GetWindowRect(mhex->hwnd, &rect[0]);
				GetWindowRect2(mhex->hwnd, &rect[1]);
				ModifiedRect(mhex->hwnd, rect[1]);
				dif.x = rect[1].left - rect[0].left;
				dif.y = rect[1].top - rect[0].top;
				
				extSize = { rect[1].right - rect[1].left, rect[1].bottom - rect[1].top };

				if (GetKeyState(nMoveKey) & 0x8000) {

					// カーソル位置
					POINT basePos = { mhex->pt.x - rect[1].left, mhex->pt.y - rect[1].top };
					struct { int x, y; } baseIdx = { 0, 0 };
					POINT srcDif = { 0,0 };
					memset(neighbors, 0, sizeof(intptr_t) * 255);
					neighbors[0] = (intptr_t)movement[0].hwnd;
					int ncnt = 1;

					// 移動ウィンドウがひっつく位置を取得
					for (int i = 0; i < (int)movement.size(); ++i) {
						// 移動ウィンドウのサイズ
						GetWindowRect(movement[i].hwnd, &rect[0]);
						GetWindowRect2(movement[i].hwnd, &rect[1]);
						ModifiedRect(movement[i].hwnd, rect[1]);
						srcDif.x = rect[1].left - rect[0].left;
						srcDif.y = rect[1].top - rect[0].top;
						srcPos.x = basePos.x + rect[1].left;
						srcPos.y = basePos.y + rect[1].top;
						extSize = { rect[1].right - rect[1].left, rect[1].bottom - rect[1].top };
						tempPos = srcPos;
						movement[i].temp = tempPos;
						dir = 0;
						SIZE tempDiff = minDiff;

						for (int j = 0; j < (int)windows.size(); ++j) {
							if (windows[j] == NULL) continue;

							auto itr = std::find(movement.begin(), movement.end(), windows[j]);
							size_t index = std::distance(movement.begin(), itr);
							if (index != movement.size())
								continue;

							GetWindowRect2(windows[j], &rect[1]);
							ModifiedRect(windows[j], rect[1]);

							if (IsRectNull(rect[1])) continue;

							int tdir = 0;

							if ((tdir = Magnet(movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos)) != 0) {
								if (GetKeyState(nGroupKey) & 0x8000) {
									neighbors[ncnt] = (intptr_t)windows[j];
									ncnt++;
									bMagnetflag = true;
								}
							}

							dir |= tdir;
						}

						if (isFitDisplay) {
							GetMonitorRect(movement[i].hwnd, rect[1]);
							dir |= Magnet(movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
						}
						if (isFitTaskbar) {
							GetWorkRect(mhex->hwnd, rect[1]);
							dir |= Magnet(movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
						}

						if (tempDiff.cx > minDiff.cx || tempDiff.cy > minDiff.cy) {
							if (dir != 0) {
								movement[i].temp = tempPos;
							}

							if ((dir & 1) == 1 || (dir & 4) == 4) {
								baseIdx.x = i;
							}
							if ((dir & 2) == 2 || (dir & 8) == 8) {
								baseIdx.y = i;
							}
						}
					}

					for (int i = 1; i < (int)movement.size(); ++i) {
						neighbors[ncnt++] = (intptr_t)movement[i].hwnd;
					}

					GetWindowRect2(movement[baseIdx.x].hwnd, &rect[0]);
					GetWindowRect2(movement[baseIdx.y].hwnd, &rect[1]);
					ModifiedRect(movement[baseIdx.x].hwnd, rect[0]);
					ModifiedRect(movement[baseIdx.y].hwnd, rect[1]);

					// 基準とするウィンドウの移動距離
					POINT offset = {
						movement[baseIdx.x].temp.x - ptCurFromLT.x - rect[0].left,
						movement[baseIdx.y].temp.y - ptCurFromLT.y - rect[1].top
					};

					std::vector<POINT> tempdif;
					tempdif.push_back(dif);

					for (int i = 0; i < (int)movement.size(); ++i) {
						GetWindowRect(movement[i].hwnd, &rect[0]);
						GetWindowRect2(movement[i].hwnd, &rect[1]);
						ModifiedRect(movement[i].hwnd, rect[1]);
						srcDif.x = rect[1].left - rect[0].left;
						srcDif.y = rect[1].top - rect[0].top;
						POINT tdif = { srcDif.x - dif.x, srcDif.y - dif.y };
						rect[1].left = rect[1].left + offset.x - tdif.x;
						rect[1].top = rect[1].top + offset.y - tdif.y;
						tempdif.push_back(tdif);
						SetWindowPos(movement[i].hwnd, NULL, rect[1].left, rect[1].top, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE);
					}

				}
				else {
					// 移動中ウィンドウ
					memset(neighbors, 0, sizeof(intptr_t) * 255);
					int ncnt = 1;
					if (isFitWindows) {
						neighbors[0] = (intptr_t)mhex->hwnd;

						for (int i = 0; i < (int)windows.size(); ++i) {
							if (windows[i] == NULL) continue;

							if (nMoving == i) continue;

							GetWindowRect2(windows[i], &rect[1]);
							ModifiedRect(windows[i], rect[1]);

							if (IsRectNull(rect[1])) continue;

							if (Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos) != 0) {
								if (GetKeyState(nGroupKey) & 0x8000) {
									neighbors[ncnt] = (intptr_t)windows[i];
									ncnt++;
									bMagnetflag = true;
								}
							}
						}
					}

					if (isFitDisplay) {
						GetMonitorRect(mhex->hwnd, rect[1]);
						Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
					}
					if (isFitTaskbar) {
						GetWorkRect(mhex->hwnd, rect[1]);
						Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
					}

					srcPos = tempPos;

					SetWindowPos(mhex->hwnd, NULL, srcPos.x - ptCurFromLT.x, srcPos.y - ptCurFromLT.y, 0, 0, SWP_NOSIZE);
				}

				bAddGroupFlag = bMagnetflag;
			}
			else if (nResizing > 0) {
				GetWindowRect(mhex->hwnd, &rect[0]);
				GetWindowRect(mhex->hwnd, &rect[1]);
				//ModifiedRect(mhex->hwnd, rect);
				width = rect[1].right - rect[1].left;
				height = rect[1].bottom - rect[1].top;
				RECT tempRect = rect[1];

				//for (int i = 0; i < windows.size(); i++)
				//{
				//	if (windows[i] == NULL) continue;

				//	if (nMoving == i) continue;

					//if (IsRectNull(rect2)) continue;

					// left        1 0x0001  right       2 0x0010
					// top         3 0x0011  topleft     4 0x0100
					// topright    5 0x0101  bottom      6 0x0110
					// bottomleft  7 0x0111  bottomright 8 0x1000

				int movex = ptCurFromLT.x - (LONG)round((double)mhex->pt.x * scale.cx);
				int movey = ptCurFromLT.y - (LONG)round((double)mhex->pt.y * scale.cy);

				// left 1 3 7
				if (nResizing == 1 || nResizing == 4 || nResizing == 7) {
					tempRect.left = srcPos.x - ptCurFromLT.x;
					width = rect[1].right - srcPos.x + ptCurFromLT.x;
					if (tempRect.left + width > ptDispFromRB.x - 7) {
						tempRect.left = rect[1].left;
					}
				}
				// right 2 5 8
				else if (nResizing == 2 || nResizing == 5 || nResizing == 8) {
					width = srcPos.x - rect[1].left - ptCurFromRB.x;
				}

				// top 3 4 5
				if (3 <= nResizing && nResizing <= 5) {
					tempRect.top = srcPos.y - ptCurFromLT.y;
					height = rect[1].bottom - srcPos.y + ptCurFromLT.y;
					if (tempRect.top + height > ptDispFromRB.y - 7) {
						tempRect.top = rect[1].top;
					}
				}
				// bottom 6 7 8
				else if (6 <= nResizing && nResizing <= 8) {
					height = srcPos.y - rect[1].top - ptCurFromRB.y;
				}

				//}

				rect[1] = tempRect;
				//TCHAR buf[255];
				//wsprintf(buf, TEXT("%d %d %d"), x, ptCurFromLT.x, rect.left);
				//hWinAppHandle = FindWindow(WinAppClassName, NULL);
				//if (hWinAppHandle != NULL) {
				//	HWND hChildWinHandle = FindWindowEx(hWinAppHandle, NULL, ChildWinClassName, NULL);
				//	if (hChildWinHandle != NULL) {
				//		SendMessage(hChildWinHandle, WM_SETTEXT, 0, (LPARAM)buf);
				//	}
				//}

				MoveWindow(mhex->hwnd, rect[1].left, rect[1].top, width, height, TRUE);
				//SetWindowPos(mhex->hwnd, NULL, tx, rect.top, w, rect.bottom - rect.top, NULL);
			}

			break;
		}
		case WM_LBUTTONUP:
			if (bMoving) {
				bMoving = false;
				ReleaseCapture();

				if (bAddGroupFlag && neighbors[1] != NULL) {
					parentHwnd = FindWindowEx(NULL, NULL, NULL, launcherWindowText);

					cdsNeighbor.dwData = 0;
					cdsNeighbor.cbData = sizeof(intptr_t) * 255;
					cdsNeighbor.lpData = neighbors;

					SendMessage(parentHwnd, WM_COPYDATA, 0, (LPARAM)&cdsNeighbor);

					bAddGroupFlag = false;
				}
				nMoving = -1;
			}
			if (nResizing > 0) {
				nResizing = 0;
				ptCurFromRB.x = -20;
				ReleaseCapture();
			}

			break;

	}

	return CallNextHookEx(hookMouse, nCode, wParam, lParam);
}

_DLLEXPORT int SetHook() {
	if (hInst == NULL) return 0;
		
	hookCwp = SetWindowsHookEx(WH_CALLWNDPROC, (HOOKPROC)CwpProc, hInst, 0);
	hookKeyboard = SetWindowsHookEx(WH_KEYBOARD_LL, (HOOKPROC)KeyboardProc, hInst, 0);
	hookMouse = SetWindowsHookEx(WH_MOUSE, (HOOKPROC)MouseProc, hInst, 0);

	UINT nCount = 0;
	windows.clear();
	EnumWindows(EnumWindowsProc, (LPARAM)&nCount);

	if (hookCwp == NULL || hookMouse == NULL || hookKeyboard == NULL) {
		//フック失敗
		MessageBox(NULL, TEXT("フック失敗"), NULL, MB_OK);
		return -1;
	}
	else {
		//フック成功
	}
	return 0;
}

_DLLEXPORT int ResetHook() {
	if (UnhookWindowsHookEx(hookCwp) != 0 && UnhookWindowsHookEx(hookMouse) != 0 && UnhookWindowsHookEx(hookKeyboard) != 0) {
		//フック解除成功
	}
	else {
		//フック解除失敗
		MessageBox(NULL, TEXT("フック解除失敗"), NULL, MB_OK);
		return -1;
	}
	return 0;
}

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam) {
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

//int Magnet(int src, int ref, RECT& rect2, MoveInfo& mv)
//{
//	int d = 0;
//	int bPos, mPos, sPos;
//	int var1, var2, var3, var4;
//	bool b1, b2;
//
//	bPos = ref == 0 ? rect2.left - 7 :
//		ref == 2 ? rect2.right - 7 :
//		ref == 1 ? rect2.top :
//		ref == 3 ? rect2.bottom : 0;
//
//	mPos = src % 2 == 0 ? mv.pt.x - ptCurFromLT.x : mv.pt.y - ptCurFromLT.y;
//
//	sPos = src >= 2 ? ref % 2 == 0 ? mv.size.cx : mv.size.cy : 0;
//
//	b1 = src <= 1 ? bPos - nFitRange < mPos && mPos < bPos + nFitRange :
//		bPos + nFitRange > mPos + sPos && mPos + sPos > bPos - nFitRange;
// 
//	if (b1)
//	{
//		b2 = src % 2 == 0 ? (rect2.top - mv.size.cy - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange) :
//			(rect2.left - mv.size.cx - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange);
//
//		if (b2)
//		{
//			if (abs(bPos - mPos) < src % 2 == 0 ? mv.minx : mv.miny)
//			{
//				if (ref % 2 == 0)
//				{
//					mv.minx = abs(bPos - mPos);
//					mv.temp.x = bPos - sPos + mPos;
//					d = ref == 0 ? 0x01 : 0x04;
//				}
//				else
//				{
//					mv.miny = abs(bPos - mPos);
//					mv.temp.y = bPos - sPos + mPos;
//					d = ref == 1 ? 0x01 : 0x04;
//				}
//
//				d = 1 ^ src;
//			}
//		}
//	}
//
//	return d;
//}

//int Magnet(RECT& rect2, MoveInfo& mv)
//{
//	int dir = 0;
//
//	// 移動中のウィンドウの左側
//	// 他のウィンドウの左側
//	if (rect2.left - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.left - 7 + nFitRange)
//	{
//		if (rect2.top - mv.size.cy - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange)
//		{
//			if (abs(rect2.left - 7 - mv.pt.x - ptCurFromLT.x) < mv.minx)
//			{
//				mv.minx = abs(rect2.left - 7 - mv.pt.x - ptCurFromLT.x);
//				mv.temp.x = rect2.left + ptCurFromLT.x - 7;
//				dir |= 0x01;
//			}
//		}
//	}
//	// 他のウィンドウの右側
//	if (rect2.right - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange)
//	{
//		if (rect2.top - mv.size.cy - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange)
//		{
//			if (abs(rect2.right - 7 - mv.pt.x - ptCurFromLT.x) < mv.minx)
//			{
//				mv.minx = abs(rect2.right - 7 - mv.pt.x - ptCurFromLT.x);
//				mv.temp.x = rect2.right + ptCurFromLT.x - 7;
//				dir |= 0x01;
//			}
//		}
//	}
//	// 移動中のウィンドウの右側
//	// 他のウィンドウの右側
//	if (rect2.right - 7 + nFitRange > mv.pt.x - ptCurFromLT.x + mv.size.cx && mv.pt.x - ptCurFromLT.x + mv.size.cx > rect2.right - 7 - nFitRange)
//	{
//		if (rect2.top - mv.size.cy - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange)
//		{
//			if (abs(rect2.right - 7 - mv.pt.x - ptCurFromLT.x) < mv.minx)
//			{
//				mv.minx = abs(rect2.right - 7 - mv.pt.x - ptCurFromLT.x);
//				mv.temp.x = rect2.right - mv.size.cx + ptCurFromLT.x - 7;
//				dir |= 0x04;
//			}
//		}
//	}
//	// 他のウィンドウの左側
//	if (rect2.left - 7 + nFitRange > mv.pt.x - ptCurFromLT.x + mv.size.cx && mv.pt.x - ptCurFromLT.x + mv.size.cx > rect2.left - 7 - nFitRange)
//	{
//		if (rect2.top - mv.size.cy - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange)
//		{
//			if (abs(rect2.left - 7 - mv.pt.x - ptCurFromLT.x) < mv.minx)
//			{
//				mv.minx = abs(rect2.left - 7 - mv.pt.x - ptCurFromLT.x);
//				mv.temp.x = rect2.left - mv.size.cx + ptCurFromLT.x - 7;
//				dir |= 0x04;
//			}
//		}
//	}
//	// 移動中のウィンドウの上
//	// 他のウィンドウの上
//	if (rect2.top - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.top + nFitRange)
//	{
//		if (rect2.left - mv.size.cx - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange)
//		{
//			if (abs(rect2.top - mv.pt.y - ptCurFromLT.y) < mv.miny)
//			{
//				mv.miny = abs(rect2.top - mv.pt.y - ptCurFromLT.y);
//				mv.temp.y = rect2.top + ptCurFromLT.y;
//				dir |= 0x02;
//			}
//		}
//	}
//	// 他のウィンドウの下
//	if (rect2.bottom - nFitRange < mv.pt.y - ptCurFromLT.y && mv.pt.y - ptCurFromLT.y < rect2.bottom + nFitRange)
//	{
//		if (rect2.left - mv.size.cx - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange)
//		{
//			if (abs(rect2.bottom - mv.pt.y - ptCurFromLT.y) < mv.miny)
//			{
//				mv.miny = abs(rect2.bottom - mv.pt.y - ptCurFromLT.y);
//				mv.temp.y = rect2.bottom + ptCurFromLT.y;
//				dir |= 0x02;
//			}
//		}
//	}
//	// 移動中のウィンドウの下
//	// 他のウィンドウの下
//	if (rect2.bottom + nFitRange > mv.pt.y - ptCurFromLT.y + mv.size.cy && mv.pt.y - ptCurFromLT.y + mv.size.cy > rect2.bottom - nFitRange)
//	{
//		if (rect2.left - mv.size.cx - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange)
//		{
//			if (abs(rect2.bottom - mv.pt.y - ptCurFromLT.y) < mv.miny)
//			{
//				mv.miny = abs(rect2.bottom - mv.pt.y - ptCurFromLT.y);
//				mv.temp.y = rect2.bottom - mv.size.cy + ptCurFromLT.y;
//				dir |= 0x08;
//			}
//		}
//	}
//	// 他のウィンドウの上
//	if (rect2.top + nFitRange > mv.pt.y - ptCurFromLT.y + mv.size.cy && mv.pt.y - ptCurFromLT.y + mv.size.cy > rect2.top - nFitRange)
//	{
//		if (rect2.left - mv.size.cx - 7 - nFitRange < mv.pt.x - ptCurFromLT.x && mv.pt.x - ptCurFromLT.x < rect2.right - 7 + nFitRange)
//		{
//			if (abs(rect2.top - mv.pt.y - ptCurFromLT.y) < mv.miny)
//			{
//				mv.miny = abs(rect2.top - mv.pt.y - ptCurFromLT.y);
//				mv.temp.y = rect2.top - mv.size.cy + ptCurFromLT.y;
//				dir |= 0x08;
//			}
//		}
//	}
//
//	return dir;
//}

int Magnet(HWND& hwnd, POINT dif, RECT& rect2, POINT srcPos, SIZE& minDiff, SIZE extSize, POINT& tempPos) {
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

//int Magnet(HWND& hwnd, POINT dif, RECT& rect2, int x, int y, int& minx, int& miny, int aeroh, int aerow, int& tempX, int& tempY) {
//	DSIZE scale;
//	GetScale(hwnd, scale);
//
//	POINT fit;
//	fit.x = (LONG)((double)nFitRange / scale.cx);
//	fit.y = (LONG)((double)nFitRange / scale.cy);
//	//int dif = (int)((double)7 / scale.cx);
//
//	int dir = 0;
//	// 移動中のウィンドウの左側
//	// 他のウィンドウの左側
//	if (rect2.left - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.left - dif.x + fit.x) {
//		if (rect2.top - aeroh - dif.y - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.bottom - dif.y + fit.y) {
//			if (abs(rect2.left - dif.x - x - ptCurFromLT.x) < minx) {
//				minx = abs(rect2.left - dif.x - x - ptCurFromLT.x);
//				tempX = rect2.left + ptCurFromLT.x - dif.x;
//				dir |= 0x01;
//			}
//		}
//	}
//	// 他のウィンドウの右側
//	if (rect2.right - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
//		if (rect2.top - aeroh - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.bottom + fit.y) {
//			if (abs(rect2.right - dif.x - x - ptCurFromLT.x) < minx) {
//				minx = abs(rect2.right - dif.x - x - ptCurFromLT.x);
//				tempX = rect2.right + ptCurFromLT.x - dif.x;
//				dir |= 0x01;
//			}
//		}
//	}
//	// 移動中のウィンドウの右側
//	// 他のウィンドウの右側
//	if (rect2.right - dif.x + fit.x > x - ptCurFromLT.x + aerow && x - ptCurFromLT.x + aerow > rect2.right - dif.x - fit.x) {
//		if (rect2.top - aeroh - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.bottom + fit.y) {
//			if (abs(rect2.right - dif.x - x - ptCurFromLT.x) < minx) {
//				minx = abs(rect2.right - dif.x - x - ptCurFromLT.x);
//				tempX = rect2.right - aerow + ptCurFromLT.x - dif.x;
//				dir |= 0x04;
//			}
//		}
//	}
//	// 他のウィンドウの左側
//	if (rect2.left - dif.x + fit.x > x - ptCurFromLT.x + aerow && x - ptCurFromLT.x + aerow > rect2.left - dif.x - fit.x) {
//		if (rect2.top - aeroh - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.bottom + fit.y) {
//			if (abs(rect2.left - dif.x - x - ptCurFromLT.x) < minx) {
//				minx = abs(rect2.left - dif.x - x - ptCurFromLT.x);
//				tempX = rect2.left - aerow + ptCurFromLT.x - dif.x;
//				dir |= 0x04;
//			}
//		}
//	}
//	// 移動中のウィンドウの上
//	// 他のウィンドウの上
//	if (rect2.top - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.top + fit.y) {
//		if (rect2.left - aerow - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
//			if (abs(rect2.top - y - ptCurFromLT.y) < miny) {
//				miny = abs(rect2.top - y - ptCurFromLT.y);
//				tempY = rect2.top + ptCurFromLT.y;
//				dir |= 0x02;
//			}
//		}
//	}
//	// 他のウィンドウの下
//	if (rect2.bottom - fit.y < y - ptCurFromLT.y && y - ptCurFromLT.y < rect2.bottom + fit.y) {
//		if (rect2.left - aerow - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
//			if (abs(rect2.bottom - y - ptCurFromLT.y) < miny) {
//				miny = abs(rect2.bottom - y - ptCurFromLT.y);
//				tempY = rect2.bottom + ptCurFromLT.y;
//				dir |= 0x02;
//			}
//		}
//	}
//	// 移動中のウィンドウの下
//	// 他のウィンドウの下
//	if (rect2.bottom + fit.y > y - ptCurFromLT.y + aeroh && y - ptCurFromLT.y + aeroh > rect2.bottom - fit.y) {
//		if (rect2.left - aerow - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
//			if (abs(rect2.bottom - y - ptCurFromLT.y) < miny) {
//				miny = abs(rect2.bottom - y - ptCurFromLT.y);
//				tempY = rect2.bottom - aeroh + ptCurFromLT.y;
//				dir |= 0x08;
//			}
//		}
//	}
//	// 他のウィンドウの上
//	if (rect2.top + fit.y > y - ptCurFromLT.y + aeroh && y - ptCurFromLT.y + aeroh > rect2.top - fit.y) {
//		if (rect2.left - aerow - dif.x - fit.x < x - ptCurFromLT.x && x - ptCurFromLT.x < rect2.right - dif.x + fit.x) {
//			if (abs(rect2.top - y - ptCurFromLT.y) < miny) {
//				miny = abs(rect2.top - y - ptCurFromLT.y);
//				tempY = rect2.top - aeroh + ptCurFromLT.y;
//				dir |= 0x08;
//			}
//		}
//	}
//
//	return dir;
//}

bool IsRectNull(RECT& rect) {
	return ((rect.left | rect.top | rect.right | rect.bottom) == 0x00);
}

int GetWindowIndex(HWND& hwnd) {
	if (hwnd == NULL) return -1;

	for (int i = 0; i < (int)windows.size(); i++) {
		if (windows[i] == hwnd) {
			return i;
		}
	}

	return -1;
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

void NeighborWindowRegister() {

}

void BeforeWindowMoving(const CWPSTRUCT* p) {

}

void BeforeWindowSizing(const CWPSTRUCT* p) {

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
