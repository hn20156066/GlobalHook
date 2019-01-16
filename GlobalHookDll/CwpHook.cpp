#include "GlobalHookDll.h"

bool CwpHook::Init() {
	CwpHook::cHook = SetWindowsHookEx(WH_MOUSE, CwpHookProc, DLL::hInst, 0);
	return CwpHook::cHook != NULL;
}

bool CwpHook::Fin() {
	return UnhookWindowsHookEx(CwpHook::cHook) != 0;
}

LRESULT CALLBACK CwpHook::CwpHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
	CWPSTRUCT* const p = (CWPSTRUCT*)lParam;
	long wp = p->wParam & 0xFFF0;
	DSIZE scale;

	if (nCode == HC_ACTION) {
		switch (p->message) {
			case WM_SYSCOMMAND:
				WinMgr::GetScale(p->hwnd, scale);

				switch (wp) {
					case SC_MOVE: // 移動開始時
					{
						WinMgr::UpdateWindows();
						parentHwnd = FindWindowEx(NULL, NULL, NULL, WinMgr::launcherWindowText);

						RECT rect[2];

						GetWindowRect(p->hwnd, &rect[0]);
						WinMgr::GetWindowRect2(p->hwnd, &rect[1]);
						WinMgr::ModifiedRect(p->hwnd, rect[1]);

						// 移動するウィンドウの番号を取得
						WinMgr::nMoving = WinMgr::GetWindowIndex(p->hwnd);

						// 番号取得に失敗した場合終了
						if (WinMgr::nMoving < 0) break;
						if (WinMgr::nMoving >= WinMgr::windows.size()) break;

						WinMgr::movement.clear();
						Movement move;
						move.hwnd = WinMgr::windows[WinMgr::nMoving];
						WinMgr::movement.push_back(move);

						if (GetKeyState(WinMgr::nMoveKey) & 0x8000) {
							RECT src, ref;

							int added = 0;

							do {
								// 1ループ内で追加した数
								added = 0;

								for (int q = 0; q < 2; ++q) {
									// すべてのウィンドウ
									for (int i = 0; i < (int)WinMgr::windows.size(); ++i) {
										// 表示されているウィンドウのみ
										if (!IsWindowVisible(WinMgr::windows[i])) continue;

										// すでに登録されていたら次へ
										auto itr = std::find(WinMgr::movement.begin(), WinMgr::movement.end(), WinMgr::windows[i]);
										size_t index = std::distance(WinMgr::movement.begin(), itr);
										if (index != WinMgr::movement.size())
											continue;

										// ウィンドウサイズ取得
										WinMgr::GetWindowRect2(WinMgr::windows[i], &ref);
										WinMgr::ModifiedRect(WinMgr::windows[i], ref);

										// 登録ウィンドウすべて
										for (int j = 0; j < (int)WinMgr::movement.size(); ++j) {
											// ウィンドウサイズ取得
											WinMgr::GetWindowRect2(WinMgr::movement[j].hwnd, &src);
											WinMgr::ModifiedRect(WinMgr::movement[j].hwnd, src);

											if (WinMgr::MatchNeighborWindow(src, ref)) {
												move.hwnd = WinMgr::windows[i];
												WinMgr::movement.push_back(move);
												added++;

												break;
											}
										}
									}
								}
							} while (added != 0);

							for (int i = 0; i < (int)WinMgr::movement.size(); ++i) {
								if (WinMgr::movement[i].hwnd == p->hwnd) continue;
								SetWindowPos(WinMgr::movement[i].hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
								SetWindowPos(WinMgr::movement[i].hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
							}
							SetWindowPos(p->hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
							SetWindowPos(p->hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
						}
						SendMessage(p->hwnd, WM_CANCELMODE, 0, 0);
						WinMgr::bMoving = true;
						SetCapture(p->hwnd);
						GetWindowRect(p->hwnd, &rect[0]);
						WinMgr::GetWindowRect2(p->hwnd, &rect[1]);
						WinMgr::ModifiedRect(p->hwnd, rect[1]);

						POINT dif = { rect[1].left - rect[0].left, rect[1].top - rect[0].top };
						LONG x = (LONG)((double)LOWORD(p->lParam));
						LONG y = (LONG)((double)HIWORD(p->lParam));
						WinMgr::ptCurFromLT.x = x + dif.x - rect[1].left;
						WinMgr::ptCurFromLT.y = y + dif.y - rect[1].top;

						//TCHAR buf[1024] = { 0 };
						//swprintf_s(buf, TEXT("%d,%d "), dif.x, dif.y);
						//hWinAppHandle = FindWindow(WinAppClassName, NULL);
						//if (hWinAppHandle != NULL) {
						//	HWND hChildWinHandle = FindWindowEx(hWinAppHandle, NULL, ChildWinClassName, NULL);
						//	if (hChildWinHandle != NULL) {
						//		SendMessage(hChildWinHandle, WM_SETTEXT, 0, (LPARAM)buf);
						//	}
						//}

						CallNextHookEx(CwpHook::cHook, nCode, wParam, lParam);
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

	return CallNextHookEx(CwpHook::cHook, nCode, wParam, lParam);
}
