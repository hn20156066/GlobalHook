using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GH {

	/// <summary>
	/// グループ
	/// </summary>
	public class Group {

		/// <summary>
		/// グループアイテム
		/// </summary>
		public List<GroupItem> Items;

		/// <summary>
		/// グループアイコン
		/// </summary>
		public GHIcon icon;

		/// <summary>
		/// グループのメニュー
		/// </summary>
		public ContextMenu groupmenu;
		
		/// <summary>
		/// タイマー (アイテムリストの表示まで待つため)
		/// </summary>
		private Timer timer;

		private bool IsTiledWindows = false;

		/// <summary>
		/// グループコンストラクタ
		/// </summary>
		/// <param name="name">グループ名</param>
		public Group() {
			Items = new List<GroupItem>();
			icon = new GHIcon(SkinImage.Launcher_Item, FormType.Launcher);
			icon.control.MouseEnter += new EventHandler(Group_Control_Enter);
			icon.control.MouseLeave += new EventHandler(Group_Control_Leave);
			icon.control.MouseClick += new MouseEventHandler(Group_Control_Click);

			timer = new Timer {
				Interval = 400
			};
			timer.Tick += new EventHandler(Group_Control_Timer);

		}

		~Group() {
			Items.Clear();
			Items = null;
			icon = null;
			if (groupmenu != null) {
				groupmenu.Dispose();
				groupmenu = null;
			}
			if (timer != null) {
				timer.Dispose();
				timer = null;
			}
		}

		/// <summary>
		/// グループメニューの初期化
		/// </summary>
		private void GroupMenuInitialize() {

			groupmenu = new ContextMenu();
			MenuItem item = new MenuItem("前に移動", MenuItem_MovePrev_Click);
			groupmenu.MenuItems.Add(item);
			item = new MenuItem("次に移動", MenuItem_MoveNext_Click);
			groupmenu.MenuItems.Add(item);
			item = new MenuItem("グループ削除", MenuItem_DelGroup_Click);
			groupmenu.MenuItems.Add(item);
			item = new MenuItem("すべて閉じる", MenuItem_AllClose_Click);
			groupmenu.MenuItems.Add(item);
			item = new MenuItem("マイセットに登録", MenuItem_RegMyset_Click);
			groupmenu.MenuItems.Add(item);
			if (IsTiledWindows) {
				item = new MenuItem("元に戻す", MenuItem_AutoTilePrev_Click);
			}
			else {
				item = new MenuItem("ウィンドウを並べる", MenuItem_AutoTile_Click);
			}
			groupmenu.MenuItems.Add(item);

		}

		private void MenuItem_MoveNext_Click(object sender, EventArgs e) {
			GroupManager.GroupMove(this, true);
		}

		private void MenuItem_MovePrev_Click(object sender, EventArgs e) {
			GroupManager.GroupMove(this, false);
		}

		/// <summary>
		/// メニューの グループ削除 をクリックした時のイベント
		/// </summary>
		private void MenuItem_DelGroup_Click(object sender, EventArgs e) {
			GroupManager.DeleteGroup(this);
		}

		/// <summary>
		/// メニューの すべて閉じる をクリックした時のイベント
		/// </summary>
		private void MenuItem_AllClose_Click(object sender, EventArgs e) {
			foreach (var item in Items) {
				WinAPI.SendMessage((IntPtr)item.Handle, WinAPI.WM_CLOSE, 0, 0);
			}
		}

		/// <summary>
		/// メニューの マイセットに登録 をクリックした時のイベント
		/// </summary>
		private void MenuItem_RegMyset_Click(object sender, EventArgs e) {
			MysetManager.AddMyset(this);
		}

		/// <summary>
		/// メニューの ウィンドウを並べる をクリックした時のイベント
		/// </summary>
		private void MenuItem_AutoTile_Click(object sender, EventArgs e) {

			int tileCnt = 0;
			if (Items.Count == 1) {
				if (GHProcess.IsMinimize((IntPtr)Items.First().Handle)) {
					return;
				}
				DwmAPI.GetWindowRect((IntPtr)Items[0].Handle, out Rectangle rect);
				Items[0].PrevRect = rect;
				GHProcess.Maximize((IntPtr)Items[0].Handle);
				tileCnt++;
			}
			else {
				int column = (int)(Math.Ceiling((decimal)((Items.Count - 1) / 7.0f)) + 1);
				int[] row = new int[column];
				int cnt = Items.Count;
				int col = column;
				for (int i = 0; i < row.Length; ++i) {
					row[i] = (int)(Math.Floor((decimal)(cnt / col)));
					cnt -= row[i];
					--col;
				}

				int n = 0;
				int width, height;
				IntPtr Handle;
				WinAPI.DSIZE scale = GHManager.Scale;

				Rectangle workRect = GHManager.WorkingArea;
				workRect = new Rectangle(
					(int)(workRect.Left / scale.cx),
					(int)(workRect.Top / scale.cy),
					(int)(workRect.Width / scale.cx),
					(int)(workRect.Height / scale.cy)
					);
				
				width = (workRect.Width / column);
				for (int i = 0; i < column; ++i) {
					height = (workRect.Height / row[i]);
					for (int j = 0; j < row[i]; ++j) {
						Handle = (IntPtr)Items[n].Handle;
						if (GHProcess.IsMinimize(Handle)) {
							GHProcess.Normalize(Handle);
						}
						WinAPI.GetWindowRect(Handle, out WinAPI.RECT rect1);
						DwmAPI.GetWindowRect(Handle, out Rectangle rect2);
						int dif = rect2.Left - rect1.left;

						Items[n].PrevRect = new Rectangle(rect1.left, rect1.top, rect1.right - rect1.left, rect1.bottom - rect1.top);
						WinAPI.SetWindowPos(Handle, new IntPtr(-1), width * i - dif + workRect.Left, height * j + workRect.Top, width + dif * 2, height + dif, WinAPI.SWP_SHOWWINDOW);
						WinAPI.SetWindowPos(Handle, new IntPtr(-2), 0, 0, 0, 0, WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_SHOWWINDOW);
						++n;
						tileCnt++;
					}
				}
			}
			if (tileCnt > 0)
				IsTiledWindows = true;
		}

		private void MenuItem_AutoTilePrev_Click(object sender, EventArgs e) {
			foreach (var item in Items) {
				if (GHProcess.IsMinimize((IntPtr)item.Handle)) {
					GHProcess.Normalize((IntPtr)item.Handle);
				}
				WinAPI.SetWindowPos((IntPtr)item.Handle, new IntPtr(-1), item.PrevRect.Left, item.PrevRect.Top, item.PrevRect.Width, item.PrevRect.Height, WinAPI.SWP_SHOWWINDOW);
				WinAPI.SetWindowPos((IntPtr)item.Handle, new IntPtr(-2), 0, 0, 0, 0, WinAPI.SWP_NOMOVE | WinAPI.SWP_NOSIZE | WinAPI.SWP_SHOWWINDOW);
			}
			IsTiledWindows = false;
		}

		public void GroupItemsTile() {
			if (IsTiledWindows) {
				MenuItem_AutoTilePrev_Click(null, null);
			}
			else {
				MenuItem_AutoTile_Click(null, null);
			}
		}

		/// <summary>
		/// グループにアイテムを追加
		/// </summary>
		/// <param name="hwnd">追加するアイテムのウィンドウハンドル</param>
		public void AddItem(ref long hwnd) {

			if (hwnd == 0L)
				return;

			Items.Add(new GroupItem(ref hwnd));

		}

		/// <summary>
		/// アイテムを削除
		/// </summary>
		/// <param name="hwnd">削除するウィンドウハンドル</param>
		public void DeleteItem(ref long hwnd) {

			int n = InGroup(ref hwnd);

			if (n != -1) {
				DeleteItem(Items[n]);
			}
		}

		public void DeleteItem(int idx) {
			if (0 <= idx && idx < Items.Count) {
				DeleteItem(Items[idx]);
			}
		}

		/// <summary>
		/// アイテムを削除
		/// </summary>
		/// <param name="item">削除するアイテム</param>
		public void DeleteItem(GroupItem item) {

			item.icon.control.Dispose();
			Items.Remove(item);
			item = null;
		}

		/// <summary>
		/// グループを描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph) {

			icon.DrawBackGround(ref graph);
			icon.Draw(ref graph);

		}

		/// <summary>
		/// グループアイテムを描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void DrawItems(ref Graphics graph) {

			foreach (var item in Items) {
				item.Draw(ref graph);
			}

		}

		/// <summary>
		/// アイテムの位置・サイズを設定
		/// </summary>
		/// <param name="rect">基準となる位置・サイズ</param>
		public void SetRectItems(ref Rectangle rect) {

			int itemSize = GHManager.Settings.Style.ItemList.ItemSize;
			int pad = GHManager.Settings.Style.ItemList.ItemSpace;
			int column = GHManager.Settings.Style.ItemList.Column;
			int baseX = rect.X;

			foreach (var item in Items.Select((item, i) => new { item, i })) {
				item.item.icon.SetRect(ref rect);
				rect.X += itemSize + pad;

				if (item.i % column == column - 1) {
					rect.X = baseX;
					rect.Y += itemSize + pad;
				}
			}
		}

		/// <summary>
		/// グループ内に指定のウィンドウハンドルを持つアイテムがあるか
		/// </summary>
		/// <param name="hwnd">ウィンドウハンドル</param>
		/// <returns></returns>
		public int InGroup(ref long hwnd) {

			for (int i = 0; i < Items.Count; ++i) {
				if (Items[i].Handle == hwnd) {
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// グループアイコンにカーソルを乗せた時のイベント
		/// </summary>
		private void Group_Control_Enter(object sender, EventArgs e) {

			// 表示されていて、既にアイテム番号が設定されていた時はすぐに変える
			if (GHManager.ItemList.ItemIndex != -1 && GHManager.ItemList.FormVisible /*GHManager.ItemList.GHFormVisible*/) {
				if (GroupManager.ShowItemList(this)) {
					timer.Stop();
				}
			}
			else {
				timer.Start();
			}

		}

		/// <summary>
		/// グループアイコンからカーソルを離した時のイベント
		/// </summary>
		private void Group_Control_Leave(object sender, EventArgs e) {
			if (!GHManager.Launcher.FixedActive /*GHManager.Launcher.GHFormFixed*/) {
				timer.Stop();
			}
		}

		/// <summary>
		/// グループアイコンをクイックした時のイベント
		/// </summary>
		private void Group_Control_Click(object vender, MouseEventArgs e) {
			timer.Stop();
			
			if (e.Button == MouseButtons.Left) {
				// 全アイテムのウィンドウの表示・非表示を切り替え
				GHManager.ItemList.HideItemList();
				SwitchShowOrHide();
			}
			else if (e.Button == MouseButtons.Right) {
				// グループメニュー表示
				GHManager.Launcher.FixedActive = true;
				GHManager.ItemList.HideItemList();
				GHManager.ItemList.TopMost = false;
				GHManager.Launcher.TopMost = false;
				GroupMenuInitialize();
				groupmenu.Show(icon.control, e.Location);
				GHManager.ItemList.TopMost = true;
				GHManager.Launcher.TopMost = true;
				GHManager.Launcher.BringToFront();
				GHManager.Launcher.FixedActive = false;
			}
		}

		/// グループアイコンに一定時間乗せた時のイベント
		/// </summary>
		private void Group_Control_Timer(object sender, EventArgs e) {
			timer.Stop();
			GroupManager.ShowItemList(this);
		}

		/// <summary>
		/// グループの更新
		/// </summary>
		/// <returns></returns>
		public bool GroupUpdate() {

			for (int i = 0; i < Items.Count; ++i) {
				if (!Items[i].UpdateItem()) {
					DeleteItem(Items[i]);
				}
			}

			SetGroupIcon();

			if (Items.Count <= 0) {
				return false;
			}
			else {
				return true;
			}

		}
		
		/// <summary>
		/// アイテムリストにアイテムを追加
		/// </summary>
		public void AddItems() {
			for (int i = 0; i < Items.Count; ++i) {
				GHManager.ItemList.Controls.Add(Items[i].icon.control);
			}
		}

		/// <summary>
		/// 全てのアイテムのウィンドウの表示・非表示を切り替え
		/// </summary>
		public void SwitchShowOrHide() {

			if (Items.Count <= 0)
				return;

			IntPtr hwnd = (IntPtr)Items[0].Handle;

			if (GHProcess.IsMinimize(hwnd)) {
				for (int i = 0; i < Items.Count; ++i) {
					GHProcess.Normalize((IntPtr)Items[i].Handle);
				}
			}
			else {
				for (int i = 0; i < Items.Count; ++i) {
					GHProcess.Minimize((IntPtr)Items[i].Handle);
				}
			}

		}

		/// <summary>
		/// 選択中のアイテム
		/// </summary>
		/// <returns></returns>
		public int GetActiveItem() {
			for (int i = 0; i < Items.Count; ++i) {
				if (Items[i].icon.control.Focused) {
					return i;
				}
			}
			return -1;
		}

		public bool CheckRange(int idx) {
			return 0 <= idx && idx < Items.Count;
		}

		public int GetActiveIndex() {
			for (int i = 0; i < Items.Count; ++i) {
				if (Items[i].icon.IsEntered) {
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// グループ内ウィンドウをすべてアクティブか最小化にする
		/// </summary>
		/// <param name="show"></param>
		public void ShowOrHideWindows(bool show) {
			if (Items.Count > 0) {
				if (show) {
					for (int i = 0; i < Items.Count; ++i) {
						GHProcess.Normalize((IntPtr)Items[i].Handle);
					}
				}
				else {
					for (int i = 0; i < Items.Count; ++i) {
						GHProcess.Minimize((IntPtr)Items[i].Handle);
					}
				}
			}
		}

		/// <summary>
		/// グループアイコンを更新 (アイテム上位４個まで)
		/// </summary>
		private void SetGroupIcon() {

			int lsize = GHManager.Settings.Style.Launcher.ItemSize;
			int isize = lsize / 2;
			int cnt = Items.Count;
			int x = 0, y = 0;
			System.Drawing.Drawing2D.CompositingQuality compositingQuality;
			System.Drawing.Drawing2D.SmoothingMode smoothingMode;
			
			if (cnt <= 0)
				return;

			if (GHManager.Settings.DrawQuality == 2) {
				compositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				smoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			}
			else if (GHManager.Settings.DrawQuality == 1) {
				compositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;
				smoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			}
			else {
				compositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
				smoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
			}

			using (Bitmap bmp = new Bitmap(lsize, lsize))
			using (Graphics g = Graphics.FromImage(bmp)) {

				g.CompositingQuality = compositingQuality;
				g.SmoothingMode = smoothingMode;

				g.Clear(Color.FromArgb(0, 0, 0, 0));

				if (GHManager.Settings.GroupIconStyle == 2) {
					Skin.GetSkinImage(SkinImage.Launcher_Item, out Bitmap image);
					g.DrawImage(image, x, y, lsize, lsize);
					image.Dispose();
					image = null;
				}
				else if (GHManager.Settings.GroupIconStyle == 1) {
					g.DrawImage(Items[0].icon.image, 0, 0, lsize, lsize);
				}
				else {
					cnt = (cnt > 4) ? 4 : cnt;

					for (int i = 0; i < cnt; ++i) {
						g.DrawImage(Items[i].icon.image, x, y, isize, isize);
						x += isize;
						if (i == 1) {
							x = 0;
							y += isize;
						}
					}
				}
				
				icon.image = (Bitmap)bmp.Clone();
			}
		}
	}
}
