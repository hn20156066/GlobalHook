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
		public static List<Group> GroupList { get; set; }

		/// <summary>
		/// グループマネージャーの初期化
		/// </summary>
		public static void Initialize() {
			GroupList = new List<Group>(100);
		}

		public static bool AtIndex(int idx) {
			return 0 <= idx && idx < GroupList.Count;
		}

		/// <summary>
		/// グループの位置・サイズを設定
		/// </summary>
		/// <param name="rect">基準となる位置・サイズ</param>
		public static void SetRectGroups(ref Rectangle rect) {

			foreach (var item in GroupList) {

				if (GHManager.IsVertical)
					rect.Y += GHManager.Settings.Style.Launcher.ItemSize + GHManager.Settings.Style.Launcher.ItemSpace;
				else
					rect.X += GHManager.Settings.Style.Launcher.ItemSize + GHManager.Settings.Style.Launcher.ItemSpace;

				item.icon.SetRect(ref rect);
			}

		}

		/// <summary>
		/// グループの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public static void Draw(ref Graphics graph) {
			foreach (var item in GroupList) {
				item.Draw(ref graph);
			}
		}

		/// <summary>
		/// グループの追加
		/// </summary>
		/// <returns></returns>
		public static int AddGroup() {

			GroupList.Add(new Group());
			GHManager.Launcher.Controls.Add(GroupList[GroupList.Count - 1].icon.control);

			return GroupList.Count - 1;

		}

		/// <summary>
		/// グループの追加しそのグループにアイテムを追加
		/// </summary>
		/// <param name="hwnds">ウィンドウハンドル</param>
		public static void AddGroupAndItems(ref long[] hwnds) {

			int idx = AddGroup();

			for (int i = 0; i < hwnds.Length; ++i) {
				if (hwnds[i] != 0) {
					GroupList[idx].AddItem(ref hwnds[i]);
				}
			}

		}

		/// <summary>
		/// 指定のウィンドウハンドルを持つアイテムを含むグループの取得
		/// </summary>
		/// <param name="hwnd"></param>
		/// <returns></returns>
		public static int InGroup(ref long hwnd) {

			for (int i = 0; i < GroupList.Count; ++i) {
				if (GroupList[i].InGroup(ref hwnd) != -1) {
					return i;
				}
			}

			return -1;

		}

		/// <summary>
		/// アイテムの追加
		/// </summary>
		/// <param name="idx">グループの番号</param>
		/// <param name="hwnd">ウィンドウハンドル</param>
		/// <returns></returns>
		public static bool AddItem(int idx, ref long hwnd) {

			if (idx < 0 || GroupList.Count >= idx)
				return false;

			int h = InGroup(ref hwnd);

			if (h == -1) {
				GroupList[idx].AddItem(ref hwnd);
				return true;
			}

			return false;

		}

		/// <summary>
		/// 複数のアイテムを追加
		/// </summary>
		/// <param name="items">追加するウィンドウハンドル</param>
		/// <returns></returns>
		public static bool AddItems(ref long[] items) {
			if (items.Length < 2) return false;
			int parent = InGroup(ref items[0]);
			int idx = -1;
			if (parent == -1) {
				AddGroup();
				GroupList.Last().AddItem(ref items[0]);
				for (int i = 1; i < items.Length; ++i) {
					if ((idx = InGroup(ref items[i])) != -1) {
						GroupList[idx].DeleteItem(ref items[i]);
					}
					GroupList.Last().AddItem(ref items[i]);
				}
			}
			else {
				for (int i = 1; i < items.Length; ++i) {
					if ((idx = InGroup(ref items[i])) != -1) {
						GroupList[idx].DeleteItem(ref items[i]);
					}
					GroupList[parent].AddItem(ref items[i]);
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

			// どちらもグループにない場合、新規グループに追加
			if (p == -1 && c == -1) {
				AddGroup();
				GroupList.Last().AddItem(ref parent);
				GroupList.Last().AddItem(ref child);
				return true;
			}
			else if (p != c) {

				// 追加済みのアイテムの方に追加
				// 両方とも追加済みの場合は親のアイテムの方のグループに追加

				if (p != -1 && c == -1) {
					GroupList[p].AddItem(ref child);
				}
				else if (p == -1 && c != -1) {
					GroupList[c].AddItem(ref parent);
				}
				else {
					GroupList[p].AddItem(ref child);
					GroupList[c].DeleteItem(ref child);
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

			int n = GroupList.IndexOf(group);

			if (GHManager.ItemList.Item_Num == n && GHManager.ItemList.ParentGHForm == 0) {
				GHManager.ItemList.SetGroup(n);
				return false;
			}
			else {
				GHManager.ItemList.SetGroup(n);
				return true;
			}

		}

		public static bool CheckOutRange(int idx) {
			if (0 <= idx && idx < GroupList.Count) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// グループを削除
		/// </summary>
		/// <param name="group">削除するグループ</param>
		public static void DeleteGroup(Group group) {
			group.icon.control.Dispose();
			GroupList.Remove(group);
			group = null;
		}

		/// <summary>
		/// グループアイテムを削除
		/// </summary>
		/// <param name="item">削除するアイテム</param>
		public static void DeleteItem(GroupItem item) {
			for (int i = 0; i < GroupList.Count; ++i) {
				if (GroupList[i].GroupItems.ToList().IndexOf(item) != -1) {
					GroupList[i].DeleteItem(item);
					return;
				}
			}
		}

		/// <summary>
		/// グループを更新
		/// </summary>
		public static void UpdateGroup() {
			for(int i = 0; i < GroupList.Count; ++i) {
				if (!GroupList[i].GroupUpdate())
					DeleteGroup(GroupList[i]);
			}
		}
		
		/// <summary>
		/// グループをアクティブにする
		/// </summary>
		/// <param name="idx">グループ番号</param>
		/// <param name="otherGroupHide">他のグループを最小化するか</param>
		public static void SwitchGroup(int idx, bool otherGroupHide = true) {
			if (0 <= idx && idx < GroupList.Count) {
				if (otherGroupHide) {
					GroupList[idx].ShowOrHideWindows(true);
				}
				else {
					GroupList[idx].SwitchShowOrHide();
				}

				if (otherGroupHide) {
					for(int i = 0; i < GroupList.Count; ++i) {
						if (i == idx) continue;
						GroupList[i].ShowOrHideWindows(false);
					}
				}
			}
		}

	}
}
