using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace GH {

	/// <summary>
	/// マイセットを管理するクラス
	/// </summary>
	public static class MysetManager {

		private static string MysetFileName { get; } = "Myset.xml";

		//private static MysetXml Mysets { get; set; }

		// マイセット
		public static List<Myset> MysetList;

		/// <summary>
		/// 初期化 (初期量 = 10)
		/// </summary>
		public static void Initialize() {
			MysetList = new List<Myset>(10);
			//Mysets = new MysetXml();
		}

		/// <summary>
		/// マイセットを追加
		/// </summary>
		/// <param name="group">追加するグループ</param>
		public static void AddMyset(Group group) {
			MysetList.Add(new Myset(group));
			GHManager.MysetList.Controls.Add(MysetList.Last().icon.control);
		}

		public static void AddMyset(string[] path) {
			MysetList.Add(new Myset(path));
			GHManager.MysetList.Controls.Add(MysetList.Last().icon.control);
		}

		/// <summary>
		/// アイテムの位置を設定
		/// </summary>
		/// <param name="rect">基準位置</param>
		/// <param name="ver">向き true = 縦 / false = 横</param>
		public static void SetPosition(ref Rectangle rect, bool ver) {
			foreach (var item in MysetList) {
				item.icon.SetRect(ref rect);

				if (ver)
					rect.Y += GHManager.Settings.Style.MysetList.ItemSize + GHManager.Settings.Style.MysetList.ItemSpace;
				else
					rect.X += GHManager.Settings.Style.MysetList.ItemSize + GHManager.Settings.Style.MysetList.ItemSpace;
			}
		}

		/// <summary>
		/// マイセットを描画
		/// </summary>
		/// <param name="graph">描画先</param>
		public static void Draw(ref Graphics graph) {
			foreach (var item in MysetList) {
				item.Draw(ref graph);
			}
		}

		/// <summary>
		/// アイテムリストにマイセットの番号を設定
		/// </summary>
		/// <param name="myset">表示するマイセット</param>
		/// <returns></returns>
		public static bool SetMysetNum(Myset myset) {
			int n = MysetList.IndexOf(myset);

			if (GHManager.ItemList.Item_Num == n && GHManager.ItemList.ParentGHForm == 1) {
				GHManager.ItemList.SetMyset(n);
				return false;
			}
			else {
				GHManager.ItemList.SetMyset(n);
				return true;
			}
		}

		public static bool CheckOutRange(int idx) {
			if (0 <= idx && idx < MysetList.Count) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// マイセットを削除
		/// </summary>
		/// <param name="myset">削除するマイセット</param>
		public static void DeleteMyset(Myset myset) {
			myset.icon.control.Dispose();
			MysetList.Remove(myset);
		}

		/// <summary>
		/// マイセットのアイテムを削除（最初に見つかったアイテムのみ）
		/// </summary>
		/// <param name="item">削除するマイセットアイテム</param>
		public static void DeleteItem(MysetItem item) {
			for (int i = 0; i < MysetList.Count; ++i) {
				if (MysetList[i].MysetItems.IndexOf(item) != -1) {
					MysetList[i].DeleteItem(item);
					return;
				}
			}
		}

		public static bool AtIndex(int idx) {
			return 0 <= idx && idx < MysetList.Count;
		}

		public static void SaveMyset() {
			XmlDocument document = new XmlDocument();
			XmlWriterSettings writerSettings = new XmlWriterSettings {
				Indent = true,
				IndentChars = "\t",
				NewLineOnAttributes = true
			};

			using (XmlWriter writer = XmlWriter.Create("Myset.xml", writerSettings)) {
				writer.WriteStartDocument();
				writer.WriteStartElement("MysetList");

				for (int i = 0; i < MysetList.Count; ++i) {
					writer.WriteStartElement("Myset");
					
					for (int j = 0; j < MysetList[i].MysetItems.Count; ++j) {
						writer.WriteElementString("Item", MysetList[i].MysetItems[j].ItemPath.ToString());
					}
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}

		public static void LoadMyset() {
			XmlDocument document = new XmlDocument();
			XmlReaderSettings readerSettings = new XmlReaderSettings {
				IgnoreComments = true,
				IgnoreWhitespace = true
			};
			List<string> path;

			try {
				using (XmlReader reader = XmlReader.Create("Myset.xml", readerSettings)) {
					while (reader.Read() == true) {
						if (reader.NodeType == XmlNodeType.EndElement) continue;

						if (reader.Name == "Myset") {
							path = new List<string>();
							reader.Read();
							while (reader.NodeType != XmlNodeType.EndElement) {
								if (reader.Name == "Item") {
									path.Add(reader.ReadElementContentAsString());
								}
							}

							if (path.Count > 0) {
								AddMyset(path.ToArray());
							}
						}
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("LoadMyset: " + e.Message);
			}
		}

	}
}
