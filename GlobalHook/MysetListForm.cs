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
	/// マイセットの一覧を表示
	/// </summary>
	public partial class MysetListForm : GHForm {

		public MysetListForm() {
			InitializeComponent();
		}

		#region イベント

		/// <summary>
		/// マイセットリストの読み込み時のイベント
		/// </summary>
		private void MysetList_Load(object sender, EventArgs e) {

			animateInfo = new GHAnimateInfo(GHManager.Settings.Animate.MysetList_DelayTime, GHManager.Settings.Animate.MysetList_AnimateTime, GHManager.Settings.Animate.MysetList_Slide, GHManager.Settings.Animate.MysetList_Fade);

			// アイテムリストの位置・サイズを設定
			Size = new Size(GHManager.Settings.Style.MysetList.Width, GHManager.Settings.Style.MysetList.Height);
			Location = new Point(-GHManager.Launcher.Width, 0);
		}

		#endregion

		#region オーバーライド

		protected override void Update_Visible() {
			MouseActive = GHManager.Contains.MysetList || GHManager.Launcher.MysetIcon.IsEntered;

			int cnt = MysetManager.GetActiveIndex();
			if (cnt != -1) {
				SelectIndex = cnt;
			}
			else {
				SelectIndex = -1;
				NoSelectItem();
			}
		}

		protected override void Update_Timer() {
			if (FixedActive && (MouseActive || KeyboardActive)) {
				if (!FormVisible) {
					GHManager.Launcher.MysetIcon.opened = true;
					ShowAnimation();
				}
			}
			else {
				if (FormVisible) {
					if (GHManager.Contains.MysetList || GHManager.Launcher.MysetIcon.IsEntered || (GHManager.ItemList.ParentGHForm == 1 && GHManager.Contains.ItemList)) {
						HideTimer.Stop();
					}
					else {
						HideTimer.Start();
					}
				}
			}
		}

		protected override void Update_Bounds() {
			int cnt = MysetManager.Items.Count;
			Rectangle rect = new Rectangle(0, 0, 0, 0);
			GHManager.Launcher.MysetIcon.GetRect(out rect);

			int w = GHManager.Settings.Style.MysetList.Width;
			int h = GHManager.Settings.Style.MysetList.ItemSize * cnt + GHManager.Settings.Style.MysetList.WindowPadding.HSize + GHManager.Settings.Style.MysetList.ItemSpace * (cnt - 1);
			int x = GHManager.IsVertical ? Left : GHManager.Launcher.Left + rect.X - GHManager.Settings.Style.Launcher.WindowPadding.Left;
			int y = GHManager.IsVertical ? GHManager.Launcher.Top + rect.Y - GHManager.Settings.Style.Launcher.WindowPadding.Top : Top;

			if (!GHManager.IsVertical) {
				int t = w;
				w = h;
				h = t;
			}

			Bounds = new Rectangle(x, y, w, h);
		}

		protected override void Update_ItemPos() {
			Rectangle rect = new Rectangle(GHManager.Settings.Style.MysetList.WindowPadding.Left, GHManager.Settings.Style.MysetList.WindowPadding.Top, GHManager.Settings.Style.MysetList.ItemSize, GHManager.Settings.Style.MysetList.ItemSize);

			MysetManager.SetPosition(ref rect, GHManager.IsVertical);
		}

		protected override void Update_SlideMaxAndMin() {
			Point offset = new Point(
				GHManager.Launcher.Width,
				GHManager.Launcher.Height
			);
			int showPos = GHManager.ScreenSize.Left + offset.X;
			int hidePos = GHManager.ScreenSize.Left;
			if (GHManager.Settings.Launcher.Pos == 1) {
				showPos = GHManager.ScreenSize.Top + offset.Y;
				hidePos = GHManager.ScreenSize.Top;
			}
			else if (GHManager.Settings.Launcher.Pos == 2) {
				hidePos = GHManager.ScreenSize.Right - offset.X;
				showPos = GHManager.ScreenSize.Right - Width - offset.X;
			}
			else if (GHManager.Settings.Launcher.Pos == 3) {
				hidePos = GHManager.ScreenSize.Bottom - offset.Y;
				showPos = GHManager.ScreenSize.Bottom - Height - offset.Y;
			}

			GHFormShowPosition = showPos;
			GHFormHidePosition = hidePos;
		}

		protected override bool Hide_Criteria() {
			if (FormVisible) {
				if (GHManager.Contains.MysetList || GHManager.Launcher.MysetIcon.IsEntered || (GHManager.ItemList.ParentGHForm == 1 && GHManager.Contains.ItemList || GHManager.ItemList.FormVisible)) {
					GHManager.Launcher.MysetIcon.opened = true;
					return false;
				}

				if (!IsAnimation) {
					GHManager.Launcher.MysetIcon.opened = false;
					FixedActive = false;
					return true;
				}
			}

			GHManager.Launcher.MysetIcon.opened = true;
			return false;
		}

		protected override void DrawForm(ref Graphics graph) {

			base.DrawForm(ref graph);

			Skin.DrawingSkinImageRepeat(ref graph, SkinImage.Launcher_Background, new Rectangle(0, 0, Size.Width, Size.Height));

			MysetManager.Draw(ref graph);
		}

		#endregion

		/// <summary>
		/// マイセットリストの表示
		/// </summary>
		public void MysetList_Show() {
			if (MysetManager.Items.Count == 0) return;

			if (!FormVisible) {
				if (!Visible)
					Visible = true;
				SelectIndex = -1;
				NoSelectItem();
				FixedActive = true;
				GHFormUpdate();
				GHManager.Launcher.MysetIcon.opened = true;
			}
		}

		/// <summary>
		/// マイセットの非表示
		/// </summary>
		public void MysetList_Hide() {
			if (FormVisible) {
				if (GHManager.ItemList.ParentGHForm == 1) {
					GHManager.ItemList.FixedActive = false;
					GHManager.ItemList.HideItemList();
				}
				FixedActive = false;
				HideTimer.Stop();
				GHManager.Launcher.MysetIcon.opened = false;
				HideAnimation();
			}
		}

		protected override int GetItemCount() {
			return MysetManager.Items.Count;
		}

		protected override void DrawPriorUpdate() {
			if (KeyboardActive) {
				if (MysetManager.CheckRange(SelectIndex)) {
					if (!MysetManager.Items[SelectIndex].icon.IsEntered) {
						MysetManager.Items[SelectIndex].icon.control.Focus();
					}
				}
			}
		}
	}
}
