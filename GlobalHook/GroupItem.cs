using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// グループのアイテム
	/// </summary>
	public class GroupItem {
		
		/// <summary>
		/// ウィンドウハンドル
		/// </summary>
		private long hwnd;
		
		/// <summary>
		/// アイテムのサイズ
		/// </summary>
		//private Rectangle rect_item;

		/// <summary>
		/// テキストのサイズ
		/// </summary>
		//private Rectangle rect_text;

		// *サムネイルのハンドル
		// UpdateLayeredWindow と同時に使えない
		//private IntPtr thumb;

		/// <summary>
		/// アイテムアイコン
		/// </summary>
		public GHIconEx icon;

		/// <summary>
		/// アイテムの名前
		/// </summary>
		//public StringBuilder item_name;

		/// <summary>
		/// アイテムメニュー
		/// </summary>
		public ContextMenu itemmenu;

		public int priority;

		public Rectangle PrevRect;
		
		/// <summary>
		/// ウィンドウハンドル
		/// </summary>
		public long Handle {
			get {
				return hwnd;
			}
		}

		/// <summary>
		/// グループアイテムのコンストラクタ
		/// </summary>
		/// <param name="hwnd">アイテムのウィンドウハンドル</param>
		public GroupItem(ref long hwnd) {

			this.hwnd = hwnd;
			
			if (GHProcess.GetProcessIcon(ref this.hwnd, out Icon ic)) {
				icon = new GHIconEx(ref ic, FormType.ItemList);
			}
			else {
				icon = new GHIconEx(SkinImage.Group_Item, FormType.ItemList);
			}

			ic.Dispose();
			ic = null;
			priority = 0;
			WinAPI.GetWindowText((IntPtr)this.hwnd, icon.item_name, icon.item_name.Capacity);

			icon.control.MouseClick += new MouseEventHandler(Item_Control_Click);
			PrevRect = new Rectangle(0, 0, 0, 0);

			//int i = WinAPI.DwmRegisterThumbnail(icon.control.Handle, (IntPtr)hwnd, out thumb);
			//if (i == 0) {
			//	UpdateThumbnail();
			//}

		}

		public GroupItem(GroupItem item) {
			hwnd = item.Handle;
			icon = new GHIconEx(item.icon);
			PrevRect = item.PrevRect;
			priority = item.priority;
		}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~GroupItem() {
			icon = null;
			if (itemmenu != null) {
				itemmenu.Dispose();
				itemmenu = null;
			}
			//WinAPI.DwmUnregisterThumbnail(thumb);
		}

		/*// <summary>
		/// サムネイルのプロパティの更新
		/// </summary>
		private void UpdateThumbnail() {
			if (thumb != IntPtr.Zero) {
				WinAPI.DwmQueryThumbnailSourceSize(thumb, out WinAPI.PSIZE size);

				WinAPI.DWM_THUMBNAIL_PROPERTIES props = new WinAPI.DWM_THUMBNAIL_PROPERTIES();
				props.dwFlags = WinAPI.DWM_TNP_VISIBLE | WinAPI.DWM_TNP_RECTDESTINATION | WinAPI.DWM_TNP_OPACITY;
				props.fVisible = true;
				props.opacity = 255;

				props.rcDestination = new WinAPI.Rect(icon.control.Left, icon.control.Top, icon.control.Right, icon.control.Bottom);
				if (size.x < icon.control.Width) {
					props.rcDestination.Right = props.rcDestination.Left + size.x;
				}
				if (size.y < icon.control.Height) {
					props.rcDestination.Bottom = props.rcDestination.Top + size.y;
				}

				WinAPI.DwmUpdateThumbnailProperties(thumb, ref props);
			}
		}*/

		/// <summary>
		/// メニューの初期化
		/// </summary>
		private void ItemMenuInitialize() {

			if (itemmenu != null) {
				itemmenu.Dispose();
			}
			itemmenu = new ContextMenu();
			MenuItem menuItem = new MenuItem("グループから削除", ItemMenu_DelItem_Click);
			itemmenu.MenuItems.Add(menuItem);
			menuItem = new MenuItem("閉じる", ItemMenu_Close_Click);
			itemmenu.MenuItems.Add(menuItem);
			//menuItem = new MenuItem("前に移動", ItemMenu_MovePrev_Click);
			//itemmenu.MenuItems.Add(menuItem);
			//menuItem = new MenuItem("次に移動", ItemMenu_MoveNext_Click);
			//itemmenu.MenuItems.Add(menuItem);

		}

		private void ItemMenu_MoveNext_Click(object sender, EventArgs e) {
			GroupManager.GroupItemMove(this, true);
		}

		private void ItemMenu_MovePrev_Click(object sender, EventArgs e) {
			GroupManager.GroupItemMove(this, false);
		}

		/// <summary>
		/// アイテムの削除
		/// </summary>
		private void ItemMenu_DelItem_Click(object sender, EventArgs e) {
			GroupManager.DeleteItem(this);
		}

		/// <summary>
		/// ウィンドウを閉じる
		/// </summary>
		private void ItemMenu_Close_Click(object sender, EventArgs e) {
			WinAPI.SendMessage((IntPtr)hwnd, WinAPI.WM_CLOSE, 0, 0);
		}

		/// <summary>
		/// アイテムの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph) {

			icon.DrawBackGround(ref graph);
			icon.Draw(ref graph);
			//SolidBrush solidBrush = new SolidBrush(GHManager.GetColor());

			//graph.DrawString(item_name.ToString(), GHManager.GetFont(), solidBrush, new Point(0, 0));

		}

		/// <summary>
		/// アイテムの名前とアイコンを更新
		/// </summary>
		/// <returns></returns>
		public bool UpdateItem() {

			icon.item_name.Clear();
			WinAPI.GetWindowText((IntPtr)hwnd, icon.item_name, icon.item_name.Capacity);

			if (GHProcess.GetProcessIcon(ref hwnd, out Icon ic)) {
				icon.UpdateIcon(ref ic);
				if (ic != null) {
					ic.Dispose();
					ic = null;
				}
				return true;
			}
			else {
				return false;
			}

		}

		/// <summary>
		/// アイテムをクリックした時のイベント
		/// </summary>
		private void Item_Control_Click(object sender, MouseEventArgs e) {

			if (e.Button == MouseButtons.Left) {
				// アイテムのウィンドウの表示・非表示の切り替え
				GHProcess.SwitchShowOrHide((IntPtr)hwnd);
			}
			else if (e.Button == MouseButtons.Right) {
				// アイテムメニューの表示
				GHManager.Launcher.FixedActive = true;
				GHManager.ItemList.FixedActive = true;
				GHManager.Launcher.TopMost = false;
				GHManager.ItemList.TopMost = false;
				ItemMenuInitialize();
				Point point = icon.control.PointToClient(Cursor.Position);
				itemmenu.Show(icon.control, point);
				GHManager.Launcher.TopMost = true;
				GHManager.ItemList.TopMost = true;
				GHManager.Launcher.FixedActive = false;
				GHManager.ItemList.FixedActive = false;
			}

		}

	}
}
