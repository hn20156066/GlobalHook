using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH {

	/// <summary>
	/// ショートカットキーの処理
	/// </summary>
	public static class ShortcutProc {

		private static Dictionary<int, Action> ActionMap;

		public static void Initialize() {
			ActionMap = new Dictionary<int, Action>() {
				{ GH_SHID.Show				, ShowForm			},
				{ GH_SHID.Hide				, HideForm			},
				{ GH_SHID.ShowConfig		, ShowConfig		},
				{ GH_SHID.OpenSelectItem	, OpenSelectItem	},
				{ GH_SHID.DeleteSelectItem	, DeleteSelectItem	},
				{ GH_SHID.SelectNextItem	, SelectNextItem	},
				{ GH_SHID.SelectPrevItem	, SelectPrevItem	},
				{ GH_SHID.SelectNextGroup	, SelectNextGroup	},
				{ GH_SHID.SelectPrevGroup	, SelectPrevGroup	},
				{ GH_SHID.SelectGroupTile	, SelectGroupTile	},
			};
		}

		public static void RunAction(int id) {
			if (ActionMap.ContainsKey(id)) {
				ActionMap[id]();
			}
		}

		private static void ShowForm() {
			if (GHManager.Launcher.FormVisible) {
				FormType n = GHManager.GetActiveForm();
				if (n == FormType.Launcher) {
					int idx = GHManager.Launcher.SelectIndex - 1;
					if (0 <= idx && idx < GroupManager.GroupList.Count) {
						GHManager.ItemList.KeyboardActive = true;
						GroupManager.ShowItemList(GroupManager.GroupList[idx]);
					}
				}
				else if (n == FormType.MysetList) {
					int idx = GHManager.MysetList.SelectIndex;
					if (0 <= idx && idx < MysetManager.MysetList.Count) {
						GHManager.MysetList.KeyboardActive = true;
						MysetManager.SetMysetNum(MysetManager.MysetList[idx]);
					}
				}
				else {

				}
			}
			else {
				GHManager.Launcher.KeyboardActive = true;
			}
		}

		private static void HideForm() {
			if (GHManager.Launcher.FormVisible) {
				FormType n = GHManager.GetActiveForm();
				if (n == FormType.ItemList) {
					GHManager.ItemList.KeyboardActive = false;
					GHManager.ItemList.FixedActive = false;
					GHManager.ItemList.HideAnimation();
				}
				else if (n == FormType.MysetList) {
					GHManager.MysetList.KeyboardActive = false;
					GHManager.MysetList.FixedActive = false;
					GHManager.MysetList.HideAnimation();
				}
				else {
					GHManager.Launcher.KeyboardActive = false;
					GHManager.Launcher.HideAnimation();
				}
			}
		}

		private static void ShowConfig() {
			GHManager.Launcher.MenuItem_Config_Click(null, null);
		}

		private static void OpenSelectItem() {
			if (GHManager.Launcher.FormVisible) {
				FormType n = GHManager.GetActiveForm();
				if (n == FormType.Launcher) {
					// ランチャー
					if (GHManager.Launcher.MysetIcon.control.Focused) {
						if (GHManager.MysetList.FormVisible) {
							GHManager.MysetList.FixedActive = false;
							GHManager.MysetList.MysetList_Hide();
						}
						else {
							if (MysetManager.MysetList.Count > 0) {
								GHManager.MysetList.FixedActive = true;
								GHManager.MysetList.MysetList_Show();
							}
						}
					}
					else if (GroupManager.AtIndex(GHManager.Launcher.SelectIndex - 1)) {
						GroupManager.GroupList[GHManager.Launcher.SelectIndex - 1].SwitchShowOrHide();
					}
				}
				else if (n == FormType.ItemList) {
					// アイテムリスト
					int num;
					int select = GHManager.ItemList.SelectIndex;
					if (GHManager.ItemList.ParentGHForm == 0) {
						num = GHManager.Launcher.SelectIndex - 1;
						if (GroupManager.AtIndex(num)) {
							if (GroupManager.GroupList[num].AtIndex(select)) {
								GHProcess.SwitchShowOrHide((IntPtr)GroupManager.GroupList[num].GroupItems[select].Handle);
							}
						}
					}
					else {
						num = GHManager.MysetList.SelectIndex;
						if (MysetManager.AtIndex(num)) {
							if (MysetManager.MysetList[num].AtIndex(select)) {
								MysetManager.MysetList[num].MysetItems[select].Execute();
							}
						}
					}
				}
				else if (n == FormType.MysetList) {
					// マイセット
					int num = GHManager.MysetList.SelectIndex;
					if (MysetManager.AtIndex(num)) {
						MysetManager.MysetList[num].ExecuteItems();
					}
				}
			}
		}

		private static void DeleteSelectItem() {
			FormType n = GHManager.GetActiveForm();
			if (n == FormType.Launcher) {
				if (GHManager.Launcher.FormVisible) {
					int idx = GHManager.Launcher.SelectIndex - 1;
					if (0 <= idx && idx < GroupManager.GroupList.Count) {
						GroupManager.DeleteGroup(GroupManager.GroupList[idx]);
					}
				}
			}
			else if (n == FormType.ItemList) {
				if (GHManager.ItemList.FormVisible) {
					int select = 0;

					if (GHManager.ItemList.ParentGHForm == 0) {
						select = GHManager.Launcher.SelectIndex - 1;
						if (GroupManager.AtIndex(select)) {
							GroupManager.GroupList[select].DeleteItem(GHManager.ItemList.SelectIndex);
							if (GroupManager.GroupList[select].GroupItems.Count <= 0) {
								GHManager.ItemList.HideItemList();
							}
						}
					}
					else {
						select = GHManager.MysetList.SelectIndex;
						if (MysetManager.AtIndex(select)) {
							if (MysetManager.MysetList[select].DeleteItem(GHManager.ItemList.SelectIndex)) {
								GHManager.ItemList.HideItemList();
								if (MysetManager.MysetList.Count <= 0) {
									GHManager.MysetList.MysetList_Hide();
								}
							}
						}
					}

				}
			}
			else if (n == FormType.MysetList) {
				if (GHManager.MysetList.FormVisible) {
					int idx = GHManager.MysetList.SelectIndex;
					if (MysetManager.AtIndex(idx)) {
						MysetManager.DeleteMyset(MysetManager.MysetList[idx]);
						if (MysetManager.MysetList.Count <= 0) {
							GHManager.MysetList.MysetList_Hide();
						}
					}
				}
			}
		}

		private static void SelectNextItem() {
			GHManager.Launcher.KeyboardActive = true;
			FormType active = GHManager.GetActiveForm();
			if (active == FormType.Launcher) {
				GHManager.Launcher.SelectNextItem(1);
			}
			else if (active == FormType.ItemList) {
				GHManager.ItemList.SelectNextItem(1);
			}
			else if (active == FormType.MysetList) {
				GHManager.MysetList.SelectNextItem(1);
			}
		}

		private static void SelectPrevItem() {
			GHManager.Launcher.KeyboardActive = true;
			FormType active = GHManager.GetActiveForm();
			if (active == FormType.Launcher) {
				GHManager.Launcher.SelectNextItem(-1);
			}
			else if (active == FormType.ItemList) {
				GHManager.ItemList.SelectNextItem(-1);
			}
			else if (active == FormType.MysetList) {
				GHManager.MysetList.SelectNextItem(-1);
			}
		}

		private static void SelectNextGroup() {
			if (GroupManager.GroupList.Count == 0) return;

			GHManager.Launcher.KeyboardActive = true;

			GHManager.Launcher.SelectNextGroup(1);
			GroupManager.SwitchGroup(GHManager.Launcher.SelectIndex - 1);
		}

		private static void SelectPrevGroup() {
			if (GroupManager.GroupList.Count == 0) return;

			GHManager.Launcher.KeyboardActive = true;
			GHManager.Launcher.SelectNextGroup(-1);
			GroupManager.SwitchGroup(GHManager.Launcher.SelectIndex - 1);
		}

		private static void SwitchSelectGroup() {
			int idx = GHManager.Launcher.SelectIndex - 1;
			if (0 <= idx && idx < GroupManager.GroupList.Count) {
				GHManager.ItemList.KeyboardActive = true;
				GroupManager.SwitchGroup(idx, false);
			}
		}

		private static void SelectGroupTile() {
			if (GroupManager.GroupList.Count == 0) return;

			if (GroupManager.AtIndex(GHManager.Launcher.SelectIndex - 1)) {
				GHManager.Launcher.KeyboardActive = true;
				GroupManager.GroupList[GHManager.Launcher.SelectIndex - 1].GroupItemsTile();
			}
		}

	}

}
