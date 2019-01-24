using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// グループを管理するクラス
	/// </summary>
	public static class GroupManager {

		/// <summary>
		/// グループのリスト
		/// </summary>
		//public static SortedDictionary<int, Group> Items;

		public static List<Group> Items;

		/// <summary>
		/// グループマネージャーの初期化
		/// </summary>
		public static void Initialize() {
			Items = new List<Group>(100);
			//Items = new SortedDictionary<int, Group>();
		}

		public static bool CheckRange(int idx) {
			return 0 <= idx && idx < Items.Count;
		}

		public static int GetActiveIndex() {
			for (int i = 0; i < Items.Count; ++i) {
				if (Items[i].icon.IsEntered) {
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// グループの位置・サイズを設定
		/// </summary>
		/// <param name="rect">基準となる位置・サイズ</param>
		public static void SetRectGroups(ref Rectangle rect) {

			SortedDictionary<int, int> pri = new SortedDictionary<int, int>();
			for (int i = 0; i < Items.Count; ++i) {
				pri.Add(i, Items[i].priority);
			}
			var sorted = pri.OrderBy(kvp => kvp.Value);

			foreach (var kv in sorted) {
				if (GHManager.IsVertical)
					rect.Y += GHManager.Settings.Style.Launcher.ItemSize + GHManager.Settings.Style.Launcher.ItemSpace;
				else
					rect.X += GHManager.Settings.Style.Launcher.ItemSize + GHManager.Settings.Style.Launcher.ItemSpace;

				Items[kv.Key].icon.SetRect(ref rect);
			}
		}

		/// <summary>
		/// グループの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public static void Draw(ref Graphics graph) {
			bool open = GHManager.Settings.GroupIconStyle == 2;
			foreach (var item in Items) {
				item.Draw(ref graph, open);
			}
		}

		/// <summary>
		/// グループの追加
		/// </summary>
		/// <returns></returns>
		public static int AddGroup() {

			Items.Add(new Group());
			GHManager.Launcher.Controls.Add(Items[Items.Count - 1].icon.control);
			return Items.Count - 1;

		}

		/// <summary>
		/// グループの追加しそのグループにアイテムを追加
		/// </summary>
		/// <param name="hwnds">ウィンドウハンドル</param>
		public static void AddGroupAndItems(ref long[] hwnds) {

			int idx = AddGroup();

			for (int i = 0; i < hwnds.Length; ++i) {
				if (hwnds[i] != 0) {
					Items[idx].AddItem(ref hwnds[i]);
				}
			}

		}

		/// <summary>
		/// 指定のウィンドウハンドルを持つアイテムを含むグループの取得
		/// </summary>
		/// <param name="hwnd"></param>
		/// <returns></returns>
		public static int InGroup(ref long hwnd) {

			for (int i = 0;i < Items.Count; ++i) {
				if (Items[i].InGroup(ref hwnd) != -1) {
					return i;
				}
			}
			
			return -1;

		}

		public static int InGroupItem(GroupItem item, out int itemIndex) {
			for (int i = 0; i < Items.Count; ++i) {
				if ((itemIndex = Items[i].Items.IndexOf(item)) != -1) {
					return i;
				}
			}
			itemIndex = -1;
			return -1;
		}

		public static void GroupMove(Group group, bool next) {
			int src = Items.IndexOf(group);
			if (src == -1) return;
			int dest = src + (next ? 1 : -1);
			if (dest < 0 || Items.Count <= dest) return;
			int srcPri = Items[src].priority;
			Items[src].priority = Items[dest].priority;
			Items[dest].priority = srcPri;
		}

		public static void GroupItemMove(GroupItem item, bool next) {
			int groupIndex = InGroupItem(item, out int itemIndex);
			Console.WriteLine(groupIndex + " : " + itemIndex);
			if (groupIndex == -1 || itemIndex == -1) return;
			int newIndex = itemIndex + (next ? 1 : -1);
			Console.WriteLine("newIndex:" + newIndex);
			if (newIndex < 0 || Items[groupIndex].Items.Count <= newIndex) return;
			GroupItem temp = new GroupItem(item);
			Items[groupIndex].Items.RemoveAt(itemIndex);
			Items[groupIndex].Items.Insert(newIndex, temp);
			Console.WriteLine(Items[groupIndex].Items.Count);

		}

		/// <summary>
		/// アイテムの追加
		/// </summary>
		/// <param name="idx">グループの番号</param>
		/// <param name="hwnd">ウィンドウハンドル</param>
		/// <returns></returns>
		public static bool AddItem(int idx, ref long hwnd) {

			if (idx < 0 || Items.Count >= idx)
				return false;

			int h = InGroup(ref hwnd);

			if (h == -1) {
				Items[idx].AddItem(ref hwnd);
				return true;
			}

			return false;

		}

		/// <summary>
		/// 複数のアイテムを追加
		/// </summary>
		/// <param name="hwnds">追加するウィンドウハンドル</param>
		/// <returns></returns>
		public static bool AddItems(ref long[] hwnds) {
			if (hwnds.Length < 2) return false;

			int[] index = new int[hwnds.Length];
			for (int i = 0; i < hwnds.Length; ++i) index[i] = InGroup(ref hwnds[i]);
			var items = new { hwnds, index };
			int idx = 0;

			// ウィンドウ移動
			if (hwnds.Length == 2) {
				// 移動ウィンドウが非グループ
				if (items.index[0] == -1) {
					// ひっつくウィンドウが非グループ
					if (items.index[1] == -1) {
						idx = AddGroup();
						Items[idx].AddItem(ref items.hwnds[0]);
						Items[idx].AddItem(ref items.hwnds[1]);
					}
					else {
						Items[items.index[1]].AddItem(ref items.hwnds[0]);
					}
				}
				else {
					// ひっつくウィンドウが非グループ
					if (items.index[1] == -1) {
						Items[items.index[0]].AddItem(ref items.hwnds[1]);
					}
					else {
						Items[items.index[1]].AddItem(ref items.hwnds[0]);
						Items[items.index[0]].DeleteItem(ref items.hwnds[0]);
					}
				}
			}			
			else {
				// 移動ウィンドウが非グループ
				if (items.index[0] == -1) {
					// ひっつくウィンドウが非グループ
					if (items.index[1] == -1) {
						idx = AddGroup();
						Items[idx].AddItem(ref items.hwnds[0]);
						Items[idx].AddItem(ref items.hwnds[1]);
						for (int i = 2; i < items.hwnds.Length; ++i) {
							Items[idx].AddItem(ref items.hwnds[i]);
							if (items.index[i] != -1) {
								Items[items.index[i]].DeleteItem(ref items.hwnds[i]);
							}
						}
					}
					else {
						Items[items.index[1]].AddItem(ref items.hwnds[0]);
						for (int i = 2; i < items.hwnds.Length; ++i) {
							Items[items.index[1]].AddItem(ref items.hwnds[i]);
							if (items.index[i] != -1) {
								Items[items.index[i]].DeleteItem(ref items.hwnds[i]);
							}
						}
					}
				}
				else {
					// ひっつくウィンドウが非グループ
					if (items.index[1] == -1) {
						Items[items.index[0]].AddItem(ref items.hwnds[1]);
						for (int i = 2; i < items.hwnds.Length; ++i) {
							Items[items.index[0]].AddItem(ref items.hwnds[i]);
							if (items.index[i] != -1) {
								Items[items.index[i]].DeleteItem(ref items.hwnds[i]);
							}
						}
					}
					else {
						Items[items.index[0]].DeleteItem(ref items.hwnds[0]);
						Items[items.index[1]].AddItem(ref items.hwnds[0]);
						for (int i = 2; i < items.hwnds.Length; ++i) {
							Items[items.index[1]].AddItem(ref items.hwnds[i]);
							if (items.index[i] != -1) {
								Items[items.index[i]].DeleteItem(ref items.hwnds[i]);
							}
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// アイテムの追加
		/// </summary>
		/// <param name="parent">ウィンドウのハンドル</param>
		/// <param name="child">ウィンドウハンドル</param>
		/// <returns></returns>
		public static bool AddItem(ref long parent, ref long child) {

			int p = InGroup(ref parent);
			int c = InGroup(ref child);
			int idx = -1;

			// どちらもグループにない場合、新規グループに追加
			if (p == -1 && c == -1) {
				idx = AddGroup();
				Items[idx].AddItem(ref parent);
				Items[idx].AddItem(ref child);
				return true;
			}
			else if (p != c) {

				// 追加済みのアイテムの方に追加
				// 両方とも追加済みの場合は親のアイテムの方のグループに追加

				if (p != -1 && c == -1) {
					Items[p].AddItem(ref child);
				}
				else if (p == -1 && c != -1) {
					Items[c].AddItem(ref parent);
				}
				else {
					Items[p].AddItem(ref child);
					Items[c].DeleteItem(ref child);
				}
			}

			return false;

		}

		/// <summary>
		/// アイテムリストを表示
		/// </summary>
		/// <param name="group">表示させるグループ</param>
		/// <returns></returns>
		public static bool ShowItemList(Group group) {

			int n = Items.IndexOf(group);

			if (GHManager.ItemList.ItemIndex == n && GHManager.ItemList.ParentGHForm == 0) {
				GHManager.ItemList.SetGroup(n);
				return false;
			}
			else {
				GHManager.ItemList.SetGroup(n);
				return true;
			}

		}
		
		/// <summary>
		/// グループを削除
		/// </summary>
		/// <param name="group">削除するグループ</param>
		public static void DeleteGroup(Group group) {
			group.icon.control.Dispose();
			Items.Remove(group);
			group = null;
		}

		/// <summary>
		/// グループアイテムを削除
		/// </summary>
		/// <param name="item">削除するアイテム</param>
		public static void DeleteItem(GroupItem item) {
			for (int i = 0; i < Items.Count; ++i) {
				if (Items[i].Items.ToList().IndexOf(item) != -1) {
					Items[i].DeleteItem(item);
					return;
				}
			}
		}

		/// <summary>
		/// グループを更新
		/// </summary>
		public static void UpdateGroup() {
			for(int i = 0; i < Items.Count; ++i) {
				if (!Items[i].GroupUpdate())
					DeleteGroup(Items[i]);
			}
		}
		
		/// <summary>
		/// グループをアクティブにする
		/// </summary>
		/// <param name="idx">グループ番号</param>
		/// <param name="otherGroupHide">他のグループを最小化するか</param>
		public static void SwitchGroup(int idx, bool otherGroupHide = true) {
			if (0 <= idx && idx < Items.Count) {
				if (otherGroupHide) {
					Items[idx].ShowOrHideWindows(true);
				}
				else {
					Items[idx].SwitchShowOrHide();
				}

				if (otherGroupHide) {
					for(int i = 0; i < Items.Count; ++i) {
						if (i == idx) continue;
						Items[i].ShowOrHideWindows(false);
					}
				}
			}
		}

		public static void OpenedItemList(int idx, bool open) {
			if (CheckRange(idx)) {
				Items[idx].icon.opened = open;
			}
		}

	}
}
