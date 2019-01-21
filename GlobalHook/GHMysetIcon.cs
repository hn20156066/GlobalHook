using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GH {

	// マイセットのアイコン
	public class GHMysetIcon : GHIcon {

		public bool OpenMysetList { private get; set; }

		public GHMysetIcon() : base() {
			Init();
		}

		public GHMysetIcon(SkinImage skin, FormType windowType) : base(skin, windowType) {
			Init();
		}


		public GHMysetIcon(ref Icon hIcon, FormType windowType) : base(ref hIcon, windowType) {
			Init();
		}

		public GHMysetIcon(Bitmap bmp, FormType windowType) : base(ref bmp, windowType) {
			Init();
		}
		
		private void Init() {
			OpenMysetList = false;
		}

		/// <summary>
		/// アイコンの描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public new void Draw(ref Graphics graph) {

			GHPadding padding = GHManager.GetStyle(windowType).ItemPadding;

			Rectangle rect = new Rectangle {
				X = control.Location.X + padding.Left,
				Y = control.Location.Y + padding.Top,
				Width = control.Width - padding.WSize,
				Height = control.Height - padding.HSize
			};

			SkinImage skin = OpenMysetList ? SkinImage.Myset_Open_Icon : SkinImage.Myset_Icon;
			Skin.GetSkinImage(skin, out Bitmap bmp);
			graph.DrawImage(bmp, rect);
			bmp.Dispose();
			bmp = null;
		}

	}
}
