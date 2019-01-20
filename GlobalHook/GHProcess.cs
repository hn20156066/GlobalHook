using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Shell32;
using SHDocVw;

namespace GH {

	/// <summary>
	/// プロセスに関するクラス
	/// </summary>
	public static class GHProcess {
		
		/// <summary>
		/// ハンドルからプロセスのアイコンを取得
		/// </summary>
		/// <param name="hwnd">ウィンドウハンドル</param>
		/// <param name="icon">アイコンの読み込み先</param>
		/// <returns></returns>
		public static bool GetProcessIcon(ref long hwnd, out Icon icon) {
			StringBuilder path = new StringBuilder(255);

			if (GetProcessPath(ref hwnd, out path)) {
				WinAPI.Icon.SHFILEINFO shinfo = new WinAPI.Icon.SHFILEINFO();
				IntPtr pimgList = IntPtr.Zero;
				
				string filename = path.ToString();

				try {
					WinAPI.Icon.SHGetFileInfo(filename, 0, out shinfo, (uint)Marshal.SizeOf(typeof(WinAPI.Icon.SHFILEINFO)), WinAPI.Icon.SHGFI.SHGFI_SYSICONINDEX);

					WinAPI.Icon.SHIL currentshil = (WinAPI.Icon.SHIL)GHManager.Settings.Style.ItemList.UseIconSize;

					int rsult = WinAPI.Icon.SHGetImageList(currentshil, ref WinAPI.Icon.IID_IImageList, out pimgList);
					icon = Icon.FromHandle(WinAPI.Icon.ImageList_GetIcon(pimgList, shinfo.iIcon, 0));
				}
				catch (Exception e) {
					Console.WriteLine("GetProcessIcon: " + e.Message);
					icon = null;
				}

				WinAPI.Icon.DestroyIcon(shinfo.hIcon);
				WinAPI.Icon.ImageList_Destroy(pimgList);
				shinfo.hIcon = IntPtr.Zero;
				pimgList = IntPtr.Zero;

				return true;
			}
			else {
				icon = null;
				return false;
			}
		}

		/// <summary>
		/// ハンドルからプロセスの実行パスを取得
		/// </summary>
		/// <param name="hwnd">ウィンドウハンドル</param>
		/// <param name="path">実行パス</param>
		/// <returns></returns>
		public static bool GetProcessPath(ref long hwnd, out StringBuilder path) {
			path = new StringBuilder(255);
			WinAPI.GetWindowThreadProcessId((IntPtr)hwnd, out int processId);
			IntPtr hProcess = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.QueryInformation | WinAPI.ProcessAccessFlags.PROCESS_VM_READ, false, (uint)processId);
			IntPtr hModule = IntPtr.Zero;
			if (hProcess != null) {
				if (WinAPI.EnumProcessModules(hProcess, out hModule, Marshal.SizeOf(hModule), out int nReturned)) {
					if (WinAPI.GetModuleFileNameEx(hProcess, hModule, path, 1024) != 0) {
						WinAPI.CloseHandle(hProcess);
						return true;
					}
				}
			}

			WinAPI.CloseHandle(hProcess);
			return false;
		}

		public static bool GetPathIcon(ref StringBuilder path, out Icon icon) {

			if (System.IO.File.Exists(path.ToString())) {
				WinAPI.Icon.SHFILEINFO shinfo = new WinAPI.Icon.SHFILEINFO();
				IntPtr pimgList = IntPtr.Zero;

				string filename = path.ToString();
				WinAPI.Icon.SHGetFileInfo(filename, 0, out shinfo, (uint)Marshal.SizeOf(typeof(WinAPI.Icon.SHFILEINFO)), WinAPI.Icon.SHGFI.SHGFI_SYSICONINDEX);

				WinAPI.Icon.SHIL currentshil = (WinAPI.Icon.SHIL)GHManager.Settings.Style.ItemList.UseIconSize;

				int rsult = WinAPI.Icon.SHGetImageList(currentshil, ref WinAPI.Icon.IID_IImageList, out pimgList);
				icon = Icon.FromHandle(WinAPI.Icon.ImageList_GetIcon(pimgList, shinfo.iIcon, 0));

				WinAPI.Icon.DestroyIcon(shinfo.hIcon);
				WinAPI.Icon.ImageList_Destroy(pimgList);
				shinfo.hIcon = IntPtr.Zero;
				pimgList = IntPtr.Zero;

				return true;
			}
			else {
				icon = null;
				return false;
			}
		}

