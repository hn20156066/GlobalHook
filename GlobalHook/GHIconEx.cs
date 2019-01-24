using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GH {

	// マイセットのアイコン
	public class GHIconEx : GHIcon {

		public bool opened { private get; set; }

		public SkinImage OpendImage { get; set; }
		public SkinImage ClosedImage { get; set; }

		public GHIconEx() : base() {
			Init();
		}

		public GHIconEx(SkinImage opened, SkinImage closed, FormType windowType) : base(closed, windowType) {
			OpendImage = opened;
			ClosedImage = closed;
			Init();
		}

		public GHIconEx(SkinImage skin, FormType windowType) : base(skin, windowType) {
			OpendImage = ClosedImage = skin;
			Init();
		}


		public GHIconEx(ref Icon hIcon, FormType windowType) : base(ref hIcon, windowType) {
			OpendImage = ClosedImage = SkinImage.Kind_Null;
			Init();
		}

		public GHIconEx(Bitmap bmp, FormType windowType) : base(ref bmp, windowType) {
			OpendImage = ClosedImage = SkinImage.Kind_Null;
			Init();
		}
		
		private void Init() {
			opened = false;
		}

		/// <summary>
		/// アイコンのクローンを生成（簡易版・分類指定）
		/// </summary>
		/// <param name="windowType">画像の分類</param>
		/// <returns></returns>
		public new GHIconEx Clone(FormType windowType) {

			GHIconEx icon = new GHIconEx(image, windowType);
			icon.OpendImage = OpendImage;
			icon.ClosedImage = ClosedImage;
			icon.opened = opened;

			return icon;

		}

		/// <summary>
		/// アイコンの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public void Draw(ref Graphics graph, bool open) {

			GHPadding padding = GHManager.GetStyle(windowType).ItemPadding;

			Rectangle rect = new Rectangle {
				X = control.Location.X + padding.Left,
				Y = control.Location.Y + padding.Top,
				Width = control.Width - padding.WSize,
				Height = control.Height - padding.HSize
			};

			if (open) {
				SkinImage skin = opened ? OpendImage : ClosedImage;
				Skin.GetSkinImage(skin, out Bitmap bmp);
				graph.DrawImage(bmp, rect);
				bmp.Dispose();
				bmp = null;
			}
			else {
				Draw(ref graph);
			}
		}

	}
}
