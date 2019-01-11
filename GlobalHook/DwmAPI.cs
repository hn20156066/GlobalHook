using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GH {

	/// <summary>
	/// DWMに関する追加の関数
	/// </summary>
	public static class DwmAPI {

		private static bool IsAero = false;

		public static void GetWindowRect(IntPtr hwnd, out Rectangle rectangle) {
			WinAPI.DwmIsCompositionEnabled(out IsAero);
			WinAPI.RECT rect = new WinAPI.RECT();

			if (IsAero) {
				WinAPI.DwmGetWindowAttribute(hwnd, WinAPI.DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out rect, Marshal.SizeOf(rect));
			}
			else {
				WinAPI.GetWindowRect(hwnd, out rect);
			}

			rectangle = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
		}



	}
}
