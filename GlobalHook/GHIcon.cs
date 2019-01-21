using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// アイコン
	/// </summary>
	public class GHIcon {

		/// <summary>
		/// 表示する画像
		/// </summary>
		public Bitmap image;

		/// <summary>
		/// 表示位置設定やイベントハンドラの役割
		/// </summary>
		public Control control;

		/// <summary>
		/// カーソルが乗っているか
		/// </summary>
		protected bool entered;

		/// <summary>
		/// クリックしているか
		/// </summary>
		protected bool selected;

		public bool IsEntered => entered;

		/// <summary>
		/// アイコンがあるウィンドウの種類
		/// </summary>
		protected readonly FormType windowType;

		public GHIcon() {
			windowType = FormType.ItemList;
			Skin.GetSkinImage(SkinImage.Group_Item, out image);

			Init();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="skin">画像の種類</param>
		/// <param name="windowType">画像の分類</param>
		public GHIcon(SkinImage skin, FormType windowType) {

			this.windowType = windowType;
			Skin.GetSkinImage(skin, out image);

			Init();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="hIcon">アイコン</param>
		/// <param name="windowType">画像の分類</param>
		public GHIcon(ref Icon hIcon, FormType windowType) {

			this.windowType = windowType;
			using (Bitmap bmp = hIcon.ToBitmap()) {
				bmp.MakeTransparent(Color.FromArgb(0, 0, 0));
				image = (Bitmap)bmp.Clone();
			}

			Init();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="bmp">画像</param>
		/// <param name="windowType">画像の分類</param>
		public GHIcon(ref Bitmap bmp, FormType windowType) {
			
			image = (Bitmap)bmp.Clone();
			
			Init();
		}

		~GHIcon() {
			//if (image != null) {
			//	image.Dispose();
			//	image = null;
			//}
			//if (!control.IsDisposed) {
			//	control.Dispose();
			//	control = null;
			//}
		}

		/// <summary>
		/// アイコンの初期化
		/// </summary>
		private void Init() {
			
			entered = false;
			selected = false;

			control = new Control {
				Padding = new Padding(0),
				Margin = new Padding(0)
			};

			control.MouseEnter += new EventHandler(Control_Enter);
			control.MouseLeave += new EventHandler(Control_Leave);
			control.MouseDown += new MouseEventHandler(Control_Down);
			control.MouseUp += new MouseEventHandler(Control_Up);
			control.Enter += new EventHandler(Control_Enter);
			control.Leave += new EventHandler(Control_Leave);
			control.KeyDown += new KeyEventHandler(Control_KeyDown);
			control.KeyUp += new KeyEventHandler(Control_KeyUp);
		}

		/// <summary>
		/// アイコンのクローンを生成（簡易版・分類指定）
		/// </summary>
		/// <param name="windowType">画像の分類</param>
		/// <returns></returns>
		public GHIcon Clone(FormType windowType) {

			GHIcon icon = new GHIcon(ref image, windowType);

			return icon;

		}

		/// <summary>
		/// アイコンの更新
		/// </summary>
		/// <param name="icon">上書きさせるアイコン</param>
		public void UpdateIcon(ref Icon icon) {

			using (Bitmap bmp = icon.ToBitmap()) {
				bmp.MakeTransparent(Color.FromArgb(0, 0, 0));
				image.Dispose();
				image = null;
				image = (Bitmap)bmp.Clone();

				if(icon != null) {
					icon.Dispose();
					icon = null;
				}
			}

		}

		/// <summary>
		/// アイコンの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph) {

			GHPadding padding = GHManager.GetStyle(windowType).ItemPadding;

			Rectangle rect = new Rectangle {
				X = control.Location.X + padding.Left,
				Y = control.Location.Y + padding.Top,
				Width = control.Width - padding.WSize,
				Height = control.Height - padding.HSize
			};

			if (image != null) {
				graph.DrawImage(image, rect);
			}

		}

		/// <summary>
		/// 背景を描画
		/// </summary>
		/// <param name="graph">描画</param>
		public void DrawBackGround(ref Graphics graph) {
			if (selected) {
				Skin.DrawingSkinImage(ref graph, Skin.GetItemBackType(windowType, true), control.Bounds);
			}
			else if (entered) {
				Skin.DrawingSkinImage(ref graph, Skin.GetItemBackType(windowType), control.Bounds);
			}

		}

		/// <summary>
		/// 位置・サイズを設定
		/// </summary>
		/// <param name="rect">設定する位置・サイズ</param>
		public void SetRect(ref Rectangle rect) {

			control.Bounds = rect;

		}

		/// <summary>
		/// 位置・サイズを取得
		/// </summary>
		/// <param name="rect">格納する四角形</param>
		public void GetRect(out Rectangle rect) {

			rect = control.Bounds;

		}

		/// <summary>
		/// 座標がアイコン内にあるかの判定
		/// </summary>
		/// <param name="point">座標（親コントロールからの相対位置）</param>
		/// <returns></returns>
		public bool Contains(ref Point point) {

			return control.Bounds.Contains(point);

		}

		public void Leave() {
			selected = false;
			entered = false;
		}


		private void Control_Enter(object vender, EventArgs e) {
			entered = true;
		}

		private void Control_Leave(object vender, EventArgs e) {
			entered = false;
		}

		private void Control_Down(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				selected = true;
			}
		}

		private void Control_Up(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				selected = false;
			}
		}

		private void Control_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				selected = true;
			}
		}

		private void Control_KeyUp(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				selected = false;
			}
		}

	}
}
