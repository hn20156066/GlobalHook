using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace GH {

	/// <summary>
	/// マイセット
	/// </summary>
	public class Myset {

		// マイセットアイテム
		public List<MysetItem> MysetItems;

		// マイセットのアイコン
		public GHIcon icon;

		// マイセットメニュー
		public ContextMenu mysetmenu;

		// アイテムが表示される時間
		private Timer timer;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="group">登録するグループ</param>
		public Myset(Group group) {

			MysetItems = new List<MysetItem>(10);
			icon = group.icon.Clone(FormType.MysetList);
			icon.control.MouseEnter += new EventHandler(Myset_Control_Enter);
			icon.control.MouseLeave += new EventHandler(Myset_Control_Leave);
			icon.control.MouseClick += new MouseEventHandler(Myset_Control_Click);

			timer = new Timer {
				Interval = 400
			};
			timer.Tick += new EventHandler(Myset_Control_Timer);

			foreach (var item in group.GroupItems) {
				AddMysetItem(item);
			}
		}

		public Myset(string[] path) {
			MysetItems = new List<MysetItem>(10);
			Skin.GetSkinImage(SkinImage.Myset_Item, out Bitmap image);
			icon = new GHIcon(ref image, FormType.MysetList);
			icon.control.MouseEnter += new EventHandler(Myset_Control_Enter);
			icon.control.MouseLeave += new EventHandler(Myset_Control_Leave);
			icon.control.MouseClick += new MouseEventHandler(Myset_Control_Click);
			timer = new Timer {
				Interval = 400
			};
			timer.Tick += new EventHandler(Myset_Control_Timer);
			foreach (var item in path) {
				AddMysetItem(item);
			}

			SetMysetIcon();
		}

		/// <summary>
		/// マイセットにアイテムを登録する
		/// </summary>
		/// <param name="item">登録するアイテム</param>
		private void AddMysetItem(GroupItem item) {
			MysetItem mysetItem = new MysetItem(item);
			MysetItems.Add(mysetItem);
		}

		private void AddMysetItem(string path) {
			MysetItem mysetItem = new MysetItem(path);
			MysetItems.Add(mysetItem);
		}

		/// <summary>
		/// マイセットメニューの初期化
		/// </summary>
		private void MysetMenuInitialize() {
			mysetmenu = new ContextMenu();
			MenuItem item = new MenuItem("マイセット削除", MenuItem_DelMyset_Click);
			mysetmenu.MenuItems.Add(item);
		}

		/// <summary>
		/// マイセット削除
		/// </summary>
		private void MenuItem_DelMyset_Click(object sender, EventArgs e) {
			MysetManager.DeleteMyset(this);
		}

		/// <summary>
		/// マイセットアイコンの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph) {
			icon.DrawBackGround(ref graph);
			icon.Draw(ref graph);
		}

		/// <summary>
		/// マイセットアイテムの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void DrawItems(ref Graphics graph) {
			foreach (var item in MysetItems) {
				item.Draw(ref graph);
			}
		}

		/// <summary>
		/// マイセットアイテムをウィンドウに追加
		/// </summary>
		public void AddItems() {
			foreach (var item in MysetItems) {
				GHManager.ItemList.Controls.Add(item.icon.control);
			}
		}

		/// <summary>
		/// マイセット内からアイテムを削除
		/// </summary>
		/// <param name="item">削除するアイテム</param>
		public void DeleteItem(MysetItem item) {
			item.icon.control.Dispose();
			MysetItems.Remove(item);
			if (MysetItems.Count <= 0) {
				MysetManager.DeleteMyset(this);
			}
		}

		public bool DeleteItem(int idx) {
			if (0 <= idx && idx < MysetItems.Count) {
				MysetItems[idx].icon.control.Dispose();
				MysetItems.RemoveAt(idx);
				if (MysetItems.Count <= 0) {
					MysetManager.DeleteMyset(this);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// マイセット内のアイテムの位置を設定
		/// </summary>
		/// <param name="rect">基準の位置・サイズ</param>
		public void SetRectItems(ref Rectangle rect) {
			int itemSize = GHManager.Settings.Style.ItemList.ItemSize;
			int pad = GHManager.Settings.Style.ItemList.ItemSpace;
			int column = GHManager.Settings.Style.ItemList.Column;
			int baseX = rect.X;

			for (int i = 0; i < MysetItems.Count; ++i) {

				MysetItems[i].icon.SetRect(ref rect);
				rect.X += itemSize + pad;

				if (i % column == column - 1) {
					rect.X = baseX;
					rect.Y += itemSize + pad;
				}
			}
		}
		
		/// <summary>
		/// 他のアイテムを表示している場合はそのまま表示
		/// 表示していない場合はタイマー開始
		/// </summary>
		private void Myset_Control_Enter(object sender, EventArgs e) {
			if (GHManager.ItemList.Item_Num != -1 && GHManager.ItemList.FormVisible) {
				if (MysetManager.SetMysetNum(this)) {
					timer.Stop();
				}
			}
			else {
				timer.Start();
			}
		}

		/// <summary>
		/// アイテムから離れた時タイマー停止
		/// </summary>
		private void Myset_Control_Leave(object sender, EventArgs e) {
			timer.Stop();
		}

		private void MysetItemsExecute(long[] hwnds) {
			for (int i = 0; i < MysetItems.Count; ++i) {
				hwnds[i] = MysetItems[i].Execute();
			}
		}

		public void ExecuteItems() {
			Myset_Control_Click(null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
		}

		/// <summary>
		/// 選択中のアイテム
		/// </summary>
		/// <returns></returns>
		public int GetActiveItem() {
			for (int i = 0; i < MysetItems.Count; ++i) {
				if (MysetItems[i].icon.control.Focused) {
					return i;
				}
			}
			return -1;
		}

		public bool AtIndex(int idx) {
			return 0 <= idx && idx < MysetItems.Count;
		}

		/// <summary>
		/// 左クリック：マイセット内全てのアイテムを起動し新規グループに追加
		/// 右クリック：メニュー表示
		/// </summary>
		private void Myset_Control_Click(object sender, MouseEventArgs e) {
			timer.Stop();

			if (e.Button == MouseButtons.Left) {
				// マイセットを復元
				long[] hwnds = new long[MysetItems.Count];
				MysetItemsExecute(hwnds);
				GroupManager.AddGroupAndItems(ref hwnds);

				GHManager.ItemList.HideItemList();
			}
			else if (e.Button == MouseButtons.Right) {
				// マイセットのメニュー表示
				GHManager.ItemList.HideItemList();
				GHManager.Launcher.FixedActive = true;
				GHManager.MysetList.FixedActive = true;
				MysetMenuInitialize();
				Point point = icon.control.PointToClient(Cursor.Position);
				mysetmenu.Show(icon.control, point);
				GHManager.MysetList.FixedActive = false;
				GHManager.Launcher.FixedActive = false;
			}
		}

		/// <summary>
		/// 時間が経ったらアイテムを表示
		/// </summary>
		private void Myset_Control_Timer(object sender, EventArgs e) {
			timer.Stop();
			MysetManager.SetMysetNum(this);
		}

		public List<MysetXml.MysetItemInfo> GetItemsInfo() {
			List<MysetXml.MysetItemInfo> infos = new List<MysetXml.MysetItemInfo>();
			MysetItems.ToList().ForEach(i => infos.Add(i.GetItemInfo()));
			return infos;
		}

		public bool UpdateMysetIcon() {
			if (MysetItems.Count <= 0) {
				return false;
			}

			SetMysetIcon();

			return true;
		}

		private void SetMysetIcon() {
			int lsize = GHManager.Settings.Style.MysetList.ItemSize;
			int isize = lsize / 2;
			int cnt = MysetItems.Count;
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

				if (GHManager.Settings.MysetIconStyle == 2) {
					Skin.GetSkinImage(SkinImage.Myset_Item, out Bitmap img);
					g.DrawImage(img, x, y, lsize, lsize);
					img.Dispose();
					img = null;
				}
				else if (GHManager.Settings.MysetIconStyle == 1) {
					g.DrawImage(MysetItems[0].icon.image, 0, 0, lsize, lsize);
				}
				else {
					cnt = (cnt > 4) ? 4 : cnt;

					for (int i = 0; i < cnt; ++i) {
						g.DrawImage(MysetItems[i].icon.image, x, y, isize, isize);
						x += isize;
						if (i == 1) {
							x = 0;
							y += isize;
						}
					}
				}

				icon.image.Dispose();
				icon.image = null;
				icon.image = (Bitmap)bmp.Clone();
			}
		}

	}
}
