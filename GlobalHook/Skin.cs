using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace GH {

	/// <summary>
	/// スキンに関するクラス（アイコンなどの画像）
	/// </summary>
	public static class Skin {
		
		/// <summary>
		/// スキンの画像を保管
		/// </summary>
		public static Bitmap[] Images = new Bitmap[Enum.GetNames(typeof(SkinImage)).Length];

		public static void LoadSkinImages() {
			StringBuilder msg = new StringBuilder(1024);
			for (int i = 0; i < (int)SkinImage.Kind_Null; ++i) {
				SkinImage skinImage = (SkinImage)Enum.ToObject(typeof(SkinImage), i);
				LoadPath(skinImage, out Images[i], ref msg);
			}
			if (msg.Length > 0) {
				MessageBox.Show(msg.ToString(), "File Not Found.");
			}
		}

		public static void LoadTempSkinImages() {
			StringBuilder msg = new StringBuilder(1024);
			for (int i = 0; i < (int)SkinImage.Kind_Null; ++i) {
				SkinImage skinImage = (SkinImage)Enum.ToObject(typeof(SkinImage), i);
				LoadPath(skinImage, out Images[i], ref msg, GHManager.TempSettings.SkinName);
			}
			if (msg.Length > 0) {
				MessageBox.Show(msg.ToString(), "File Not Found.");
			}
		}

		/// <summary>
		/// 指定のスキンの画像を取得
		/// </summary>
		/// <param name="skinImage">取得する画像</param>
		/// <param name="image">画像を格納する変数</param>
		public static void GetSkinImage(SkinImage skinImage, out Bitmap image) {
			try {
				image = (Bitmap)Images[(int)skinImage].Clone();
			}
			catch (Exception e) {
				image = null;
				Console.WriteLine("GetSkinImage:" + e.Message);
			}
		}

		/// <summary>
		/// スキンの画像を描画 (拡大・縮小)
		/// </summary>
		/// <param name="graph">描画先</param>
		/// <param name="skinImage">描画する画像</param>
		/// <param name="rect">描画する位置とサイズ</param>
		public static void DrawingSkinImage(ref Graphics graph, SkinImage skinImage, Rectangle rect) {
			try {
				graph.DrawImage(Images[(int)skinImage], rect);
			}
			catch (Exception e) {
				Console.WriteLine("DrawingSkinImage:" + e.Message);
			}
		}

        /// <summary>
        /// スキンの画像を描画 (繰り返し)
        /// </summary>
        /// <param name="graph">描画先</param>
        /// <param name="skinImage">描画する画像</param>
        /// <param name="rect">描画する位置とサイズ</param>
        public static void DrawingSkinImageRepeat(ref Graphics graph, SkinImage skinImage, Rectangle rect)
        {
            using (Image img = (Image)Images[(int)skinImage].Clone())
            {
                Point imgPos = new Point(0, 0);

                while (imgPos.X < rect.Width)
                {
                    imgPos.Y = 0;

                    while (imgPos.Y < rect.Height)
                    {
                        graph.DrawImageUnscaled(img, imgPos);
                        imgPos.Y += img.Height;
                    }
                    imgPos.X += img.Width;
                }
            }

        }

		/// <summary>
		/// ウィンドウタイプのアイテムの背景画像を取得
		/// </summary>
		/// <param name="windowType">ウィンドウタイプ</param>
		/// <param name="push">押した時の画像</param>
		/// <returns></returns>
		public static SkinImage GetItemBackType(FormType windowType, bool push = false) {
			int[] n = new int[] { 4, 8, 12 };
			return (SkinImage)Enum.ToObject(typeof(SkinImage), n[(int)windowType] + (push == true ? 1 : 0));
		}

		/// <summary>
		/// 指定されたウィンドウの種類の背景画像を取得
		/// </summary>
		/// <param name="windowType">ウィンドウの種類</param>
		/// <returns></returns>
		public static SkinImage GetWindowBackground(FormType windowType) {
			return windowType == FormType.Launcher ? SkinImage.Launcher_Background :
					windowType == FormType.MysetList ? SkinImage.Myset_Background :
					SkinImage.Group_Background;
		}

		private static bool LoadPath(SkinImage skinImage, out Bitmap image, ref StringBuilder msg, string skinName) {
			if (skinName == "") {
				LoadAssemblyImage(skinImage, out image);
				return false;
			}
			else {
				string path = Directory.GetCurrentDirectory() + "\\Skin\\" + skinName + "\\" + skinImage.ToString().ToLower() + ".png";
				try {
					if (File.Exists(path)) {
						using (FileStream stream = new FileStream(path, FileMode.Open)) {
							image = new Bitmap(stream);
						}
					}
					else {
						msg.AppendLine("\'" + path + "\'");
						LoadAssemblyImage(skinImage, out image);
					}

					return true;
				}
				catch (Exception e) {
					Console.WriteLine("LoadPath:" + e.Message);
					image = null;
					return false;
				}
			}
		}

		/// <summary>
		/// スキンの読み込み
		/// </summary>
		/// <param name="skinImage">スキン画像の種類</param>
		/// <param name="image">画像の読み込み先</param>
		private static bool LoadPath(SkinImage skinImage, out Bitmap image,  ref StringBuilder msg) {
			return LoadPath(skinImage, out image, ref msg, GHManager.Settings.SkinName);
		}

		private static bool LoadAssemblyImage(SkinImage skinImage, out Bitmap image) {
			try {
				image = ((Bitmap)(Properties.Resources.ResourceManager.GetObject(skinImage.ToString().ToLower(), Properties.Resources.Culture)));
				return true;
			}
			catch (Exception e) {
				Console.WriteLine("LoadPath:" + e.Message);
				image = null;
				return false;
			}
		}

	}


}
