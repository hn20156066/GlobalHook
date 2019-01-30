using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;

namespace GH {

	/// <summary>
	/// 設定情報 スタイルと数値
	/// </summary>
	[Serializable]
	public class GHSettings {

		public GHSettings() {
			Launcher = new Launcher_Settings();
			Animate = new Animate_Settings();
			Style = new Style_Settings();
			Hotkey = new HotKey_Settings();
			Magnet = new Magnet_Settings();
			FitRangeMax = 100;
			FitRangeMin = 0;
			DrawQuality = 1;
			GroupIconStyle = 0;
			MysetIconStyle = 0;
			SkinName = "";
		}

		public GHSettings(GHSettings settings) {
			Launcher = new Launcher_Settings(settings.Launcher);
			Animate = new Animate_Settings(settings.Animate);
			Style = new Style_Settings(settings.Style);
			Hotkey = new HotKey_Settings(settings.Hotkey);
			Magnet = new Magnet_Settings(settings.Magnet);
			FitRangeMax = settings.FitRangeMax;
			FitRangeMin = settings.FitRangeMin;
			GroupIconStyle = settings.GroupIconStyle;
			MysetIconStyle = settings.MysetIconStyle;
			DrawQuality = settings.DrawQuality;
			SkinName = settings.SkinName;
		}

		// ランチャー類
		public class Launcher_Settings {
			public uint Pos { get; set; }
			public int ReactRange { get; set; }
			public bool ShownMouseButton { get; set; }
			public int Offset { get; set; }

			public Launcher_Settings() {
				Pos = 0;
				ReactRange = 1;
				ShownMouseButton = false;
				Offset = -1;
			}

			public Launcher_Settings(Launcher_Settings launcher_Settings) {
				Pos = launcher_Settings.Pos;
				ReactRange = launcher_Settings.ReactRange;
				ShownMouseButton = launcher_Settings.ShownMouseButton;
				SetOffset(launcher_Settings.Offset);
			}

			public void SetOffset(int offset) {
				Offset = offset;
				GHManager.Launcher.SetOffset(Offset);
			}

		}

		/// <summary>
		/// ホットキー
		/// </summary>
		public class HotKey_Settings {
			public SerializableDictionary<int, HotKeyInfo> HotKeys { get; set; }

