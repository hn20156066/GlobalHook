#include "GlobalHookDll.h"

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
