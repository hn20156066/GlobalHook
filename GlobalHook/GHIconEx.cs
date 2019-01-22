using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GH {

	/// <summary>
	/// アイコンとテキスト
	/// </summary>
	public class GHIconEx : GHIcon {

		/// <summary>
		/// テキストのサイズ
		/// </summary>
		private Rectangle rect_text;

		/// <summary>
		/// アイコンのサイズ
		/// </summary>
		private Rectangle rect_icon;

		/// <summary>
		/// アイテムの名前
		/// </summary>
		public StringBuilder item_name;

		public GHIconEx() : base() {
			Init();
		}

		public GHIconEx(GHIconEx icon) :base(icon) {
			rect_text = icon.rect_text;
			rect_icon = icon.rect_icon;
			item_name = new StringBuilder(64);
			item_name.Append(icon.item_name);
		}

		public GHIconEx(SkinImage skin, FormType windowType) : base(skin, windowType) {
			Init();
		}


		public GHIconEx(ref Icon hIcon, FormType windowType) : base(ref hIcon, windowType) {
			Init();
		}

		public GHIconEx(Bitmap bmp, FormType windowType) : base(ref bmp, windowType) {
			Init();
		}

		private void Init() {
			rect_icon = new Rectangle(0, 0, GHManager.Settings.Style.ItemList.IconSize, GHManager.Settings.Style.ItemList.IconSize);
			rect_text = new Rectangle(0, GHManager.Settings.Style.ItemList.IconSize, 0, (int)Math.Floor((decimal)GHManager.Settings.Style.ItemList.FontSize));
			item_name = new StringBuilder(64);
		}

		public new GHIconEx Clone(FormType windowType) {
			GHIconEx iconEx = new GHIconEx(image, windowType);

			return iconEx;
		}

		public new void Draw(ref Graphics graph) {
			GHPadding padding = GHManager.GetStyle(windowType).ItemPadding;
			GHPadding text_pad = GHManager.Settings.Style.ItemList.TextPadding;

			Rectangle iconrc = new Rectangle {
				X = control.Left + (control.Width - GHManager.Settings.Style.ItemList.IconSize) / 2,
				Y = control.Top + padding.Top,
				Width = GHManager.Settings.Style.ItemList.IconSize,
				Height = GHManager.Settings.Style.ItemList.IconSize
			};
			rect_text = new Rectangle {
				X = control.Left + text_pad.Left,
				Y = iconrc.Bottom + padding.Bottom + text_pad.Top,
				Width = control.Width - text_pad.WSize,
				Height = control.Height - iconrc.Height - padding.Top - text_pad.HSize - GHManager.Settings.Style.ItemList.WindowPadding.Top
			};
			SolidBrush solidBrush = new SolidBrush(GHManager.GetColor());
			graph.DrawImage(image, iconrc);
			StringFormat format = new StringFormat {
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center,
				Trimming = StringTrimming.EllipsisCharacter
			};

			graph.DrawString(item_name.ToString(), GHManager.GetFont(), solidBrush, rect_text, format);
			
			solidBrush.Dispose();
			format.Dispose();
		}

	}
}