		public static void Show(IntPtr hwnd) {
			WinAPI.ShowWindow(hwnd, (int)WinAPI.ShowFlags.SW_SHOWNA);
		}

		public static void Normalize(IntPtr hwnd) {
			WinAPI.ShowWindow(hwnd, (int)WinAPI.ShowFlags.SW_RESTORE);
		}

		public static void Minimize(IntPtr hwnd) {
			WinAPI.ShowWindow(hwnd, (int)WinAPI.ShowFlags.SW_MINIMIZE);
		}

		public static void Maximize(IntPtr hwnd) {
			WinAPI.ShowWindow(hwnd, (int)WinAPI.ShowFlags.SW_MAXIMIZE);
		}

		public static void Active(IntPtr hwnd) {
			WinAPI.SetForegroundWindow(hwnd);
		}

		public static bool IsNormalize(IntPtr hwnd) {
			return !(WinAPI.IsIconic(hwnd) | WinAPI.IsZoomed(hwnd));
		}

		public static bool IsMinimize(IntPtr hwnd) {
			return WinAPI.IsIconic(hwnd);
		}

		public static bool IsMaximize(IntPtr hwnd) {
			return WinAPI.IsZoomed(hwnd);
		}

		public static bool IsActive(IntPtr hwnd) {
			return (hwnd == WinAPI.GetForegroundWindow());
		}

		public static void SwitchShowOrHide(IntPtr hwnd) {

			if (IsMinimize(hwnd)) {
				Normalize(hwnd);
			}
			else {
				if (IsActive(hwnd)) {
					Minimize(hwnd);
				}
				else {
					Active(hwnd);
				}
			}
		}

		public static long StartProcess(StringBuilder filename, StringBuilder arg, Rectangle rect) {

			if (filename == null) return 0L;
			if (arg == null) arg = new StringBuilder("");

			Process process = new Process {
				StartInfo = new ProcessStartInfo(filename.ToString(), arg.ToString())
			};

			if (process.Start()) {
				//process.WaitForInputIdle(10000);

				//if (process.MainWindowHandle != IntPtr.Zero) {
				//	//WinAPI.MoveWindow(process.MainWindowHandle, rect.Left, rect.Top, rect.Width, rect.Height, 1);
				//	return (long)process.MainWindowHandle;
				//}
				//else {
				//	// エクスプローラのウィンドウハンドルを取得できない
				//	// ウィンドウが無いエクスプローラのプロセスが常時動作しているため

				//	// 方法1
				//	// スレッドIDとプロセスIDの比較
				//	// processのスレッドIDとプロセスIDが存在しないので多分無理（Spy++）
				//	// 方法2
				//	// タイトルとクラスの比較
				//	// タイトルとクラス名が一致するウィンドウを列挙する
				//	// 場合によって複数存在するため、無作為に１つ選ぶ
				//	// グループ化されていないウィンドウも範囲に含まれるため確実ではない
				//	// 方法3
				//	// ウィンドウの生成をフックし取得
				//	// プロセスの実行前にDLLのウィンドウの生成をフックするためのbool値をtrueにする
				//	// DLLがウィンドウの生成をフック
				//	// EXEにフックしたウィンドウのハンドルを送信する
				//	// DLLのウィンドウの生成をフックするためのbool値をfalseにする
				//	// bool値をtrueにしてからフックすべきウィンドウが生成されるまでの間に
				//	// 別のウィンドウが生成された時、全く別のハンドルを送信してしまう


				//	return 0L;
				//}
			}
			else {
				return 0L;
			}

			return 0L;
		}
	}
}
