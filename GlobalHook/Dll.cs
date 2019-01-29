using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GH {

	/// <summary>
	/// GlobalHook.Dll内の関数
	/// </summary>
	public static class Dll {

		[DllImport("GlobalHook.dll")]
		[return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
		public static extern long CwpProc(int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("GlobalHook.dll")]
		[return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
		public static extern long MouseProc(int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("GlobalHook.dll")]
		[return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
		public static extern long KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		private static extern void SetLancherText(char[] windowText);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		public static extern void SetConfigText(char[] windowText);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		private static extern void SetLauncherClassName(char[] className);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		private static extern void SetSubClassName(char[] mysetlist, char[] itemlist);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		public static extern void SetConfigClassName(char[] className);
		[DllImport("GlobalHook.dll", CharSet = CharSet.Unicode)]
		public static extern void SetSubConfigClassName(char[] className);
		[DllImport("GlobalHook.dll")]
		private static extern int SetHook();
		[DllImport("GlobalHook.dll")]
		private static extern int ResetHook();
		[DllImport("GlobalHook.dll")]
		public static extern WinAPI.DSIZE GetScale2(long hwnd);
		[DllImport("GlobalHook.dll")]
		public static extern void SetFitRange(int range);
		[DllImport("GlobalHook.dll")]
		public static extern void SetFitRangeLimit(int min, int max);
		[DllImport("GlobalHook.dll")]
		public static extern int GetFitRange();
		[DllImport("GlobalHook.dll")]
		public static extern int GetFitRangeMax();
		[DllImport("GlobalHook.dll")]
		public static extern int GetFitRangeMin();
		[DllImport("GlobalHook.dll")]
		public static extern void SetFitWindows(bool flag);
		[DllImport("GlobalHook.dll")]
		public static extern void SetFitTaskbar(bool flag);
		[DllImport("GlobalHook.dll")]
		public static extern void SetFitDisplay(bool flag);
		[DllImport("GlobalHook.dll")]
		public static extern bool GetFitWindows();
		[DllImport("GlobalHook.dll")]
		public static extern bool GetFitTaskbar();
		[DllImport("GlobalHook.dll")]
		public static extern bool GetFitDisplay();
		[DllImport("GlobalHook.dll")]
		public static extern void SetGroupKey(uint key);
		[DllImport("GlobalHook.dll")]
		public static extern uint GetGroupKey();
		[DllImport("GlobalHook.dll")]
		public static extern void SetMoveKey(uint key);
		[DllImport("GlobalHook.dll")]
		public static extern uint GetMoveKey();
		[DllImport("GlobalHook.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void GetWindows(IntPtr arr, int length);
		[DllImport("GlobalHook.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetNoFitWindows(IntPtr arr, int length);
		[DllImport("GlobalHook.dll")]
		public static extern void GetNoFitWindows([In, Out]Window[] windowArray, int size);
		[DllImport("GlobalHook.dll")]
		public static extern void SetNoFitWindows([In, Out]Window[] windowArray, int size);
		[DllImport("GlobalHook.dll")]
		public static extern void SetKeyboardHook(bool flag);

		private static bool DllLoadSuccess = false;

		public static bool StartHook() {

			try {
				SetLancherText(GHManager.Launcher.Text.ToCharArray());
				StringBuilder sb = new StringBuilder(255);
				string[] className = new string[3];
				WinAPI.GetClassName(GHManager.Launcher.Handle, sb, 255);
				className[0] = sb.ToString();
				WinAPI.GetClassName(GHManager.MysetList.Handle, sb, 255);
				className[1] = sb.ToString();
				WinAPI.GetClassName(GHManager.ItemList.Handle, sb, 255);
				className[2] = sb.ToString();
				SetLauncherClassName(className[0].ToCharArray());
				SetSubClassName(className[1].ToCharArray(), className[2].ToCharArray());
				DllLoadSuccess = SetHook() != -1;
			}
			catch(Exception ex) {
				Console.WriteLine(ex.Message);
				DllLoadSuccess = false;
			}

			return DllLoadSuccess;
		}

		public static bool EndHook() {

			if (DllLoadSuccess) {
				return ResetHook() != -1;
			}
			else {
				return false;
			}

		}

		public static void GetAllWindows(long[] arr) {
			const int length = 255;
			IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(long)) * length);
			GetWindows(ptr, length);
			Marshal.Copy(ptr, arr, 0, length);
			Marshal.FreeCoTaskMem(ptr);
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct Window {
			public string classname;
			public string text;
			public Window(string className, string text) {
				this.classname = className;
				this.text = text;
			}
		}
		
	}

}