			public HotKey_Settings() {
				HotKeys = new SerializableDictionary<int, HotKeyInfo>();
				HotKeys.AddRange(new KeyValuePair<int, HotKeyInfo>[] {
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.Show, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.S)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.Hide, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.H)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.ShowConfig, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.O)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.OpenSelectItem, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.Enter)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.DeleteSelectItem, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.D)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectNextItem, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.Right)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectPrevItem, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.Left)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectNextGroup, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.Down)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectPrevGroup, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.Up)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectGroupTile, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.M)),
					new KeyValuePair<int, HotKeyInfo>(GH_SHID.SelectGroupTile2, new HotKeyInfo(Keys.Alt | Keys.Shift, Keys.V))
				});
			}

			public HotKey_Settings(HotKey_Settings settings) {
				HotKeys = new SerializableDictionary<int, HotKeyInfo>(settings.HotKeys);
			}

		}

		// 表示・非表示
		public class Animate_Settings {
			private GHAnimateInfo _Launcher;
			private GHAnimateInfo _MysetList;
			private GHAnimateInfo _ItemList;

			public int Launcher_DelayTime {
				get => _Launcher._DelayTime;
				set {
					GHManager.Launcher.animateInfo.SetDelayTime(value);
					_Launcher.SetDelayTime(GHManager.Launcher.animateInfo._DelayTime);
				}
			}
			public long Launcher_AnimateTime {
				get => _Launcher._AnimateTime;
				set {
					GHManager.Launcher.animateInfo.SetAnimateTime(value);
					_Launcher.SetAnimateTime(GHManager.Launcher.animateInfo._AnimateTime);
				}
			}

			public bool Launcher_Slide {
				get => _Launcher._Slide;
				set {
					GHManager.Launcher.animateInfo.SetSlide(value);
					_Launcher.SetSlide(GHManager.Launcher.animateInfo._Slide);
				}
			}

			public bool Launcher_Fade {
				get => _Launcher._Fade;
				set {
					GHManager.Launcher.animateInfo.SetFade(value);
					_Launcher.SetFade(GHManager.Launcher.animateInfo._Fade);
				}
			}

			public int MysetList_DelayTime {
				get => _MysetList._DelayTime;
				set {
					GHManager.MysetList.animateInfo.SetDelayTime(value);
					_MysetList.SetDelayTime(GHManager.MysetList.animateInfo._DelayTime);
				}
			}
			public long MysetList_AnimateTime {
				get => _MysetList._AnimateTime;
				set {
					GHManager.MysetList.animateInfo.SetAnimateTime(value);
					_MysetList.SetAnimateTime(GHManager.MysetList.animateInfo._AnimateTime);
				}
			}

			public bool MysetList_Slide {
				get => _MysetList._Slide;
				set {
					GHManager.MysetList.animateInfo.SetSlide(value);
					_MysetList.SetSlide(GHManager.MysetList.animateInfo._Slide);
				}
			}

			public bool MysetList_Fade {
				get => _MysetList._Fade;
				set {
					GHManager.MysetList.animateInfo.SetFade(value);
					_MysetList.SetFade(GHManager.MysetList.animateInfo._Fade);
				}
			}

			public int ItemList_DelayTime {
				get => _ItemList._DelayTime;
				set {
					GHManager.ItemList.animateInfo.SetDelayTime(value);
					_ItemList.SetDelayTime(GHManager.ItemList.animateInfo._DelayTime);
				}
			}
			public long ItemList_AnimateTime {
				get => _ItemList._AnimateTime;
				set {
					GHManager.ItemList.animateInfo.SetAnimateTime(value);
					_ItemList.SetAnimateTime(GHManager.ItemList.animateInfo._AnimateTime);
				}
			}

			public bool ItemList_Slide {
				get => _ItemList._Slide;
				set {
					GHManager.ItemList.animateInfo.SetSlide(value);
					_ItemList.SetSlide(GHManager.ItemList.animateInfo._Slide);
				}
			}

			public bool ItemList_Fade {
				get => _ItemList._Fade;
				set {
					GHManager.ItemList.animateInfo.SetFade(value);
					_ItemList.SetFade(GHManager.ItemList.animateInfo._Fade);
				}
			}

			public Animate_Settings() {
				_Launcher = new GHAnimateInfo(400, 200, true, false);
				_MysetList = new GHAnimateInfo(400, 100, true, true);
				_ItemList = new GHAnimateInfo(400, 100, true, true);
			}

			public Animate_Settings(Animate_Settings animation_Settings) {
				_Launcher = animation_Settings._Launcher;
				_MysetList = animation_Settings._MysetList;
				_ItemList = animation_Settings._ItemList;
			}

		}

		// デザイン類
		public class Style_Settings {

			public GHBaseStyle Launcher;
			public GHBaseStyle MysetList;
			public ItemListStyle ItemList;

			public Style_Settings() {
				Launcher = new GHBaseStyle();
				MysetList = new GHBaseStyle();
				ItemList = new ItemListStyle();
			}

			public Style_Settings(Style_Settings style_Settings) {
				Launcher = new GHBaseStyle(style_Settings.Launcher);
				MysetList = new GHBaseStyle(style_Settings.MysetList);
				ItemList = new ItemListStyle(style_Settings.ItemList);
			}

		}

		// マグネット機能類
		public class Magnet_Settings {
			private int _FitRange { get; set; }
			private bool _FitDisplay { get; set; }
			private bool _FitTaskbar { get; set; }
			private bool _FitWindows { get; set; }
			private uint _GroupKey { get; set; }
			private uint _MoveKey { get; set; }

			//public Dll.Window[] NoFitWindows { get; set; }

			public int FitRange {
				get {
					return _FitRange;
				}
				set {
					Dll.SetFitRange(value);
					_FitRange = Dll.GetFitRange();
				}
			}

			public bool FitDisplay {
				get {
					return _FitDisplay;
				}
				set {
					Dll.SetFitDisplay(value);
					_FitDisplay = Dll.GetFitDisplay();
				}
			}

			public bool FitTaskbar {
				get {
					return _FitTaskbar;
				}
				set {
					Dll.SetFitTaskbar(value);
					_FitTaskbar = Dll.GetFitTaskbar();
				}
			}

			public bool FitWindows {
				get {
					return _FitWindows;
				}
				set {
					Dll.SetFitWindows(value);
					_FitWindows = Dll.GetFitWindows();
				}
			}

			public uint GroupKey {
				get {
					return _GroupKey;
				}
				set {
					Dll.SetGroupKey(value);
					_GroupKey = Dll.GetGroupKey();
				}
			}

			public uint MoveKey {
				get {
					return _MoveKey;
				}
				set {
					Dll.SetMoveKey(value);
					_MoveKey = Dll.GetMoveKey();
				}
			}

			public Magnet_Settings() {
				FitRange = 20;
				FitDisplay = true;
				FitTaskbar = true;
				FitWindows = true;
				GroupKey = (uint)Keys.ShiftKey;
				MoveKey = (uint)Keys.ControlKey;
				//NoFitWindows = new Dll.Window[255];
			}

			public Magnet_Settings(Magnet_Settings magnet_Settings) {
				FitRange = magnet_Settings.FitRange;
				FitDisplay = magnet_Settings.FitDisplay;
				FitTaskbar = magnet_Settings.FitTaskbar;
				FitWindows = magnet_Settings.FitWindows;
				GroupKey = magnet_Settings.GroupKey;
				MoveKey = magnet_Settings.MoveKey;
				//NoFitWindows = magnet_Settings.NoFitWindowsS
			}

		}

		public Launcher_Settings Launcher { get; set; }
		public Animate_Settings Animate { get; set; }
		public HotKey_Settings Hotkey { get; set; }
		public Style_Settings Style { get; set; }
		public Magnet_Settings Magnet { get; set; }
		public int FitRangeMax { get; set; }
		public int FitRangeMin { get; set; }
		public int GroupIconStyle { get; set; }
		public int MysetIconStyle { get; set; }
		public int DrawQuality { get; set; }
		public string SkinName { get; set; }

	}

}
