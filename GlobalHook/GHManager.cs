using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;

namespace GH {

	/// <summary>
	/// アプリ情報の管理
	/// </summary>
	public static class GHManager {

		/// <summay>
		/// 設定ファイル名
		/// </summary>
		private static readonly string fileName = "GlobalHook.xml";

		/// <summary>
		/// 確定している設定情報
		/// </summary>
		public static GHSettings Settings { get; set; } = new GHSettings();

		/// <summary>
		/// 確定前の設定情報
		/// </summary>
		public static GHSettings TempSettings { get; set; } = new GHSettings();

		/// <summary>
		/// 保存された設定
		/// </summary>
		private static Dictionary<byte, string> SavedSettings = new Dictionary<byte, string>();

		/// <summary>
		/// ランチャー
		/// </summary>
		public static LauncherForm Launcher { get; private set; }

		/// <summary>
		///	マイセットリスト
		/// </summary>
		public static MysetListForm MysetList { get; private set; }

		/// <summary>
		/// アイテムリスト
		/// </summary>
		public static ItemListForm ItemList { get; private set; }

		/// <summary>
		/// ランチャーが水平か
		/// </summary>
		public static bool IsVertical => Settings.Launcher.Pos % 2 == 0;

		/// <summary>
		/// 画面サイズ
		/// </summary>
		public static Rectangle ScreenSize { get; private set; } = new Rectangle(0, 0, 0, 0);

		/// <summary>
		/// 画面作業サイズ
		/// </summary>
		public static Rectangle WorkingArea { get; private set; } = new Rectangle(0, 0, 0, 0);

		/// <summary>
		/// スケール
		/// </summary>
		public static WinAPI.DSIZE Scale => Dll.GetScale2((long)Launcher.Handle);

		/// <summary>
		/// カーソル位置
		/// </summary>
		public static Point CursorPosition => Cursor.Position;

		public static class Contains {
			public static bool Launcher => GHManager.Launcher.Bounds.Contains(Cursor.Position);
			public static bool ItemList => GHManager.ItemList.Bounds.Contains(Cursor.Position);
			public static bool MysetList => GHManager.MysetList.Bounds.Contains(Cursor.Position);
			public static bool AnyContain() {
				Point pt = Cursor.Position;
				bool ret = false;
				ret |= GHManager.Launcher.Bounds.Contains(pt);
				ret |= GHManager.ItemList.Bounds.Contains(pt);
				ret |= GHManager.MysetList.Bounds.Contains(pt);
				return ret;
			}
		}


		public static FormType GetActiveForm() {
			if (!ItemList.FormVisible && MysetList.FormVisible) {
				return FormType.MysetList;
			}
			else if (ItemList.FormVisible) {
				return FormType.ItemList;
			}
			else if (Launcher.FormVisible) {
				return FormType.Launcher;
			}
			else {
				return FormType.None;
			}
		}
		
		/// <summary>
		/// 初期化
		/// </summary>
		public static int Initialize() {
			try {
				UpdateScrSize();

				GroupManager.Initialize();
				MysetManager.Initialize();

				Launcher = new LauncherForm();
				ItemList = new ItemListForm();
				MysetList = new MysetListForm();

				ItemList.Hide();
				MysetList.Hide();

				IntPtr h = Launcher.Handle;
				h = ItemList.Handle;
				h = MysetList.Handle;

				ShortcutProc.Initialize();

				LoadSetting();

				Skin.LoadSkinImages();

				MysetManager.LoadMyset();

				return 0;
			}
			catch {

				Console.WriteLine("Error: Initializing.");

				return -1;
			}
		}

		public static void UpdateScrSize() {
			ScreenSize = Screen.PrimaryScreen.Bounds;
			WorkingArea = Screen.PrimaryScreen.WorkingArea;
		}

		/// <summary>
		/// フォントの取得
		/// </summary>
		/// <returns>フォント</returns>
		public static Font GetFont() {
			return new Font(Settings.Style.ItemList.FontName, Settings.Style.ItemList.FontSize, FontStyle.Regular);
		}

		/// <summary>
		/// 色の取得
		/// </summary>
		/// <returns>色</returns>
		public static Color GetColor() {
			return Color.FromArgb(Settings.Style.ItemList.FontColor.Red, Settings.Style.ItemList.FontColor.Green, Settings.Style.ItemList.FontColor.Blue);
		}

