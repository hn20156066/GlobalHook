using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// グループ、またはマイセットのアイテムを表示するウィンドウ
	/// </summary>
	public sealed partial class ItemListForm : GHForm {

		public ItemListForm() {
			InitializeComponent();
		}
		
		public enum ParentGH {
			Launcher = 0,
			MysetList = 1
		}

		// 表示中のアイテム番号
		public int Item_Num { get; set; } = -1;

		// 0:ランチャー 1:マイセット
		public int ParentGHForm { get; set; } = 0;

		private int ParentSelectIndex = -1;

		#region イベント

		/// <summary>
		/// アイテムリストの読み込み時のイベント
		/// </summary>
		private void ItemList_Load(object sender, EventArgs e) {
			// アイテムリストの位置・サイズを設定
			Size = new Size(GHManager.Settings.Style.ItemList.Width, GHManager.Settings.Style.ItemList.Height);
			Location = new Point(-GHManager.Launcher.Width, 0);
		}

		/// <summary>
		/// フォームが閉じる時のイベント
		/// </summary>
		protected override void OnFormClosing(FormClosingEventArgs e) {
			e.Cancel = true;
			Hide();
		}

		#endregion

		#region オーバーライド関数

		protected override bool Hide_Criteria() {
			if (FormVisible) {
				bool n = ParentGHForm == 0 ? GHManager.Launcher.SelectIndex - 1 != -1 : GHManager.MysetList.SelectIndex != -1;
				if (GHManager.Contains.ItemList || n) {
					return false;
				}

				if (!IsAnimation) {
					FixedActive = false;
					return true;
				}
			}
			return false;
		}
	
		protected override void DrawForm(ref Graphics graph) {

			base.DrawForm(ref graph);

			Skin.DrawingSkinImageRepeat(ref graph, SkinImage.Group_Background, new Rectangle(0, 0, Size.Width, Size.Height));

			if (ParentGHForm == 0) {
				// グループアイテム
				if (0 <= Item_Num && Item_Num < GroupManager.GroupList.Count) {
					GroupManager.GroupList[Item_Num].DrawItems(ref graph);
				}
			}
			else {
				// マイセットアイテム
				if (0 <= Item_Num && Item_Num < MysetManager.MysetList.Count) {
					MysetManager.MysetList[Item_Num].DrawItems(ref graph);
				}
			}
		}

		protected override void Update_Visible() {
			ParentSelectIndex = ParentGHForm == 0 ? GHManager.Launcher.SelectIndex - 1 : GHManager.MysetList.SelectIndex;
			MouseActive = GHManager.Contains.ItemList;
			MouseActive = ParentSelectIndex != -1 ? true : GHManager.Contains.ItemList;
			KeyboardActive = ParentSelectIndex != -1;
		}

		protected override void Update_Timer() {
			if (FixedActive && (MouseActive || KeyboardActive)) {
				if (!FormVisible) {
					ShowAnimation();
				}
			}
			else {
				if (FormVisible) {
					if (FixedActive && (GHManager.Contains.ItemList || ParentSelectIndex != -1)) {
						HideTimer.Stop();
					}
					else {
						HideTimer.Start();
					}
				}
			}
		}

		protected override void Update_Bounds() {
			int cnt = 0;
			Rectangle rect = new Rectangle(0, 0, 0, 0);
			int left = 0;
			int top = 0;

			if (ParentGHForm == 0) {
				if (!GroupManager.CheckOutRange(Item_Num)) return;

				GroupManager.GroupList[Item_Num].icon.GetRect(out rect);

				cnt = GroupManager.GroupList[Item_Num].GroupItems.Count;
				left = GHManager.Launcher.Left;
				top = GHManager.Launcher.Top;
			}
			else {
				if (!MysetManager.CheckOutRange(Item_Num)) return;

				MysetManager.MysetList[Item_Num].icon.GetRect(out rect);

				cnt = MysetManager.MysetList[Item_Num].MysetItems.Count;
				left = GHManager.MysetList.Left;
				top = GHManager.MysetList.Top;
			}

			CalcBounds(out Rectangle bounds, cnt, rect, left, top);

			Bounds = bounds;
		}

		protected override void Update_ItemPos() {
			Rectangle rect = new Rectangle(0, 0, 0, 0);

			if (ParentGHForm == 0) {
				if (!GroupManager.CheckOutRange(Item_Num)) return;
				rect = new Rectangle(GHManager.Settings.Style.ItemList.WindowPadding.Left, GHManager.Settings.Style.ItemList.WindowPadding.Top, GHManager.Settings.Style.ItemList.ItemSize, GHManager.Settings.Style.ItemList.ItemSize);

				GroupManager.GroupList[Item_Num].SetRectItems(ref rect);
			}
			else {
				if (!MysetManager.CheckOutRange(Item_Num)) return;
				rect = new Rectangle(GHManager.Settings.Style.ItemList.WindowPadding.Left, GHManager.Settings.Style.ItemList.WindowPadding.Top, GHManager.Settings.Style.ItemList.ItemSize, GHManager.Settings.Style.ItemList.ItemSize);

				MysetManager.MysetList[Item_Num].SetRectItems(ref rect);
			}
		}

		protected override void Update_SlideMaxAndMin() {
			int ls = GHManager.IsVertical ? GHManager.Launcher.Width : GHManager.Launcher.Height;
			int ms = GHManager.IsVertical ? GHManager.MysetList.Width : GHManager.MysetList.Height;
			GHFormDestPosition = ParentGHForm == 0 ? ls : ls + ms;
			GHFormSrcPosition = ParentGHForm == 0 ? GHManager.Launcher.Left : GHManager.MysetList.Left;
		}

		#endregion

		/// <summary>
		/// アイテムリストの位置・サイズを計算
		/// </summary>
		/// <param name="cnt">アイテム数</param>
		/// <param name="iconRect">基準となるアイコンの位置・サイズ</param>
		/// <param name="left">基準となる左の位置</param>
		/// <param name="top">基準となる上の位置</param>
		/// <returns></returns>
		private void CalcBounds(out Rectangle rect, int cnt, Rectangle iconRect, int left, int top) {

			//列数* アイテム間隔 -アイテム間隔 + 横の余白
			int w = (cnt >= GHManager.Settings.Style.ItemList.Column ? GHManager.Settings.Style.ItemList.Column : cnt) * (GHManager.Settings.Style.ItemList.ItemSize + GHManager.Settings.Style.ItemList.ItemSpace) - GHManager.Settings.Style.ItemList.ItemSpace + GHManager.Settings.Style.ItemList.WindowPadding.WSize;

			// 行数 = 切り捨て((アイテム数 - 1 / 最大列数) + 1)
			// 行数 * アイテム間隔 - アイテム間隔 + 縦の余白
			int h = (int)(Math.Ceiling((decimal)((cnt - 1) / GHManager.Settings.Style.ItemList.Column)) + 1) * (GHManager.Settings.Style.ItemList.ItemSize + GHManager.Settings.Style.ItemList.ItemSpace) - GHManager.Settings.Style.ItemList.ItemSpace + GHManager.Settings.Style.ItemList.WindowPadding.HSize;

			int x = GHManager.IsVertical ? Left : left + iconRect.X - (w - iconRect.Width);
			int y = GHManager.IsVertical ? top + iconRect.Y - (h - iconRect.Height) / 2 : Top;

			rect = new Rectangle(x, y, w, h);

		}

		/// <summary>
		/// グループの番号を設定し表示
		/// </summary>
		/// <param name="n">グループ番号</param>
		public void SetGroup(int n) {
			if (0 <= n && n < GroupManager.GroupList.Count) {
				if (!Visible)
					Visible = true;
				ParentGHForm = 0;
				FixedActive = true;
				Item_Num = n;
				Controls.Clear();
				GroupManager.GroupList[Item_Num].AddItems();
				SelectIndex = 0;
				GHFormUpdate();
			}
		}

		/// <summary>
		/// マイセットの番号を設定し表示
		/// </summary>
		/// <param name="n">マイセット番号</param>
		public void SetMyset(int n) {
			if (0 <= n && n < MysetManager.MysetList.Count) {
				if (!Visible)
					Visible = true;
				ParentGHForm = 1;
				FixedActive = true;
				Item_Num = n;
				Controls.Clear();
				MysetManager.MysetList[Item_Num].AddItems();
				SelectIndex = 0;
				GHFormUpdate();
			}
		}

		/// <summary>
		/// アイテムリストを非表示にする
		/// </summary>
		public void HideItemList() {
			if (FormVisible) {
				HideAnimation();
				FixedActive = false;
				HideTimer.Stop();
			}
		}

		protected override int GetItemCount() {
			int cnt = 0;
			if (ParentGHForm == 0) {
				if (GroupManager.AtIndex(Item_Num)) {
					cnt = GroupManager.GroupList[Item_Num].GroupItems.Count;
				}
			}
			else {
				if (MysetManager.AtIndex(Item_Num)) {
					cnt = MysetManager.MysetList[Item_Num].MysetItems.Count;
				}
			}

			return cnt;
		}
		
		protected override void DrawPriorUpdate() {
			if (ParentGHForm == 0) {
				if (0 <= ParentSelectIndex && ParentSelectIndex < GroupManager.GroupList.Count) {
					if (0 <= SelectIndex && SelectIndex < GroupManager.GroupList[ParentSelectIndex].GroupItems.Count) {
						if (!GroupManager.GroupList[ParentSelectIndex].GroupItems[SelectIndex].icon.IsEntered)
							GroupManager.GroupList[ParentSelectIndex].GroupItems[SelectIndex].icon.control.Focus();
					}
				}
			}
			else {
				if (0 <= ParentSelectIndex && ParentSelectIndex < MysetManager.MysetList.Count) {
					if (0 <= SelectIndex && SelectIndex < MysetManager.MysetList[ParentSelectIndex].MysetItems.Count) {
						if (!MysetManager.MysetList[ParentSelectIndex].MysetItems[SelectIndex].icon.IsEntered)
							MysetManager.MysetList[ParentSelectIndex].MysetItems[SelectIndex].icon.control.Focus();
					}
				}
			}
		}
	}
}
