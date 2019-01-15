using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GH {

	/// <summary>
	/// ランチャー (マイセットボタンとグループを表示)
	/// </summary>
	public partial class LauncherForm : GHForm {

		public LauncherForm() {
			InitializeComponent();
		}

		/// <summary>
		/// マイセットアイコン
		/// </summary>
		public GHIcon MysetIcon { get; private set; }

		/// <summary>
		/// タスクトレイアイコン
		/// </summary>
		private NotifyIcon notifyIcon;

		/// <summary>
		/// マウスカーソルの座標を保存
		/// </summary>
		private Point MousePoint;

		/// <summary>
		/// ランチャーを移動しているか
		/// </summary>
		private bool LauncherMoving = false;
		
		/// <summary>
		/// ウィンドウプロシージャ
		/// </summary>
		/// <param name="m">Windowsメッセージ</param>
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);

			switch (m.Msg) {

				case WinAPI.WM_COPYDATA: // DLLからデータを受信
										 // 受信したデータを元の型にキャストして、追加する
										 //WinAPI.COPYDATASTRUCT cds = (WinAPI.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(WinAPI.COPYDATASTRUCT));

					//WinAPI.Neighbor neighbor = (WinAPI.Neighbor)Marshal.PtrToStructure(cds.lpData, typeof(WinAPI.Neighbor));
					//GroupManager.AddItem(ref neighbor.parent, ref neighbor.child);

					WinAPI.COPYDATASTRUCT_LONG_ARRAY cds = (WinAPI.COPYDATASTRUCT_LONG_ARRAY)Marshal.PtrToStructure(m.LParam, typeof(WinAPI.COPYDATASTRUCT_LONG_ARRAY));
					StringBuilder stringBuilder = new StringBuilder(8);
					long[] dest = new long[255];
					for (int i = 0, j = 0; i < cds.lpData.Length; ++i) {
						if (cds.lpData[i] == 0) continue;
						if (WinAPI.GetWindowText((IntPtr)cds.lpData[i], stringBuilder, stringBuilder.Capacity) == 0) continue;
						dest[j++] = cds.lpData[i];
					}

					GroupManager.AddItems(ref dest);

					break;

				case WinAPI.WM_HOTKEY:
					ShortcutProc.RunAction((int)m.WParam);
					break;

				default:
					break;
			}
		}

		#region イベント

		/// <summary>
		/// ランチャーの読み込み時のイベント
		/// </summary>
		private void Launcher_Load(object sender, EventArgs e) {

			Text += "-" + Application.ProductName + "-" + Application.ProductVersion;
			
			// タスクトレイに追加
			AddNotifyIcon();

			// マイセットアイコンの初期化＆追加
			MysetIcon = new GHIcon(SkinImage.Myset_Icon, FormType.MysetList);
			MysetIcon.control.MouseClick += (s, a) => {
				if (a.Button == MouseButtons.Left) {
					if (GHManager.MysetList.FormVisible) {
						GHManager.MysetList.MouseActive = false;
						GHManager.MysetList.MysetList_Hide();
					}
					else {
						GHManager.MysetList.MouseActive = true;
						GHManager.MysetList.MysetList_Show();
					}
				}
			};
			Controls.Add(MysetIcon.control);
			
			// フック開始
			if (!Dll.StartHook(Text)) {
				Application.Exit();
			}

			// ランチャーの位置・サイズを設定
			Size = new Size(GHManager.Settings.Style.Launcher.Width, GHManager.Settings.Style.Launcher.Height);
			SetOffset(GHManager.Settings.Launcher.Offset);

			GHManager.RegistHotKey(Handle);
			// タイマー開始
			DrawTimer.Start();
			UpdateTimer.Start();
		}

		/// <summary>
		/// ランチャーが終了する時のイベント (後処理)
		/// </summary>
		private void Launcher_FormClosing(object sender, FormClosingEventArgs e) {
			// フックを解除
			Dll.EndHook();
			MysetManager.SaveMyset();
			GHManager.SaveSetting();
			GHManager.UnregistHotKey(Handle);
		}

		/// <summary>
		/// ランチャー上でマウスボタンを押した時のイベント
		/// </summary>
		private void Launcher_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				MousePoint = new Point(e.X, e.Y);
				if (FormVisible) {
					LauncherMoving = true;
					FixedActive = true;
				}
			}
		}

		/// <summary>
		/// ランチャー上でマウスボタンを離した時のイベント
		/// </summary>
		private void Launcher_MouseUp(object sender, MouseEventArgs e) {
			if (LauncherMoving) {
				LauncherMoving = false;
				FixedActive = false;
			}
		}

		/// <summary>
		/// ランチャー上でマウスカーソルを移動した時のイベント
		/// </summary>
		private void Launcher_MouseMove(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {

				int temp = 0;

				// 縦向きは縦、横向きは横の方向にのみ移動する
				if (GHManager.IsVertical) {
					temp = Top + e.Y - MousePoint.Y;
					if (temp < GHManager.ScreenSize.Top) {
						temp = GHManager.ScreenSize.Top;
					}
					else if (temp > GHManager.ScreenSize.Bottom - Height) {
						temp = GHManager.ScreenSize.Bottom - Height;
					}
					Top = temp;
					GHManager.Settings.Launcher.Offset = Top;
				}
				else {
					temp = Left + e.X - MousePoint.X;
					if (temp < GHManager.ScreenSize.Left) {
						temp = GHManager.ScreenSize.Left;
					}
					else if (temp > GHManager.ScreenSize.Right + Width) {
						temp = GHManager.ScreenSize.Right + Width;
					}
					Left = temp;
					GHManager.Settings.Launcher.Offset = Left;
				}
			}
		}

		/// <summary>
		/// 描画を行うタイマーイベント ループ最速
		/// </summary>
		private void DrawTimer_Tick(object sender, EventArgs e) {
			if (MouseButtons != 0) {
				GHManager.MysetList.KeyboardActive = false;
				GHManager.ItemList.KeyboardActive = false;
				KeyboardActive = false;
			}
			// 描画
			GHFormDraw();
			GHManager.ItemList.GHFormDraw();
			GHManager.MysetList.GHFormDraw();
		}

		/// <summary>
		/// 更新を行うタイマーイベント 低速 500ms
		/// </summary>
		private void UpdateTimer_Tick(object sender, EventArgs e) {
			// 設定の確定
			GHManager.Commit();
			// 更新
			GHFormUpdate();
			GHManager.ItemList.GHFormUpdate();
			GHManager.MysetList.GHFormUpdate();

			if (FormVisible) {
				// グループとグループアイテムの情報を更新
				GroupManager.UpdateGroup();
			}
		}

		/// <summary>
		/// メニューの 設定 をクリックした時のイベント
		/// </summary>
		public void MenuItem_Config_Click(object sender, EventArgs e) {

			bool fix = FixedActive;
			FixedActive = true;

			// 設定ウィンドウはモーダルウィンドウで開く
			using (Config conf = new Config()) {
				conf.ShowDialog();
			}

			FixedActive = fix;
		}

		/// <summary>
		/// メニューの 終了 をクリックした時のイベント
		/// </summary>
		private void MenuItem_Close_Click(object sender, EventArgs e) {
			Close();
		}

		#endregion

		#region オーバーライド

		/// <summary>
		/// Active状態を更新
		/// </summary>
		protected override void Update_Visible() {
			Point curPos = GHManager.CursorPosition;

			// ランチャーの位置に対応した範囲
			int[,] range = new int[,] {
				{ curPos.X, GHManager.Settings.Launcher.ReactRange },
				{ curPos.Y, GHManager.Settings.Launcher.ReactRange},
				{ GHManager.ScreenSize.Width - GHManager.Settings.Launcher.ReactRange, curPos.X },
				{ GHManager.ScreenSize.Height - GHManager.Settings.Launcher.ReactRange, curPos.Y }
			};

			// 認識範囲内なら表示
			if (range[GHManager.Settings.Launcher.Pos, 0] <= range[GHManager.Settings.Launcher.Pos, 1]) {
				MouseActive = true;
			}
			// ランチャー上にカーソルがあるなら表示
			else if (GHManager.Contains.Launcher) {
				MouseActive = true;
			}
			else {
				MouseActive = false;
			}
			int cnt = 0;
			for (int i = 0; i < GroupManager.GroupList.Count; ++i) {
				if (GroupManager.GroupList[i].icon.IsEntered) {
					SelectIndex = i + 1;
					cnt++;
				}
			}
			if (cnt == 0) {
				SelectIndex = 0;
			}
		}

		protected override void Update_Timer() {
			// 範囲内の場合は表示　範囲外は非表示タイマーを開始
			if (MouseActive || KeyboardActive || FixedActive) {
				if (MouseActive && GHManager.Settings.Launcher.ShownMouseButton) {
					if (MouseButtons == MouseButtons.None) {
						return;
					}
				}

				if (!FormVisible) {
					ShowAnimation();
				}
			}
			else {
				if (FormVisible) {
					if (GHManager.Contains.AnyContain()) {
						HideTimer.Stop();
					}
					else {
						HideTimer.Start();
					}
				}
			}
		}

		protected override void Update_Bounds() {
			int cnt = GroupManager.GroupList.Count + 1;
			GHBaseStyle style = GHManager.Settings.Style.Launcher;
			bool isVertical = GHManager.IsVertical;

			// 縦向きまたは横向きのサイズを設定
			int w = isVertical ? style.Width : style.Height;
			int h = style.ItemSize * cnt + style.ItemSpace * (cnt - 1)
						+ (isVertical ? style.WindowPadding.HSize : style.WindowPadding.HSize);
			int x = Left;
			int y = Top;

			if (!isVertical) {
				int t = w;
				w = h;
				h = t;
			}

			// 表示位置修正
			if (GHManager.IsVertical && (Bottom <= 0 || GHManager.ScreenSize.Bottom <= y)) {
				y = (GHManager.ScreenSize.Height - Height) / 2;
			}
			else if (!GHManager.IsVertical && (Right <= 0 || GHManager.ScreenSize.Right <= x)) {
				x = (GHManager.ScreenSize.Width - Width) / 2;
			}

			Bounds = new Rectangle(x, y, w, h);
		}

		protected override void Update_ItemPos() {
			GHBaseStyle style = GHManager.Settings.Style.Launcher;

			Rectangle rect = new Rectangle(style.WindowPadding.Left, style.WindowPadding.Top, style.ItemSize, style.ItemSize);

			// マイセットアイコンの位置・サイズを設定
			MysetIcon.SetRect(ref rect);

			// グループの位置・サイズを設定
			GroupManager.SetRectGroups(ref rect);
		}

		protected override void Update_SlideMaxAndMin() {
			// ループ毎に最大値と最小値を設定
			int max = GHManager.ScreenSize.Left;
			int min = max - Width;
			if (GHManager.Settings.Launcher.Pos == 1)
				max = GHManager.ScreenSize.Top; min = max - Height;
			if (GHManager.Settings.Launcher.Pos == 2)
				max = GHManager.ScreenSize.Right; min = max - Width;
			if (GHManager.Settings.Launcher.Pos == 3)
				max = GHManager.ScreenSize.Bottom; min = max - Height;

			GHFormDestPosition = max;
			GHFormSrcPosition = min;
		}

		protected override bool Hide_Criteria() {
			if (GHManager.Contains.AnyContain()) {
				return false;
			}

			if (!IsAnimation) {
				if (GHManager.ItemList.FormVisible) {
					GHManager.ItemList.FixedActive = false;
					GHManager.ItemList.HideAnimation();
				}
				if (GHManager.MysetList.FormVisible) {
					GHManager.MysetList.FixedActive = false;
					GHManager.MysetList.HideAnimation();
				}
				return true;
			}
			return false;
		}

		protected override void DrawForm(ref Graphics graph) {

			base.DrawForm(ref graph);

			// 背景描画
			Skin.DrawingSkinImageRepeat(ref graph, SkinImage.Launcher_Background, new Rectangle(0, 0, Size.Width, Size.Height));

			// マイセットアイコン
			MysetIcon.DrawBackGround(ref graph);
			MysetIcon.Draw(ref graph);

			// グループ
			GroupManager.Draw(ref graph);
		}

		#endregion

		/// <summary>
		/// ランチャーを表示位置の中央に移動する
		/// </summary>
		public void MovingCenter() {
			uint pos = GHManager.Settings.Launcher.Pos;

			if (GHManager.IsVertical) {
				Top = (GHManager.ScreenSize.Height - Height) / 2;
				GHManager.Settings.Launcher.Offset = Top;
				Left = FormVisible ? pos == 0 ? GHManager.ScreenSize.Left : GHManager.ScreenSize.Right - Width : -Width;
			}
			else {
				Left = (GHManager.ScreenSize.Width - Width) / 2;
				GHManager.Settings.Launcher.Offset = Left;
				Top = FormVisible ? pos == 1 ? GHManager.ScreenSize.Top : GHManager.ScreenSize.Bottom - Height : -Height;
			}
		}

		/// <summary>
		/// オフセットの位置に移動
		/// </summary>
		/// <param name="offset">オフセット</param>
		public void SetOffset(int offset) {
			uint pos = GHManager.Settings.Launcher.Pos;
			bool check = true;

			if (GHManager.IsVertical) {
				if (offset + Height > GHManager.ScreenSize.Height || offset < 0) {
					check = false;
				}
				else {
					Top = offset;
					Left = FormVisible ? pos == 0 ? GHManager.ScreenSize.Left : GHManager.ScreenSize.Right - Width : -Width;
				}
			}
			else {
				if (offset + Width > GHManager.ScreenSize.Width || offset < 0) {
					check = false;
				}
				else {
					Left = offset;
					Top = FormVisible ? pos == 1 ? GHManager.ScreenSize.Top : GHManager.ScreenSize.Bottom - Height : -Height;
				}
			}

			if (!check) MovingCenter();
		}

		/// <summary>
		/// タスクトレイアイコンを追加
		/// </summary>
		private void AddNotifyIcon() {
			notifyIcon = new NotifyIcon(components) {
				Icon = Icon,
				Visible = true,
				Text = "GlobalHook",
				ContextMenu = notifyIconMenu
			};

			MenuItem menuItem = new MenuItem("設定(&S)", MenuItem_Config_Click);
			notifyIconMenu.MenuItems.Add(menuItem);
			menuItem = new MenuItem("-");
			notifyIconMenu.MenuItems.Add(menuItem);
			menuItem = new MenuItem("終了(&X)", MenuItem_Close_Click);
			notifyIconMenu.MenuItems.Add(menuItem);
		}

		protected override int GetItemCount() {
			return GroupManager.GroupList.Count + 1;
		}

		public void SelectNextGroup(int next = 1) {
			int cnt = GetItemCount();
			SelectIndex += next;
			if (SelectIndex <= 0) {
				SelectIndex = cnt - 1;
			}
			else if (SelectIndex >= cnt) {
				SelectIndex = 1;
			}
		}

		protected override void DrawPriorUpdate() {
			GHManager.UpdateScrSize();
			if (KeyboardActive) {
				if (SelectIndex <= 0) {
					SelectIndex = 0;
					MysetIcon.control.Focus();
				}
				else {
					if (GroupManager.AtIndex(SelectIndex - 1)) {
						GroupManager.GroupList[SelectIndex - 1].icon.control.Focus();
					}
				}
			}
		}

	}

}