		/// <summary>
		/// ウィンドウのスタイルを取得
		/// </summary>
		/// <param name="windowType">ウィンドウの種類</param>
		/// <returns></returns>
		public static GHBaseStyle GetStyle(FormType windowType)
		{
			return (windowType == FormType.Launcher) ? Settings.Style.Launcher :
					(windowType == FormType.MysetList) ? Settings.Style.MysetList :
					Settings.Style.ItemList;
		}

		/// <summary>
		/// 設定を保存する
		/// </summary>
		public static void SaveSetting() {
			XmlSerializer serializer = new XmlSerializer(typeof(GHSettings));
			try {
				using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/" + fileName, FileMode.Create)) {
					serializer.Serialize(fs, Settings);
				}
			}
			catch (Exception ex) {
				Console.WriteLine("GHManager.Save:" + ex.Message);
			}
		}

		/// <summary>
		/// 設定を読み込む
		/// </summary>
		public static void LoadSetting() {
			XmlSerializer serializer = new XmlSerializer(typeof(GHSettings));
			try {
				using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/" + fileName, FileMode.Open)) {
					Settings = (GHSettings)serializer.Deserialize(fs);
				}
			}
			catch (Exception ex) {
				Console.WriteLine("GHManager.Load:" + ex.Message);
			}
		}

		/// <summary>
		/// 設定を一時的に保管
		/// </summary>
		public static void SavePoint(byte id) {
			if (SavedSettings.ContainsKey(id)) {
				return;
			}
			XmlSerializer serializer = new XmlSerializer(typeof(GHSettings));
			using (StringWriter sw = new StringWriter()) {
				serializer.Serialize(sw, TempSettings);
				SavedSettings.Add(id, sw.ToString());
			}
		}

		/// <summary>
		/// 保管した設定に戻す
		/// </summary>
		public static void Rollback(byte id) {
			if (!SavedSettings.ContainsKey(id)) {
				return;
			}
			XmlSerializer serializer = new XmlSerializer(typeof(GHSettings));
			using (StringReader sr = new StringReader(SavedSettings[id])) {
				TempSettings = (GHSettings)serializer.Deserialize(sr);
			}
			Launcher.SetOffset(TempSettings.Launcher.Offset);
			Commit();
		}

		public static void Commit(bool uncheck = false) {
			if (SavedSettings.Count == 0 && !uncheck) return;
			XmlSerializer serializer = new XmlSerializer(typeof(GHSettings));
			using (StringWriter sw = new StringWriter()) {
				serializer.Serialize(sw, TempSettings);
				using (StringReader sr = new StringReader(sw.ToString())) {
					Settings = (GHSettings)serializer.Deserialize(sr);
				}
			}
			Launcher.SetOffset(Settings.Launcher.Offset);
		}

		public static void SaveClear() {
			SavedSettings.Clear();
			SavedSettings = null;
			SavedSettings = new Dictionary<byte, string>();
		}

		private static int RegisterHotKey2(IntPtr handle, int id, HotKeyInfo hotkey) {
			return WinAPI.RegisterHotKey(handle, id, hotkey.WinAPI_ModKey, hotkey.WinAPI_Key);
		}

		/// <summary>
		/// ホットキーの登録
		/// </summary>
		/// <param name="handle"></param>
		public static void RegistHotKey(IntPtr handle) {
			Settings.Hotkey.HotKeys.ToList().ForEach(k => RegisterHotKey2(handle, k.Key, k.Value));
		}

		/// <summary>
		/// ホットキーの解除
		/// </summary>
		/// <param name="handle">ウィンドウハンドル</param>
		/// <returns></returns>
		public static void UnregistHotKey(IntPtr handle) {
			Settings.Hotkey.HotKeys.ToList().ForEach(k => WinAPI.UnregisterHotKey(handle, k.Key));
		}

		/// <summary>
		/// ホットキーが登録可能か
		/// </summary>
		/// <param name="mod">修飾キー</param>
		/// <param name="key">キー</param>
		/// <returns>登録可能ならtrue</returns>
		public static bool CheckRegistHotKey(uint mod, uint key) {
			HotKeyInfo hotKey = new HotKeyInfo(mod, key);
			const int id = 9999;
			if (RegisterHotKey2(Launcher.Handle, id, hotKey) == 0) {
				return false;
			}
			else {
				WinAPI.UnregisterHotKey(Launcher.Handle, id);
				return true;
			}
		}

		public static void MoveElement<T>(ref List<T> list, int currentIndex, bool next) {
			int newIndex = currentIndex + (next ? 2 : -2);
			if (newIndex < 0 || list.Count >= newIndex) return;

			T temp = list[currentIndex];
			list.RemoveAt(currentIndex);
			list.Insert(newIndex, temp);
		}
	}

	/// <summary>
	/// 色
	/// </summary>
	public class GHColor {
		public byte Red;
		public byte Green;
		public byte Blue;

		public GHColor() {
			Red = 0;
			Green = 0;
			Blue = 0;
		}

		public GHColor(byte red, byte green, byte blue) {
			Red = red;
			Green = green;
			Blue = blue;
		}

		public GHColor(GHColor color) {
			Red = color.Red;
			Green = color.Green;
			Blue = color.Blue;
		}

		public void SetColor(byte red, byte green, byte blue) {
			Red = red;
			Green = green;
			Blue = blue;
		}

	}

	/// <summary>
	/// 余白
	/// </summary>
	public class GHPadding {

		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public int WSize => Left + Right;
		public int HSize => Top + Bottom;

		public GHPadding(int left, int top, int right, int bottom) {
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public GHPadding(GHPadding padding) {
			Left = padding.Left;
			Top = padding.Top;
			Right = padding.Right;
			Bottom = padding.Bottom;
		}

		public GHPadding() {
			Left = 0;
			Top = 0;
			Right = 0;
			Bottom = 0;
		}

	}

	/// <summary>
	/// アニメーションの情報
	/// </summary>
	public class GHAnimateInfo {

		[NonSerialized]
		public byte _Alpha;

		public int _DelayTime { get; private set; }
		public long _AnimateTime { get; private set; }
		public bool _Slide { get; private set; }
		public bool _Fade { get; private set; }

		public GHAnimateInfo() {
			_DelayTime = 400;
			_AnimateTime = 200;
			_Slide = true;
			_Fade = true;
			_Alpha = _Fade ? (byte)0 : (byte)255;
		}

		public GHAnimateInfo(int delayTime, long animateTime, bool slide, bool fade) {
			_DelayTime = delayTime;
			_AnimateTime = animateTime;
			_Slide = slide;
			_Fade = fade;
			_Alpha = _Fade ? (byte)0 : (byte)255;
		}

		public GHAnimateInfo(GHAnimateInfo animateInfo) {
			_DelayTime = animateInfo._DelayTime;
			_AnimateTime = animateInfo._AnimateTime;
			_Slide = animateInfo._Slide;
			_Fade = animateInfo._Fade;
			_Alpha = _Fade ? (byte)0 : (byte)255;
		}

		public void SetDelayTime(int delaytime) => _DelayTime = delaytime;
		public void SetAnimateTime(long animatetime) => _AnimateTime = animatetime;
		public void SetSlide(bool slide) => _Slide = slide;
		public void SetFade(bool fade) {
			_Fade = fade;
			_Alpha = fade ? (byte)0 : (byte)255;
		}

	}

	/// <summary>
	/// 共通するスタイル
	/// </summary>
	public class GHBaseStyle {

		/// <summary>
		/// アイテムの縦幅
		/// </summary>
		public int ItemSizeHeight;

		/// <summary>
		/// アイテムの横幅
		/// </summary>
		public int ItemSizeWidth;

		/// <summary>
		/// アイテム同士の間隔
		/// </summary>
		public int ItemSpace;

		/// <summary>
		/// ウィンドウの内側の余白
		/// </summary>
		public GHPadding WindowPadding;

		/// <summary>
		/// アイテムの内側の余白
		/// </summary>
		public GHPadding ItemPadding;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public GHBaseStyle() {
			ItemSizeHeight = 60;
			ItemSizeWidth = 60;
			ItemSpace = 5;
			WindowPadding = new GHPadding(5, 5, 5, 5);
			ItemPadding = new GHPadding(5, 5, 5, 5);
		}

		public GHBaseStyle(GHBaseStyle baseStyle) {
			ItemSizeHeight = baseStyle.ItemSizeHeight;
			ItemSizeWidth = baseStyle.ItemSizeWidth;
			ItemSpace = baseStyle.ItemSpace;
			WindowPadding = new GHPadding(baseStyle.WindowPadding);
			ItemPadding = new GHPadding(baseStyle.ItemPadding);
		}

		/// <summary>
		/// 横幅
		/// </summary>
		public int Width => ItemSizeWidth + WindowPadding.WSize;

		/// <summary>
		/// 縦幅
		/// </summary>
		public int Height => ItemSizeHeight + WindowPadding.HSize;

		/// <summary>
		/// 縦幅基準
		/// </summary>
		public int ItemSize {
			get => ItemSizeHeight;
			set => ItemSizeHeight = ItemSizeWidth = value;
		}

	}

	/// <summary>
	/// アイテムリストのスタイル
	/// </summary>
	public class ItemListStyle : GHBaseStyle {

		public int Column;
		public int IconSize;
		public int UseIconSize;
		public GHPadding TextPadding;
		public string FontName;
		public float FontSize;
		public GHColor FontColor;

		public int GetUseIconSize() {
			if (UseIconSize == (int)WinAPI.SHIL.SHIL_LARGE) return 32;
			if (UseIconSize == (int)WinAPI.SHIL.SHIL_SMALL) return 16;
			if (UseIconSize == (int)WinAPI.SHIL.SHIL_EXTRALARGE) return 48;
			if (UseIconSize == (int)WinAPI.SHIL.SHIL_JUMBO) return 256;
			return UseIconSize = (int)WinAPI.SHIL.SHIL_LARGE;
		}

		public void SetUseIconSize(int size) {
			if (size == 32) UseIconSize = (int)WinAPI.SHIL.SHIL_LARGE;
			else if (size == 16) UseIconSize = (int)WinAPI.SHIL.SHIL_SMALL;
			else if (size == 48) UseIconSize = (int)WinAPI.SHIL.SHIL_EXTRALARGE;
			else if (size == 256) UseIconSize = (int)WinAPI.SHIL.SHIL_JUMBO;
			else UseIconSize = (int)WinAPI.SHIL.SHIL_LARGE;
		}

		public ItemListStyle() {
			Column = 3;
			UseIconSize = (int)WinAPI.SHIL.SHIL_LARGE;
			IconSize = 30;
			TextPadding = new GHPadding(0, 2, 0, 2);
			FontName = "メイリオ";
			FontSize = 8;
			FontColor = new GHColor(255, 255, 255);
		}

		public ItemListStyle(ItemListStyle itemListStyle) {
			Column = itemListStyle.Column;
			UseIconSize = itemListStyle.UseIconSize;
			IconSize = itemListStyle.IconSize;
			TextPadding = new GHPadding(itemListStyle.TextPadding);
			FontName = itemListStyle.FontName;
			FontSize = itemListStyle.FontSize;
			FontColor = new GHColor(itemListStyle.FontColor);
		}

	}

	/// <summary>
	/// ホットキーの情報
	/// </summary>
	 

	public class HotKeyInfo {
		public uint ModKey { get; set; }
		public uint Key { get; set; }

		public uint WinAPI_ModKey {
			get {
				uint mod = 0;
				if ((ModKey & (uint)Keys.Control) == (uint)Keys.Control) {
					mod |= (uint)WinAPI.MOD_KEY.CTRL;
				}
				if ((ModKey & (uint)Keys.Alt) == (uint)Keys.Alt) {
					mod |= (uint)WinAPI.MOD_KEY.ALT;
				}
				if ((ModKey & (uint)Keys.Shift) == (uint)Keys.Shift) {
					mod |= (uint)WinAPI.MOD_KEY.SHIFT;
				}
				if ((ModKey & (uint)Keys.LWin) == (uint)Keys.LWin) {
					mod |= (uint)WinAPI.MOD_KEY.WIN;
				}

				return mod;
			}
		}

		public uint WinAPI_Key => Key;

		public HotKeyInfo() {
			ModKey = 0;
			Key = 0;
		}

		public HotKeyInfo(uint mod, uint key) {
			ModKey = mod;
			Key = key;
		}

		public HotKeyInfo(Keys mod, Keys key) {
			ModKey = (uint)mod;
			Key = (uint)key;
		}

		public HotKeyInfo(HotKeyInfo hotKey) {
			ModKey = hotKey.ModKey;
			Key = hotKey.Key;
		}

		public void SetKeys(uint mod, uint key) {
			ModKey = mod;
			Key = key;
		}

		public override bool Equals(object obj) {
			if (obj == null || GetType() != obj.GetType()) {
				return false;
			}
			HotKeyInfo c = (HotKeyInfo)obj;
			return (ModKey == c.ModKey) && (Key == c.Key);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

	}

	public class SerializableList<T> : List<T>, IXmlSerializable {
		public XmlSchema GetSchema() {
			return null;
		}

		public SerializableList() {
		}

		public SerializableList(SerializableList<T> src) {
			AddRange(src);
		}

		public void ReadXml(XmlReader reader) {
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();
			if (wasEmpty)
				return;

			//XmlSerializerを用意する
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			while (reader.NodeType != XmlNodeType.EndElement) {
				reader.ReadStartElement("List");

				//キーを逆シリアル化する
				reader.ReadStartElement("Item");
				T key = (T)serializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadEndElement();

				//コレクションに追加する
				Add(key);

				//次へ
				reader.MoveToContent();
			}

			reader.ReadEndElement();
		}

		//現在の内容をXMLに書き込む
		public void WriteXml(XmlWriter writer) {
			//XmlSerializerを用意する
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			foreach (T item in this) {
				writer.WriteStartElement("List");

				//キーを書き込む
				writer.WriteStartElement("Item");
				serializer.Serialize(writer, item);
				writer.WriteEndElement();
				
				writer.WriteEndElement();
			}
		}

	}

	/// <summary>
	/// XMLシリアル化ができるDictionaryクラス
	/// </summary>
	/// <typeparam name="TKey">キーの型</typeparam>
	/// <typeparam name="TValue">値の型</typeparam>
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable {
		//nullを返す
		public XmlSchema GetSchema() {
			return null;
		}

		public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> addPairs) {
			foreach (var kv in addPairs) {
				Add(kv.Key, kv.Value);
			}
		}

		public SerializableDictionary() {
		}

		public SerializableDictionary(SerializableDictionary<TKey, TValue> src) {
			foreach (var kv in src) {
				Add(kv.Key, kv.Value);
			}
		}

		//XMLを読み込んで復元する
		public void ReadXml(XmlReader reader) {
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();
			if (wasEmpty)
				return;

			//XmlSerializerを用意する
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			while (reader.NodeType != XmlNodeType.EndElement) {
				reader.ReadStartElement("KeyValuePair");

				//キーを逆シリアル化する
				reader.ReadStartElement("Key");
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				//値を逆シリアル化する
				reader.ReadStartElement("Value");
				TValue val = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadEndElement();

				//コレクションに追加する
				this.Add(key, val);

				//次へ
				reader.MoveToContent();
			}

			reader.ReadEndElement();
		}

		//現在の内容をXMLに書き込む
		public void WriteXml(XmlWriter writer) {
			//XmlSerializerを用意する
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach (TKey key in this.Keys) {
				writer.WriteStartElement("KeyValuePair");

				//キーを書き込む
				writer.WriteStartElement("Key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				//値を書き込む
				writer.WriteStartElement("Value");
				valueSerializer.Serialize(writer, this[key]);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
	}

	public static class GH_SHID {
		public const int Show = 0x0001;
		public const int Hide = 0x0002;
		public const int ShowConfig = 0x0004;
		public const int OpenSelectItem = 0x0008;
		public const int DeleteSelectItem = 0x000F;
		public const int SelectNextItem = 0x0010;
		public const int SelectPrevItem = 0x0020;
		public const int SelectNextGroup = 0x0040;
		public const int SelectPrevGroup = 0x0080;
		public const int SelectGroupTile = 0x00A0;
		public const int SelectGroupTile2 = 0x00A2;
	}

}
