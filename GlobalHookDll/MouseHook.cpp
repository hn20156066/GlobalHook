#include "stdafx.h"

HWND MouseHook::parentHwnd;
COPYDATASTRUCT MouseHook::cdsNeighbor;

bool MouseHook::Init() {
	MouseHook::mHook = SetWindowsHookEx(WH_MOUSE, MouseHookProc, DLL::hInst, 0);
	return MouseHook::mHook != NULL;
}

bool MouseHook::Fin() {
	return UnhookWindowsHookEx(MouseHook::mHook) != 0;
}

LRESULT CALLBACK MouseHook::MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
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
	WinMgr::GetScale(mhex->hwnd, scale);

	if (WinMgr::nMoving < 0) return CallNextHookEx(MouseHook::mHook, nCode, wParam, lParam);

	switch (wParam) {

		case WM_MOUSEMOVE:
		{
			srcPos.x = (LONG)((double)mhex->pt.x/* / scale.cx*/);
			srcPos.y = (LONG)((double)mhex->pt.y/* / scale.cy*/);
			tempPos = srcPos;

			if (WinMgr::bMoving) {
				GetWindowRect(mhex->hwnd, &rect[0]);
				WinMgr::GetWindowRect2(mhex->hwnd, &rect[1]);
				WinMgr::ModifiedRect(mhex->hwnd, rect[1]);
				dif.x = rect[1].left - rect[0].left;
				dif.y = rect[1].top - rect[0].top;

				extSize = { rect[1].right - rect[1].left, rect[1].bottom - rect[1].top };

				if (GetKeyState(WinMgr::nMoveKey) & 0x8000) {

					// カーソル位置
					POINT basePos = { mhex->pt.x - rect[1].left, mhex->pt.y - rect[1].top };
					struct { int x, y; } baseIdx = { 0, 0 };
					POINT srcDif = { 0,0 };
					memset(WinMgr::neighbors, 0, sizeof(intptr_t) * 255);
					WinMgr::neighbors[0] = (intptr_t)WinMgr::movement[0].hwnd;
					int ncnt = 1;

					// 移動ウィンドウがひっつく位置を取得
					for (int i = 0; i < (int)WinMgr::movement.size(); ++i) {
						// 移動ウィンドウのサイズ
						GetWindowRect(WinMgr::movement[i].hwnd, &rect[0]);
						WinMgr::GetWindowRect2(WinMgr::movement[i].hwnd, &rect[1]);
						WinMgr::ModifiedRect(WinMgr::movement[i].hwnd, rect[1]);
						srcDif.x = rect[1].left - rect[0].left;
						srcDif.y = rect[1].top - rect[0].top;
						srcPos.x = basePos.x + rect[1].left;
						srcPos.y = basePos.y + rect[1].top;
						extSize = { rect[1].right - rect[1].left, rect[1].bottom - rect[1].top };
						tempPos = srcPos;
						WinMgr::movement[i].temp = tempPos;
						dir = 0;
						SIZE tempDiff = minDiff;

						for (int j = 0; j < (int)WinMgr::windows.size(); ++j) {
							if (WinMgr::windows[j] == NULL) continue;

							auto itr = std::find(WinMgr::movement.begin(), WinMgr::movement.end(), WinMgr::windows[j]);
							size_t index = std::distance(WinMgr::movement.begin(), itr);
							if (index != WinMgr::movement.size())
								continue;

							WinMgr::GetWindowRect2(WinMgr::windows[j], &rect[1]);
							WinMgr::ModifiedRect(WinMgr::windows[j], rect[1]);

							if (WinMgr::IsRectNull(rect[1])) continue;

							int tdir = 0;

							if ((tdir = WinMgr::Magnet(WinMgr::movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos)) != 0) {
								if (GetKeyState(WinMgr::nGroupKey) & 0x8000) {
									WinMgr::neighbors[ncnt] = (intptr_t)WinMgr::windows[j];
									ncnt++;
									bMagnetflag = true;
								}
							}

							dir |= tdir;
						}

						if (WinMgr::isFitDisplay) {
							WinMgr::GetMonitorRect(WinMgr::movement[i].hwnd, rect[1]);
							dir |= WinMgr::Magnet(WinMgr::movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
						}
						if (WinMgr::isFitTaskbar) {
							WinMgr::GetWorkRect(mhex->hwnd, rect[1]);
							dir |= WinMgr::Magnet(WinMgr::movement[i].hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
						}

						if (tempDiff.cx > minDiff.cx || tempDiff.cy > minDiff.cy) {
							if (dir != 0) {
								WinMgr::movement[i].temp = tempPos;
							}

							if ((dir & 1) == 1 || (dir & 4) == 4) {
								baseIdx.x = i;
							}
							if ((dir & 2) == 2 || (dir & 8) == 8) {
								baseIdx.y = i;
							}
						}
					}

					for (int i = 1; i < (int)WinMgr::movement.size(); ++i) {
						WinMgr::neighbors[ncnt++] = (intptr_t)WinMgr::movement[i].hwnd;
					}

					WinMgr::GetWindowRect2(WinMgr::movement[baseIdx.x].hwnd, &rect[0]);
					WinMgr::GetWindowRect2(WinMgr::movement[baseIdx.y].hwnd, &rect[1]);
					WinMgr::ModifiedRect(WinMgr::movement[baseIdx.x].hwnd, rect[0]);
					WinMgr::ModifiedRect(WinMgr::movement[baseIdx.y].hwnd, rect[1]);

					// 基準とするウィンドウの移動距離
					POINT offset = {
						WinMgr::movement[baseIdx.x].temp.x - WinMgr::ptCurFromLT.x - rect[0].left,
						WinMgr::movement[baseIdx.y].temp.y - WinMgr::ptCurFromLT.y - rect[1].top
					};

					std::vector<POINT> tempdif;
					tempdif.push_back(dif);

					for (int i = 0; i < (int)WinMgr::movement.size(); ++i) {
						GetWindowRect(WinMgr::movement[i].hwnd, &rect[0]);
						WinMgr::GetWindowRect2(WinMgr::movement[i].hwnd, &rect[1]);
						WinMgr::ModifiedRect(WinMgr::movement[i].hwnd, rect[1]);
						srcDif.x = rect[1].left - rect[0].left;
						srcDif.y = rect[1].top - rect[0].top;
						POINT tdif = { srcDif.x - dif.x, srcDif.y - dif.y };
						rect[1].left = rect[1].left + offset.x - tdif.x;
						rect[1].top = rect[1].top + offset.y - tdif.y;
						tempdif.push_back(tdif);
						SetWindowPos(WinMgr::movement[i].hwnd, NULL, rect[1].left, rect[1].top, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_NOACTIVATE);
					}

				}
				else {
					// 移動中ウィンドウ
					memset(WinMgr::neighbors, 0, sizeof(intptr_t) * 255);
					int ncnt = 1;
					if (WinMgr::isFitWindows) {
						WinMgr::neighbors[0] = (intptr_t)mhex->hwnd;

						for (int i = 0; i < (int)WinMgr::windows.size(); ++i) {
							if (WinMgr::windows[i] == NULL) continue;

							if (WinMgr::nMoving == i) continue;

							WinMgr::GetWindowRect2(WinMgr::windows[i], &rect[1]);
							WinMgr::ModifiedRect(WinMgr::windows[i], rect[1]);

							if (WinMgr::IsRectNull(rect[1])) continue;

							if (WinMgr::Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos) != 0) {
								if (GetKeyState(WinMgr::nGroupKey) & 0x8000) {
									WinMgr::neighbors[ncnt] = (intptr_t)WinMgr::windows[i];
									ncnt++;
									bMagnetflag = true;
								}
							}
						}
					}

					if (WinMgr::isFitDisplay) {
						WinMgr::GetMonitorRect(mhex->hwnd, rect[1]);
						WinMgr::Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
					}
					if (WinMgr::isFitTaskbar) {
						WinMgr::GetWorkRect(mhex->hwnd, rect[1]);
						WinMgr::Magnet(mhex->hwnd, dif, rect[1], srcPos, minDiff, extSize, tempPos);
					}

					srcPos = tempPos;

					SetWindowPos(mhex->hwnd, NULL, srcPos.x - WinMgr::ptCurFromLT.x, srcPos.y - WinMgr::ptCurFromLT.y, 0, 0, SWP_NOSIZE);
				}

				WinMgr::bAddGroupFlag = bMagnetflag;
			}
			else if (WinMgr::nResizing > 0) {
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

				int movex = WinMgr::ptCurFromLT.x - (LONG)round((double)mhex->pt.x * scale.cx);
				int movey = WinMgr::ptCurFromLT.y - (LONG)round((double)mhex->pt.y * scale.cy);

				// left 1 3 7
				if (WinMgr::nResizing == 1 || WinMgr::nResizing == 4 || WinMgr::nResizing == 7) {
					tempRect.left = srcPos.x - WinMgr::ptCurFromLT.x;
					width = rect[1].right - srcPos.x + WinMgr::ptCurFromLT.x;
					if (tempRect.left + width > WinMgr::ptDispFromRB.x - 7) {
						tempRect.left = rect[1].left;
					}
				}
				// right 2 5 8
				else if (WinMgr::nResizing == 2 || WinMgr::nResizing == 5 || WinMgr::nResizing == 8) {
					width = srcPos.x - rect[1].left - WinMgr::ptCurFromRB.x;
				}

				// top 3 4 5
				if (3 <= WinMgr::nResizing && WinMgr::nResizing <= 5) {
					tempRect.top = srcPos.y - WinMgr::ptCurFromLT.y;
					height = rect[1].bottom - srcPos.y + WinMgr::ptCurFromLT.y;
					if (tempRect.top + height > WinMgr::ptDispFromRB.y - 7) {
						tempRect.top = rect[1].top;
					}
				}
				// bottom 6 7 8
				else if (6 <= WinMgr::nResizing && WinMgr::nResizing <= 8) {
					height = srcPos.y - rect[1].top - WinMgr::ptCurFromRB.y;
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
			if (WinMgr::bMoving) {
				WinMgr::bMoving = false;
				ReleaseCapture();

				if (WinMgr::bAddGroupFlag && WinMgr::neighbors[1] != NULL) {
					parentHwnd = FindWindowEx(NULL, NULL, NULL, WinMgr::launcherWindowText);

					cdsNeighbor.dwData = 0;
					cdsNeighbor.cbData = sizeof(intptr_t) * 255;
					cdsNeighbor.lpData = WinMgr::neighbors;

					SendMessage(parentHwnd, WM_COPYDATA, 0, (LPARAM)&cdsNeighbor);

					WinMgr::bAddGroupFlag = false;
				}
				WinMgr::nMoving = -1;
			}
			if (WinMgr::nResizing > 0) {
				WinMgr::nResizing = 0;
				WinMgr::ptCurFromRB.x = -20;
				ReleaseCapture();
			}

			break;

	}

	return CallNextHookEx(MouseHook::mHook, nCode, wParam, lParam);
}
