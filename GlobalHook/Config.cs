using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace GH {
	public partial class Config : Form {

		public Config() {
			InitializeComponent();
		}

		// 閉じた時のページを保存
		private static int Page = 0;

		private bool ResultOK = false;

		private uint prevLauncherPos = uint.MaxValue;

		private string[] ModKey = new string[4] { "Alt", "Shift", "Control", "Win" };

		private List<uint> PushKeys = new List<uint>();

		private HotKeyInfo HotKey;
		private bool LeavedKey = false;
		private bool IsEntered = false;

		private ListViewHitTestInfo ListViewHitInfo;
		private ListViewInputBox ListViewInput;

		private Dictionary<int, string> PrevKeyStr = new Dictionary<int, string>();

		/// <summary>
		/// ウィンドウプロシージャ
		/// </summary>
		/// <param name="m">Windowsメッセージ</param>
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);

			switch (m.Msg) {
				case WinAPI.WM_COPYDATA:
					if (!ListViewInput.Visible || HotKey == null) break;

					WinAPI.COPYDATASTRUCT cds = (WinAPI.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(WinAPI.COPYDATASTRUCT));

					WinAPI.KeyInfo key = (WinAPI.KeyInfo)Marshal.PtrToStructure(cds.lpData, typeof(WinAPI.KeyInfo));
					uint mod = HotKey.ModKey;

					if (key.keycode == (uint)Keys.RWin) key.keycode = (uint)Keys.LWin;
					if (key.keycode == (uint)Keys.LShiftKey || key.keycode == (uint)Keys.RShiftKey) key.keycode = (uint)Keys.Shift;
					if (key.keycode == (uint)Keys.LControlKey || key.keycode == (uint)Keys.RControlKey) key.keycode = (uint)Keys.Control;
					if (key.keycode == (uint)Keys.LMenu || key.keycode == (uint)Keys.RMenu) key.keycode = (uint)Keys.Alt;
					if (key.keycode == (uint)Keys.Return) key.keycode = (uint)Keys.Enter;


					if (LeavedKey) {
						if (key.keycode == (uint)Keys.Enter || key.keycode == (uint)Keys.Escape) {
							Keys sendKey = (Keys)key.keycode;
							IsEntered = true;
							ListViewInput.Textbox_SetValue(sendKey, HotKey);
							break;
						}
					}

					if (key.keycode == (uint)Keys.Control || key.keycode == (uint)Keys.Alt || key.keycode == (uint)Keys.Shift || key.keycode == (uint)Keys.LWin) {
						// キーをすべて離したら次のキー入力で取得しなおす
						if (key.keyflag == 1) {
							if (LeavedKey) {
								mod = 0;
								LeavedKey = false;
							}
							mod |= (uint)(Keys)Enum.ToObject(typeof(Keys), key.keycode);
						}
						else if (key.keyflag == 3) {
							LeavedKey = true;
						}

						HotKey.ModKey = mod;
					}
					else {
						if (HotKey.ModKey == 0) HotKey.ModKey = (uint)Keys.Control;						
						HotKey.Key = (uint)(Keys)Enum.ToObject(typeof(Keys), key.keycode);
					}

					if (HotKey.ModKey != 0 && HotKey.Key != 0) {
						ListViewInput.Text = KeysToString(HotKey.ModKey, HotKey.Key);
					}

					break;

				default:
					break;
			}

		}


		public static class KeysMap {
			public static Dictionary<uint, string> map = new Dictionary<uint, string>();
			public static void Init() {
				if (map.Count <= 0) {
					map.Add((uint)Keys.IMEConvert, "変換");
					map.Add((uint)Keys.IMENonconvert, "無変換");
					map.Add((uint)Keys.Back, "BackSpace");
					map.Add((uint)Keys.OemMinus, "-");
					map.Add((uint)Keys.Oem7, "^");
					map.Add((uint)Keys.Oem5, "\\");
					map.Add((uint)Keys.Oemtilde, "@");
					map.Add((uint)Keys.OemOpenBrackets, "[");
					map.Add((uint)Keys.Oemplus, ";");
					map.Add((uint)Keys.Oem1, ":");
					map.Add((uint)Keys.Oem6, "]");
					map.Add((uint)Keys.Oemcomma, ",");
					map.Add((uint)Keys.OemPeriod, ".");
					map.Add((uint)Keys.OemQuestion, "/");
					map.Add((uint)Keys.OemBackslash, "_");
					map.Add((uint)Keys.Up, "↑");
					map.Add((uint)Keys.Left, "←");
					map.Add((uint)Keys.Right, "→");
					map.Add((uint)Keys.Down, "↓");
					map.Add((uint)Keys.Return, "Enter");
				}
			}
		}

		// キーコードから文字列を取得
		private string KeysToString(uint mod, uint key) {
			string result = "";
			string keyStr = "";

			if((mod & (uint)Keys.Control) == (uint)Keys.Control) {
				result += "Ctrl + ";
			}
			if ((mod & (uint)Keys.Alt) == (uint)Keys.Alt) {
				result += "Alt + ";
			}
			if ((mod & (uint)Keys.Shift) == (uint)Keys.Shift) {
				result += "Shift + ";
			}
			if((mod & (uint)Keys.LWin) == (uint)Keys.LWin) {
				result += "LWin + ";
			}

			if ((uint)Keys.D0 <= key && key <= (uint)Keys.D9) {
				keyStr = char.ToString((char)(key - (uint)Keys.D0 + '0'));
			}
			else if ((uint)Keys.A <= key && key <= (uint)Keys.Z) {
				keyStr = char.ToString((char)(key - (uint)Keys.A + 'A'));
			}
			else if (key == 240) {
				keyStr = "CapsLock";
			}
			else if (key == 242) {
				keyStr = "カタカナ/ひらがな";
			}
			else if(key == 243) {
				keyStr = "半角/全角";
			}
			else if(key == 244) {
				keyStr = "半角/全角";
			}
			else if (KeysMap.map.ContainsKey(key)) {
				keyStr = KeysMap.map[key];
			}
			else {
				keyStr = ((Keys)Enum.ToObject(typeof(Keys), key)).ToString();
			}

			if (keyStr == "" || keyStr == "ControlKey" || keyStr == "ShiftKey" || keyStr == "Menu") result = "";
			else result += keyStr;

			return result;
		}

		// スキンの一覧と選択中のスキンを選択
		private void Load_Skin() {
			string skinPath = Directory.GetCurrentDirectory() + "\\Skin";
			ListBoxSkin.Items.Add("[内部スキン]");
			if (Directory.Exists(skinPath)) {
				List<string> dirs = Directory.EnumerateDirectories(skinPath, "*", SearchOption.TopDirectoryOnly).ToList();
				dirs.ForEach(s => ListBoxSkin.Items.Add(Path.GetFileName(s)));
				for (int i = 1; i < ListBoxSkin.Items.Count; ++i) {
					if (GHManager.Settings.SkinName.Equals(ListBoxSkin.Items[i].ToString())) {
						ListBoxSkin.SelectedIndex = i;
						return;
					}
				}
			}
			ListBoxSkin.SelectedIndex = 0;
		}

		/// <summary>
		/// アップダウンコントロールの下限と上限のチェック
		/// 超えていた場合、下限または上限に丸める
		/// </summary>
		/// <param name="sender">コントロール</param>
		/// <returns>設定した値</returns>
		public static int UpDownCheckOfLimit(ref object sender) {

			if (sender.GetType() != typeof(NumericUpDown))
				return -1;

			int value = (int)((NumericUpDown)sender).Value;
			int max = (int)((NumericUpDown)sender).Maximum;
			int min = (int)((NumericUpDown)sender).Minimum;

			if (value < min) {
				value = min;
			}
			if (value > max) {
				value = max;
			}

			((NumericUpDown)sender).Value = value;

			return value;
		}
		
		private void Config_Load(object sender, EventArgs e) {

			StringBuilder sb = new StringBuilder(255);
			WinAPI.GetClassName(Handle, sb, 255);
			Dll.SetConfigClassName(sb.ToString().ToCharArray());
			Dll.SetConfigText(Text.ToCharArray());

			ConfigTabControl.SelectedIndex = Page;

			BringToFront();
			ComboBoxLauncherPos.Select();
			Activate();

			KeysMap.Init();
			
			// 現在の設定を退避
			GHManager.SavePoint(1);

			// コントロールに現在の設定を設定
			// ランチャーページ
			ComboBoxLauncherPos.SelectedIndex = (int)GHManager.Settings.Launcher.Pos;
			UpDownLauncherReactRange.Value = GHManager.Settings.Launcher.ReactRange;
			CheckLauncherMouseButton.Checked = GHManager.Settings.Launcher.ShownMouseButton;
			UpDownLauncherDelayTime.Value = GHManager.Settings.Animate.Launcher_DelayTime;
			UpDownLauncherAnimateTime.Value = GHManager.Settings.Animate.Launcher_AnimateTime;
			CheckLauncherSlide.Checked = GHManager.Settings.Animate.Launcher_Slide;
			CheckLauncherFade.Checked = GHManager.Settings.Animate.Launcher_Fade;
			UpDownMysetlistDelayTime.Value = GHManager.Settings.Animate.MysetList_DelayTime;
			UpDownMysetlistAnimateTime.Value = GHManager.Settings.Animate.MysetList_AnimateTime;
			CheckMysetlistSlide.Checked = GHManager.Settings.Animate.MysetList_Slide;
			CheckMysetlistFade.Checked = GHManager.Settings.Animate.MysetList_Fade;
			UpDownItemlistDelayTime.Value = GHManager.Settings.Animate.ItemList_DelayTime;
			UpDownItemlistAnimateTime.Value = GHManager.Settings.Animate.ItemList_AnimateTime;
			CheckItemlistSlide.Checked = GHManager.Settings.Animate.ItemList_Slide;
			CheckItemlistFade.Checked = GHManager.Settings.Animate.ItemList_Fade;

			// キーボードページ
			Dictionary<int, string> ShortcutStrMap = new Dictionary<int, string> {
				{ GH_SHID.Show, "表示" },
				{ GH_SHID.Hide, "非表示" },
				{ GH_SHID.ShowConfig, "設定を表示" },
				{ GH_SHID.OpenSelectItem, "選択項目を実行" },
				{ GH_SHID.DeleteSelectItem, "選択項目を削除" },
				{ GH_SHID.SelectNextItem, "次の項目を選択" },
				{ GH_SHID.SelectPrevItem, "前の項目を選択" },
				{ GH_SHID.SelectNextGroup, "次のグループを選択" },
				{ GH_SHID.SelectPrevGroup, "前のグループを選択" },
				{ GH_SHID.SelectGroupTile, "グループのウィンドウを左右に並べて表示" },
				{ GH_SHID.SelectGroupTile2, "グループのウィンドウを上下に並べて表示" }
			};

			foreach (var item in GHManager.Settings.Hotkey.HotKeys.Select((kv, i) => new { kv, i })) {
				if (item.i > ListViewShortcut.Items.Count) break;
				string keystr = KeysToString(item.kv.Value.ModKey, item.kv.Value.Key);
				ListViewShortcut.Items[item.i].SubItems[1].Text = keystr;
				ListViewShortcut.Items[item.i].Tag = item.kv.Key;
				ListViewShortcut.Items[item.i].Text = ShortcutStrMap[item.kv.Key];
				PrevKeyStr.Add(item.kv.Key, keystr);
			}

			Dictionary<uint, string> wpKeyMap = new Dictionary<uint, string> {
				{ (uint)Keys.Menu, "Alt" },
				{ (uint)Keys.ControlKey, "Ctrl" },
				{ (uint)Keys.ShiftKey, "Shift" }
			};
			uint[] key = new uint[2] { (uint)Keys.ShiftKey, (uint)Keys.ControlKey };
			if (wpKeyMap.ContainsKey(GHManager.TempSettings.Magnet.GroupKey)) {
				key[0] = GHManager.TempSettings.Magnet.GroupKey;
			}
			if (wpKeyMap.ContainsKey(GHManager.TempSettings.Magnet.MoveKey)) {
				key[1] = GHManager.TempSettings.Magnet.MoveKey;
			}
			((RadioButton)PanelGroup.Controls["RadioButtonGroup" + wpKeyMap[key[0]]]).Checked = true;
			((RadioButton)PanelMove.Controls["RadioButtonMove" +   wpKeyMap[key[0]]]).Enabled = false;
			((RadioButton)PanelGroup.Controls["RadioButtonGroup" + wpKeyMap[key[1]]]).Enabled = false;
			((RadioButton)PanelMove.Controls["RadioButtonMove" +   wpKeyMap[key[1]]]).Checked = true;

			// マグネットページ
			TrackBarFitRange.Value = GHManager.Settings.Magnet.FitRange;
			LabelFitRange.Text = TrackBarFitRange.Value.ToString() + "px";
			CheckFitScreen.Checked = GHManager.Settings.Magnet.FitDisplay;
			CheckFitTaskbar.Checked = GHManager.Settings.Magnet.FitTaskbar;
			CheckFitWindow.Checked = GHManager.Settings.Magnet.FitWindows;
			
			//Dll.GetNoFitWindows(GHManager.settings.Magnet.NoFitWindows, 255);
			
			long[] arr = new long[255];

			Dll.GetAllWindows(arr);
			StringBuilder windowText = new StringBuilder(255);
			StringBuilder className = new StringBuilder(255);
			for (int i = 0; i < 255; ++i) {
				if(WinAPI.GetWindowText((IntPtr)arr[i], windowText, 255) != 0 && WinAPI.GetClassName((IntPtr)arr[i], className, 255) != 0) {
					ListBoxWindows.Items.Add("[" + i.ToString("000") + "] [class:" + className.ToString() + "] [caption:" + windowText.ToString() + "]");
				}
			}

			//for(int i = 0; i < 255; ++i) {
			//	if (GHManager.settings.Magnet.NoFitWindows[i].classname != "" &&
			//		GHManager.settings.Magnet.NoFitWindows[i].text != "") {
			//		ListBoxNoFitWindows.Items.Add("[" + i.ToString("000") + "] [class:" + GHManager.settings.Magnet.NoFitWindows[i].classname + "] [caption:" + GHManager.settings.Magnet.NoFitWindows[i].text + "]");
			//	}
			//}

			// スタイルページ
			UpDownLauncherItemSize.Value = GHManager.Settings.Style.Launcher.ItemSize;
			UpDownLauncherItemSpace.Value = GHManager.Settings.Style.Launcher.ItemSpace;
			ComboBoxGroupIcon.SelectedIndex = GHManager.Settings.GroupIconStyle;

			UpDownMysetItemSize.Value = GHManager.Settings.Style.MysetList.ItemSize;
			UpDownMysetItemSpace.Value = GHManager.Settings.Style.MysetList.ItemSpace;
			ComboBoxMysetIcon.SelectedIndex = GHManager.Settings.MysetIconStyle;

			UpDownItemListItemSizeHeight.Value = GHManager.Settings.Style.ItemList.ItemSizeHeight;
			UpDownItemListItemSizeWidth.Value = GHManager.Settings.Style.ItemList.ItemSizeWidth;
			UpDownItemListItemSpace.Value = GHManager.Settings.Style.ItemList.ItemSpace;
			UpDownItemList_Column.Value = GHManager.Settings.Style.ItemList.Column;
			UpDownItemListIconSize.Value = GHManager.Settings.Style.ItemList.IconSize;
			ComboboxUseIconSize.Text = GHManager.Settings.Style.ItemList.GetUseIconSize().ToString();

			ComboBoxDrawQuality.SelectedIndex = GHManager.Settings.DrawQuality;
			Load_Skin();
		}

		private void Config_FormClosing(object sender, FormClosingEventArgs e) {

			Page = ConfigTabControl.SelectedIndex;

			if (!ResultOK) {
				// 退避した設定に戻す
				GHManager.Rollback(1);
				Skin.LoadSkinImages();
			}
			else {
				for (int i = 0; i < ListViewShortcut.Items.Count; ++i) {
					if (ListViewShortcut.Items[i].ForeColor == Color.Red) {
						ConfigTabControl.SelectedIndex = 1;
						e.Cancel = true;
						ResultOK = false;
						return;
					}
				}
				// 現在の設定を保存
				GHManager.UnregistHotKey(GHManager.Launcher.Handle);
				GHManager.RegistHotKey(GHManager.Launcher.Handle);
				GHManager.SaveSetting();
			}

			GHManager.SaveClear();
		}

		#region ランチャー ページ

		// 表示位置
		private void ComboBoxLauncherPos_SelectedIndexChanged(object sender, EventArgs e) {

			int idx = ComboBoxLauncherPos.SelectedIndex;

			uint temp = (idx != -1 ? (uint)idx : 0);

			GHManager.TempSettings.Launcher.Pos = temp;

			if (prevLauncherPos != temp && prevLauncherPos != uint.MaxValue) {
				GHManager.Launcher.MovingCenter();
			}

			prevLauncherPos = temp;
		}

		// 認識範囲
		private void UpDownLauncherReactRange_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Launcher.ReactRange = UpDownCheckOfLimit(ref sender);
		}

		// マウスボタン押下時に表示
		private void CheckLauncherMouseButton_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Launcher.ShownMouseButton = ((CheckBox)sender).Checked;
		}

		// ランチャー
		private void UpDownLauncherDelayTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.Launcher_DelayTime = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownLauncherAnimateTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.Launcher_AnimateTime = UpDownCheckOfLimit(ref sender);
		}

		private void CheckLauncherSlide_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.Launcher_Slide = ((CheckBox)sender).Checked;
		}

		private void CheckLauncherFade_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.Launcher_Fade = ((CheckBox)sender).Checked;
		}

		// マイセットリスト
		private void UpDownMysetlistDelayTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.MysetList_DelayTime = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownMysetlistAnimateTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.MysetList_AnimateTime = UpDownCheckOfLimit(ref sender);
		}

		private void CheckMysetlistSlide_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.MysetList_Slide = ((CheckBox)sender).Checked;
		}

		private void CheckMysetlistFade_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.MysetList_Fade = ((CheckBox)sender).Checked;
		}

		// アイテムリスト
		private void UpDownItemlistDelayTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.ItemList_DelayTime = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemlistAnimateTime_ValueChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.ItemList_AnimateTime = UpDownCheckOfLimit(ref sender);
		}

		private void CheckItemlistSlide_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.ItemList_Slide = ((CheckBox)sender).Checked;
		}

		private void CheckItemlistFade_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Animate.ItemList_Fade = ((CheckBox)sender).Checked; ;
		}

		#endregion

		#region キーボード ページ
		
		// ショートカットキーの設定
		private void ListViewShortcut_KeyUp(object sender, KeyEventArgs e) {
			if (ListViewShortcut.SelectedItems.Count <= 0) return;

			if (e.KeyCode == Keys.Enter) {
				if (IsEntered) {
					IsEntered = false;
				}
				else {
					ListViewHitInfo = ListViewShortcut.HitTest(ListViewShortcut.SelectedItems[0].Position);
					if (ListViewHitInfo.SubItem != null) {
						Dll.SetKeyboardHook(true);
						HotKey = new HotKeyInfo();
						ListViewInput = new ListViewInputBox(ListViewShortcut, ListViewHitInfo.Item, 1);
						ListViewInput.FinishInput += new ListViewInputBox.InputEventHandler(Input_FinishInput);
						ListViewInput.Disposed += new EventHandler(Input_Disposed);
						ListViewInput.Show();
					}
				}
			}
			else if (e.KeyCode == Keys.Delete) {
				ListViewShortcut.SelectedItems[0].SubItems[1].Text = "";
				GHManager.TempSettings.Hotkey.HotKeys[(int)ListViewShortcut.SelectedItems[0].Tag].SetKeys(0, 0);
			}
		}

		// ショートカットキーの設定
		private void ListViewShortcut_MouseDoubleClick(object sender, MouseEventArgs e) {
			ListViewHitInfo = ListViewShortcut.HitTest(e.X, e.Y);
			if (ListViewHitInfo.SubItem != null && e.Button == MouseButtons.Left) {
				Dll.SetKeyboardHook(true);
				HotKey = new HotKeyInfo();
				ListViewInput = new ListViewInputBox(ListViewShortcut, ListViewHitInfo.Item, 1);
				ListViewInput.FinishInput += new ListViewInputBox.InputEventHandler(Input_FinishInput);
				ListViewInput.Disposed += new EventHandler(Input_Disposed);
				ListViewInput.Show();
			}
		}

		// ショートカットキー入力の確定
		public bool Input_FinishInput(object sender, ListViewInputBox.InputEventArgs e) {

			Dll.SetKeyboardHook(false);
			
			if (ListViewHitInfo != null && ListViewHitInfo.SubItem != null) {
				int index = ListViewHitInfo.Item.Index;
				List<int> col = new List<int> { index };

				if (PrevKeyStr[(int)ListViewHitInfo.Item.Tag] != e.NewName) {
					if (!GHManager.CheckRegistHotKey(e.HotKey.ModKey, e.HotKey.Key)) {
						col.Add(index);
					}
				}

				foreach (var item in GHManager.TempSettings.Hotkey.HotKeys.Select((kv, i) => new { kv, i })) {
					ListViewShortcut.Items[item.i].ForeColor = Color.FromKnownColor(KnownColor.WindowText);
					if ((int)ListViewHitInfo.Item.Tag == item.kv.Key) continue;
					if (e.HotKey.Equals(item.kv.Value)) {
						col.Add(item.i);
					}
				}

				if (col.Count > 1) {
					col.ForEach(i => ListViewShortcut.Items[i].ForeColor = Color.Red);
				}

				ListViewHitInfo.Item.SubItems[1].Text = e.NewName;
				GHManager.TempSettings.Hotkey.HotKeys[(int)ListViewHitInfo.Item.Tag].SetKeys(e.HotKey.ModKey, e.HotKey.Key);
			}
			else {
				return false;
			}

			HotKey = null;
			return true;
		}

		private void Input_Disposed(object sender, EventArgs e) {
			ListViewShortcut.Select();
			//ListViewShortcut.Focus();
		}

		// グループ化・同時移動のキーを設定
		private void MagnetKeyRadioButtons_CheckedChanged(object sender, EventArgs e) {
			bool group = Regex.Match(((RadioButton)sender).Name, "Group").Success;
			string key = ((RadioButton)sender).Text.Replace(" キー", string.Empty);
			string[] type = { "Group", "Move" };
			uint keycode = 0;
			Control[] controls = GroupBoxMagnetKey.Controls.Find("RadioButton" + type[group ? 1 : 0] + key, true);

			if (controls.Length <= 0) return;

			if (key == "Alt") keycode = (uint)Keys.Menu;
			else if (key == "Ctrl") keycode = (uint)Keys.ControlKey;
			else if (key == "Shift") keycode = (uint)Keys.ShiftKey;
			else keycode = (uint)Keys.Menu;

			if (group) {
				GHManager.TempSettings.Magnet.GroupKey = keycode;

				foreach (var obj in PanelMove.Controls) {
					if (obj is RadioButton) {
						((RadioButton)obj).Enabled = true;
					}
				}
			}
			else {
				GHManager.TempSettings.Magnet.MoveKey = keycode;

				foreach (var obj in PanelGroup.Controls) {
					if (obj is RadioButton) {
						((RadioButton)obj).Enabled = true;
					}
				}
			}

			((RadioButton)controls[0]).Enabled = false;
		}

		#endregion

		#region デザイン ページ

		// ランチャーアイテムサイズ
		private void UpDownLauncherItemSize_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.Launcher.ItemSize = UpDownCheckOfLimit(ref sender);
		}
		private void UpDownLauncherItemSpace_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.Launcher.ItemSpace = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownMysetItemSize_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.MysetList.ItemSize = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownMysetItemSpace_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.MysetList.ItemSpace = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemListItemSizeWidth_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.ItemList.ItemSizeWidth = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemListItemSizeHeight_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.ItemList.ItemSizeHeight = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemListItemSpace_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.ItemList.ItemSpace = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemList_Column_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.ItemList.Column = UpDownCheckOfLimit(ref sender);
		}

		private void UpDownItemListIconSize_Leave(object sender, EventArgs e) {
			GHManager.TempSettings.Style.ItemList.IconSize = UpDownCheckOfLimit(ref sender);
		}

		// 使用アイコンサイズ
		private void ComboboxUseIconSize_SelectedIndexChanged(object sender, EventArgs e) {
			if (int.TryParse(ComboboxUseIconSize.Text, out int size)) {
				GHManager.TempSettings.Style.ItemList.SetUseIconSize(size);
			}
			else {
				GHManager.TempSettings.Style.ItemList.SetUseIconSize((int)WinAPI.SHIL.SHIL_LARGE);
			}
		}

		// 余白・位置調整のボタン
		private void Button_Click(object sender, EventArgs e) {

			string buttonName = Regex.Replace(Regex.Replace(
				((Button)sender).Name, "^Button", string.Empty), "Padding", string.Empty).ToLower();

			int flag = 0;

			if (buttonName.IndexOf("launcher") != -1) flag += 1;
			else if (buttonName.IndexOf("myset") != -1) flag += 2;
			else if (buttonName.IndexOf("itemlist") != -1) flag += 4;

			if (buttonName.IndexOf("icon") != -1) flag += 8;
			else if (buttonName.IndexOf("text") != -1) flag += 16;
			else flag += 32;

			GHBaseStyle style = (flag & 1) == 1 ? GHManager.TempSettings.Style.Launcher :
						  (flag & 2) == 2 ? GHManager.TempSettings.Style.MysetList :
						  (flag & 4) == 4 ? GHManager.TempSettings.Style.ItemList : null;

			GHPadding padding = (flag & 8) == 8 ? style?.ItemPadding :
						(flag & 16) == 16 ? GHManager.TempSettings.Style.ItemList.TextPadding :
						style?.WindowPadding;

			if (padding == null) return;

			GHPadding temp = new GHPadding {
				Left = padding.Left,
				Top = padding.Top,
				Right = padding.Right,
				Bottom = padding.Bottom
			};

			using (Config_Sub f = new Config_Sub(ref padding)) {

				f.Text = Regex.Replace(((Button)sender).Text, "\\(&[A-Z]?\\)", string.Empty);

				if (f.ShowDialog() != DialogResult.OK) {
					padding.Left = temp.Left;
					padding.Top = temp.Top;
					padding.Right = temp.Right;
					padding.Bottom = temp.Bottom;
				}
			}
		}

		// フォント選択
		private void ButtonItemListFont_Click(object sender, EventArgs e) {

			using (FontDialog fd = new FontDialog()) {
				FontStyle fs = (FontStyle)Enum.ToObject(typeof(FontStyle), GHManager.TempSettings.Style.ItemList.FontStyles);
				fd.Font = new Font(GHManager.TempSettings.Style.ItemList.FontName, GHManager.TempSettings.Style.ItemList.FontSize, fs);
				fd.ShowColor = false;
				fd.ShowEffects = false;
				fd.ShowHelp = false;
				fd.ShowApply = false;
				
				if (fd.ShowDialog() == DialogResult.OK) {
					GHManager.TempSettings.Style.ItemList.FontName = fd.Font.FontFamily.Name;
					GHManager.TempSettings.Style.ItemList.FontSize = fd.Font.Size;
					GHManager.TempSettings.Style.ItemList.FontStyles = (int)fd.Font.Style;
				}

			}

		}

		// 色選択
		private void ButtonItemListFontColor_Click(object sender, EventArgs e) {
			using (ColorDialog cd = new ColorDialog()) {
				GHColor color = GHManager.TempSettings.Style.ItemList.FontColor;
				cd.Color = Color.FromArgb(color.Red, color.Green, color.Blue);
				
				if (cd.ShowDialog() == DialogResult.OK) {
					GHManager.TempSettings.Style.ItemList.FontColor.SetColor(cd.Color.R, cd.Color.G, cd.Color.B);
				}
			}
		}


		// スキンの選択
		private void ListBoxSkin_SelectedIndexChanged(object sender, EventArgs e) {
			if (ListBoxSkin.SelectedItem != null) {
				if (ListBoxSkin.SelectedIndex == 0) {
					GHManager.TempSettings.SkinName = "";
				}
				else {
					GHManager.TempSettings.SkinName = ListBoxSkin.SelectedItem.ToString();
				}
				
				Skin.LoadTempSkinImages();
			}
		}
		
		#endregion

		#region マグネット ページ

		private void TrackBarFitRange_Scroll(object sender, EventArgs e) {
			GHManager.TempSettings.Magnet.FitRange = TrackBarFitRange.Value;
			LabelFitRange.Text = TrackBarFitRange.Value.ToString() + "px";
		}

		private void CheckFitScreen_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Magnet.FitDisplay = CheckFitScreen.Checked;
		}

		private void CheckFitTaskbar_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Magnet.FitTaskbar = CheckFitTaskbar.Checked;
		}

		private void CheckFitWindow_CheckedChanged(object sender, EventArgs e) {
			GHManager.TempSettings.Magnet.FitWindows = CheckFitWindow.Checked;
		}

		#endregion

		private void ConfigTabControl_Selected(object sender, TabControlEventArgs e) {
			if (ConfigTabControl.SelectedIndex != 2) {
				for (int i = 0; i < ListViewShortcut.Items.Count; ++i) {
					if (ListViewShortcut.Items[i].ForeColor == Color.Red) {
						ConfigTabControl.SelectedIndex = 2;
						MessageBox.Show("重複するショートカットキーが存在します。");
						break;
					}
				}
			}
		}

		private void ButtonCancel_Click(object sender, EventArgs e) {
			ResultOK = false;
			Close();
		}

		private void ButtonOK_Click(object sender, EventArgs e) {
			ResultOK = true;
			Close();
		}

		private void ComboBoxDrawQuality_SelectedIndexChanged(object sender, EventArgs e) {
			int idx = ComboBoxDrawQuality.SelectedIndex;
			if (idx < 0 || 2 < idx) {
				idx = 0;
			}
			GHManager.TempSettings.DrawQuality = idx;
		}

		private void ComboBoxGroupIcon_SelectedIndexChanged(object sender, EventArgs e) {
			int idx = ComboBoxGroupIcon.SelectedIndex;
			if (idx < 0 || 2 < idx) {
				idx = 0;
			}
			GHManager.TempSettings.GroupIconStyle = idx;
		}

		private void ComboBoxMysetIcon_SelectedIndexChanged(object sender, EventArgs e) {
			int idx = ComboBoxMysetIcon.SelectedIndex;
			if (idx < 0 || 2 < idx) {
				idx = 0;
			}
			GHManager.TempSettings.MysetIconStyle = idx;
		}

	}

	public class ListViewInputBox : TextBox {
		public class InputEventArgs : EventArgs {
			public string Path = "";
			public string NewName = "";
			public HotKeyInfo HotKey = new HotKeyInfo();
		}

		public delegate bool InputEventHandler(object sender, InputEventArgs e);

		//イベントデリゲートの宣言
		public event InputEventHandler FinishInput;

		private InputEventArgs args = new InputEventArgs();
		private bool finished = false;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent">対象となるListViewコントロール</param>
		/// <param name="item">編集対象のアイテム</param>
		/// <param name="subitem_index">編集する対象の列</param>
		public ListViewInputBox(ListView parent, ListViewItem item, int subitem_index) : base() {
			args.Path = item.SubItems[0].Text;
			args.NewName = item.SubItems[1].Text;

			int left = 0;
			for (int i = 0; i < subitem_index; i++) {
				left += parent.Columns[i].Width;
			}
			int width = item.SubItems[subitem_index].Bounds.Width;
			int height = item.SubItems[subitem_index].Bounds.Height - 4;

			this.Parent = parent;
			this.Size = new Size(width, height);
			this.Left = left;
			this.Top = item.Position.Y - 1;
			this.Text = item.SubItems[subitem_index].Text;
			this.ReadOnly = true;
			this.BackColor = Color.White;
			this.LostFocus += new EventHandler(Textbox_LostFocus);
			this.ImeMode = ImeMode.NoControl;
			this.Multiline = false;
			this.KeyDown += new KeyEventHandler(Textbox_KeyDown);
			this.Focus();
		}

		void Finish(string new_name) {
			// Enterで入力を完了した場合はKeyDownが呼ばれた後に
			// さらにLostFocusが呼ばれるため，二回Finishが呼ばれる
			if (!finished) {
				// textbox.Hide()すると同時にLostFocusするため，
				// finished=trueを先に呼び出しておかないと，
				// このブロックが二回呼ばれてしまう．
				args.NewName = new_name;
				FinishInput(this, args);
				finished = true;
				this.Hide();
				Dispose();
			}
		}

		void Textbox_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				Finish(this.Text);
			}
			else if (e.KeyCode == Keys.Escape) {
				Finish(args.NewName);
			}
			e.Handled = true;
		}

		public void Textbox_SetValue(Keys eventKey, HotKeyInfo hotKey) {
			if(eventKey == Keys.Enter) {
				args.HotKey.ModKey = hotKey.ModKey;
				args.HotKey.Key = hotKey.Key;
			}
			Textbox_KeyDown(this, new KeyEventArgs(eventKey));
		}

		void Textbox_LostFocus(object sender, EventArgs e) {
			Finish(this.Text);
		}

	}

}
