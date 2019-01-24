using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// マイセットのアイテム
	/// </summary>
	public class MysetItem {

		// 実行パス
		private StringBuilder ExePath;

		// 実行パス取得
		public StringBuilder ItemPath => ExePath;

		// アイコン
		public GHItemIcon icon;

		// メニュー
		public ContextMenu itemmenu;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="item">登録するアイテム</param>
		public MysetItem(GroupItem item) {
			icon = item.icon.Clone(FormType.MysetList);
			ExePath = new StringBuilder(255);
			long hwnd = item.Handle;
			GHProcess.GetProcessPath(ref hwnd, out ExePath);

			icon.item_name.Clear();
			icon.item_name = new StringBuilder(System.IO.Path.GetFileNameWithoutExtension(ExePath.ToString()));

			icon.control.MouseClick += new MouseEventHandler(Item_Control_Click);
		}

		public MysetItem(string path) {
			ExePath = new StringBuilder(path);

			if (GHProcess.GetPathIcon(ref ExePath,out Icon ic)) {
				icon = new GHItemIcon(ref ic, FormType.ItemList);
			}
			else {
				icon = new GHItemIcon(SkinImage.Group_Item, FormType.ItemList);
			}

			ic.Dispose();
			ic = null;

			icon.item_name.Clear();
			icon.item_name = new StringBuilder(System.IO.Path.GetFileNameWithoutExtension(ExePath.ToString()));

			icon.control.MouseClick += new MouseEventHandler(Item_Control_Click);
		}

		~MysetItem() {
			icon = null;
			if (itemmenu != null) {
				//itemmenu.Dispose();
				itemmenu = null;
			}
		}

		/// <summary>
		/// メニュー初期化
		/// </summary>
		private void ItemMenuInitialize() {
			if (itemmenu != null) {
				//itemmenu.Dispose();
			}
			itemmenu = new ContextMenu();
			MenuItem menuItem = new MenuItem("マイセットから削除", ItemMenu_DelItem_Click);
			itemmenu.MenuItems.Add(menuItem);
		}

		/// <summary>
		/// マイセットアイテム削除
		/// </summary>
		private void ItemMenu_DelItem_Click(object sender, EventArgs e) {
			MysetManager.DeleteItem(this);
		}

		/// <summary>
		/// アイテムの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph) {
			icon.DrawBackGround(ref graph);
			icon.Draw(ref graph);
		}
		
		/// <summary>
		/// アイテムのファイルを起動
		/// </summary>
		/// <returns></returns>
		public long Execute() {
			return GHProcess.StartProcess(ExePath, null/*Arg*/, new Rectangle(0,0,0,0)/*rect*/);
		}

		/// <summary>
		/// 左クリック：アイテムを
		/// 右クリック：
		/// </summary>
		private void Item_Control_Click(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				Execute();
			}
			else if (e.Button == MouseButtons.Right) {
				GHManager.Launcher.FixedActive = true;
				GHManager.MysetList.FixedActive = true;
				GHManager.ItemList.FixedActive = true;
				GHManager.ItemList.TopMost = false;
				GHManager.MysetList.TopMost = false;
				GHManager.Launcher.TopMost = false;
				ItemMenuInitialize();
				Point point = icon.control.PointToClient(Cursor.Position);
				itemmenu.Show(icon.control, point);
				GHManager.ItemList.TopMost = true;
				GHManager.MysetList.TopMost = true;
				GHManager.Launcher.TopMost = true;
				GHManager.Launcher.FixedActive = false;
				GHManager.MysetList.FixedActive = false;
				GHManager.ItemList.FixedActive = false;
			}
		}

		public MysetXml.MysetItemInfo GetItemInfo() {
			MysetXml.MysetItemInfo info = new MysetXml.MysetItemInfo {
				Path = ExePath.ToString(),
			};
			return info;
		}

	}
}
