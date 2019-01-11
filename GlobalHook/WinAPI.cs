using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace GH {

	/// <summary>
	/// WindowsAPI の関数や定数などを宣言したクラス
	/// </summary>
	public static partial class WinAPI {

		public const int WM_COPYDATA = 0x004A;
		public const int WM_CLOSE = 0x0010;
		public const int WM_SHOWWINDOW = 0x0018;
		public const int WM_HOTKEY = 0x0312;

		public const int WS_EX_LAYERED = 0x00080000;
		public const int WS_EX_NOACTIVATE = 0x8000000;

		public const int DWM_TNP_VISIBLE = 0x8;
		public const int DWM_TNP_OPACITY = 0x4;
		public const int DWM_TNP_RECTDESTINATION = 0x1;

		public enum DWMWINDOWATTRIBUTE : uint {
			NCRenderingEnabled = 1,
			NCRenderingPolicy,
			TransitionsForceDisabled,
			AllowNCPaint,
			CaptionButtonBounds,
			NonClientRtlLayout,
			ForceIconicRepresentation,
			Flip3DPolicy,
			ExtendedFrameBounds,
			HasIconicBitmap,
			DisallowPeek,
			ExcludedFromPeek,
			Cloak,
			Cloaked,
			FreezeRepresentation
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT {
			public int length;
			public int flags;
			public ShowFlags showCmd;
			public POINT minPosition;
			public POINT maxPosition;
			public RECT normalPosition;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public int X;
			public int Y;

			public POINT(int x, int y) {
				this.X = x;
				this.Y = y;
			}
		}

		public struct POINTW {
			public long x;
			public long y;

			public POINTW(long x, long y) {
				this.x = x;
				this.y = y;
			}
		}

		public struct DSIZE {
			public double cx;
			public double cy;

			public DSIZE(double x, double y) {
				cx = x;
				cy = y;
			}
		}

		public const int WPF_SETMINPOSITION = 0x0001;
		public const int WPF_RESTORETOMAXIMIZED = 0x0002;

		[DllImport("user32.dll")]
		public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		public struct PSIZE {
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect {
			public Rect(int left, int top, int right, int bottom) {
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DWM_THUMBNAIL_PROPERTIES {
			public int dwFlags;
			public Rect rcDestination;
			public Rect rcSource;
			public byte opacity;
			public bool fVisible;
			public bool fSourceClientAreaOnly;
		}

		[DllImport("dwmapi.dll")]
		public static extern int DwmIsCompositionEnabled(out bool enabled);

		[DllImport("dwmapi.dll")]
		public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttrivute, int cbAttribute);

		[DllImport("dwmapi.dll")]
		public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUnregisterThumbnail(IntPtr thumb);
		
		[DllImport("dwmapi.dll")]
		public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

		[DllImport("dwmapi.dll")]
		public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

		public const Int32 FILE_ATTRIBUTE_NORMAL = 0x80;
		public static Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");

		public struct Neighbor {
			public long parent;
			public long child;
		}

		public struct KeyInfo {
			public uint keyflag;
			public uint keycode;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct COPYDATASTRUCT {
			public IntPtr dwData;
			public int cbData;
			//[MarshalAs(UnmanagedType.)]
			public IntPtr lpData;
		}

		public enum MOD_KEY : int {
			ALT = 0x0001,
			CTRL = 0x0002,
			SHIFT = 0x0004,
			WIN = 0x0008
		}

		[DllImport("user32.dll")]
		public static extern int RegisterHotKey(IntPtr hWnd, int id, uint modKey, uint key);

		[DllImport("user32.dll")]
		public static extern int UnregisterHotKey(IntPtr hWnd, int id);
		
		public const int SWP_NOSIZE = 0x0001;
		public const int SWP_NOMOVE = 0x0002;
		public const int SWP_NOZORDER = 0x0004;
		public const int SWP_NOREDRAW = 0x0008;
		public const int SWP_NOACTIVATE = 0x0010;
		public const int SWP_SHOWWINDOW = 0x0040;

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		public enum ShowFlags : uint {
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
			SW_FORCEMINIMIZE = 11
		}



		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern int GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, int nSize);

		[DllImport("Psapi.dll", SetLastError = true)]
		public static extern bool EnumProcesses([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] UInt32[] processIds,
			UInt32 arraySizeBytes,[MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

		[DllImport("psapi.dll")]
		public static extern bool EnumProcessModules(IntPtr hProcess, out IntPtr lphModule, int cb, out int lpcbNeeded);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);
		// 出現するフラグを列挙
		public enum ProcessAccessFlags : uint {
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VMOperation = 0x00000008,
			PROCESS_VM_READ = 0x10,
			VMWrite = 0x00000020,
			DupHandle = 0x00000040,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			Synchronize = 0x00100000
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int length);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int length);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SendMessage(IntPtr pWnd, UInt32 uMsg, UInt32 wParam, IntPtr lParam);

		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		public static extern Int32 SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, UInt32 lParam);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int DeleteObject(IntPtr hobject);

		public static readonly byte AC_SRC_OVER = 0;
		public static readonly byte AC_SRC_ALPHA = 1;
		public static readonly int ULW_ALPHA = 2;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BLENDFUNCTION {
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, [In()]ref Point pptDst, [In()]ref Size psize,
			IntPtr hdcSrc, [In()]ref Point pptSrc, int crKey, [In()]ref BLENDFUNCTION pblend, int dwFlags);

		[DllImport("user32.dll")]
		public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		public const uint LWA_ALPHA = 0x2;
		public const uint LWA_COLORKEY = 0x1;

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SHGetDesktopFolder(ref IShellFolder ppshf);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SHGetFileInfo(IntPtr pIDL, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SHGetSpecialFolderLocation(IntPtr hwndOwner, CSIDL nFolder, ref IntPtr ppidl);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ILCombine(IntPtr pIDLParent, IntPtr pIDLChild);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ILClone(IntPtr pidl);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern Int32 SHGetPathFromIDList(IntPtr pIDL, StringBuilder strPath);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHGetFolderLocation(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwReserved, out IntPtr ppidl);

		//[DllImport("shell32.dll")]
		//public static extern int SHGetDataFromIDList(IShellFolder psf, ref IntPtr pidl, SHGDFIL nFormat, IntPtr pv, int cb);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHGetDataFromIDList(IShellFolder psf, IntPtr pidl, SHGDFIL nFormat, out WIN32_FIND_DATA pv, int cb);

		[DllImport("shell32.dll", EntryPoint = "#727")]
		public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IImageList ppv);

		//IMAGE LIST
		public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
		public static Guid IID_IImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");

		[Flags]
		public enum ImageListDrawItemConstants : int {
			/// <summary>
			/// Draw item normally.
			/// </summary>
			ILD_NORMAL = 0x0,
			/// <summary>
			/// Draw item transparently.
			/// </summary>
			ILD_TRANSPARENT = 0x1,
			/// <summary>
			/// Draw item blended with 25% of the specified foreground colour
			/// or the Highlight colour if no foreground colour specified.
			/// </summary>
			ILD_BLEND25 = 0x2,
			/// <summary>
			/// Draw item blended with 50% of the specified foreground colour
			/// or the Highlight colour if no foreground colour specified.
			/// </summary>
			ILD_SELECTED = 0x4,
			/// <summary>
			/// Draw the icon's mask
			/// </summary>
			ILD_MASK = 0x10,
			/// <summary>
			/// Draw the icon image without using the mask
			/// </summary>
			ILD_IMAGE = 0x20,
			/// <summary>
			/// Draw the icon using the ROP specified.
			/// </summary>
			ILD_ROP = 0x40,
			/// <summary>
			/// ?
			/// </summary>
			ILD_OVERLAYMASK = 0xF00,
			/// <summary>
			/// Preserves the alpha channel in dest. XP only.
			/// </summary>
			ILD_PRESERVEALPHA = 0x1000, // 
										/// <summary>
										/// Scale the image to cx, cy instead of clipping it.  XP only.
										/// </summary>
			ILD_SCALE = 0x2000,
			/// <summary>
			/// Scale the image to the current DPI of the display. XP only.
			/// </summary>
			ILD_DPISCALE = 0x4000
		}

		[Flags]
		public enum SHIL : int {
			// 32
			SHIL_LARGE = 0x0,
			// 16
			SHIL_SMALL = 0x1,
			// 48
			SHIL_EXTRALARGE = 0x2,
			// 256
			SHIL_JUMBO = 0x4
		}

		/*
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern uint SHGetDataFromIDList(IShellFolder psf, IntPtr pidl, SHGDFIL nFormat, out WIN32_FIND_DATA pv, int cb); 
		*/
		/*
		public struct ITEMIDLIST
		{
		  public SHITEMID mkid;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHITEMID
		{
		  public ushort cb; // The size of identifier, in bytes, including cb itself.

		  //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		  [MarshalAs(UnmanagedType.LPStr)]
		  public byte[] abID; // A variable-length item identifier.
		}
		*/

		[Flags()]
		public enum SHGDFIL : int {
			SHGDFIL_FINDDATA = 1,
			SHGDFIL_NETRESOURCE = 2,
			SHGDFIL_DESCRIPTIONID = 3
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct WIN32_FIND_DATA {
			public uint dwFileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}


		[Flags]
		public enum SHGDN : uint {
			SHGDN_NORMAL = 0x0000,                 // Default (display purpose)
			SHGDN_INFOLDER = 0x0001,               // Displayed under a folder (relative)
			SHGDN_FOREDITING = 0x1000,             // For in-place editing
			SHGDN_FORADDRESSBAR = 0x4000,          // UI friendly parsing name (remove ugly stuff)
			SHGDN_FORPARSING = 0x8000,             // Parsing name for ParseDisplayName()
		}

		[Flags]
		public enum SHCONTF : uint {
			SHCONTF_CHECKING_FOR_CHILDREN = 0x0010,
			SHCONTF_FOLDERS = 0x0020,              // Only want folders enumerated (SFGAO_FOLDER)
			SHCONTF_NONFOLDERS = 0x0040,           // Include non folders
			SHCONTF_INCLUDEHIDDEN = 0x0080,        // Show items normally hidden
			SHCONTF_INIT_ON_FIRST_NEXT = 0x0100,   // Allow EnumObject() to return before validating enum
			SHCONTF_NETPRINTERSRCH = 0x0200,       // Hint that client is looking for printers
			SHCONTF_SHAREABLE = 0x0400,            // Hint that client is looking sharable resources (remote shares)
			SHCONTF_STORAGE = 0x0800,              // Include all items with accessible storage and their ancestors
			SHCONTF_NAVIGATION_ENUM = 0x01000,
			SHCONTF_FASTITEMS = 0x02000,
			SHCONTF_FLATLIST = 0x04000,
			SHCONTF_ENABLE_ASYNC = 0x08000,
			SHCONTF_INCLUDESUPERHIDDEN = 0x10000,
		}

		[Flags]
		public enum SFGAOF : uint {
			SFGAO_CANCOPY = 0x1,                   // Objects can be copied  (DROPEFFECT_COPY)
			SFGAO_CANMOVE = 0x2,                   // Objects can be moved   (DROPEFFECT_MOVE)
			SFGAO_CANLINK = 0x4,                   // Objects can be linked  (DROPEFFECT_LINK)
			SFGAO_STORAGE = 0x00000008,            // Supports BindToObject(IID_IStorage)
			SFGAO_CANRENAME = 0x00000010,          // Objects can be renamed
			SFGAO_CANDELETE = 0x00000020,          // Objects can be deleted
			SFGAO_HASPROPSHEET = 0x00000040,       // Objects have property sheets
			SFGAO_DROPTARGET = 0x00000100,         // Objects are drop target
			SFGAO_CAPABILITYMASK = 0x00000177,
			SFGAO_SYSTEM = 0x00001000,
			SFGAO_ENCRYPTED = 0x00002000,          // Object is encrypted (use alt color)
			SFGAO_ISSLOW = 0x00004000,             // 'Slow' object
			SFGAO_GHOSTED = 0x00008000,            // Ghosted icon
			SFGAO_LINK = 0x00010000,               // Shortcut (link)
			SFGAO_SHARE = 0x00020000,              // Shared
			SFGAO_READONLY = 0x00040000,           // Read-only
			SFGAO_HIDDEN = 0x00080000,             // Hidden object
			SFGAO_DISPLAYATTRMASK = 0x000FC000,
			SFGAO_FILESYSANCESTOR = 0x10000000,    // May contain children with SFGAO_FILESYSTEM
			SFGAO_FOLDER = 0x20000000,             // Support BindToObject(IID_IShellFolder)
			SFGAO_FILESYSTEM = 0x40000000,         // Is a win32 file system object (file/folder/root)
			SFGAO_HASSUBFOLDER = 0x80000000,       // May contain children with SFGAO_FOLDER
			SFGAO_CONTENTSMASK = 0x80000000,
			SFGAO_VALIDATE = 0x01000000,           // Invalidate cached information
			SFGAO_REMOVABLE = 0x02000000,          // Is this removeable media?
			SFGAO_COMPRESSED = 0x04000000,         // Object is compressed (use alt color)
			SFGAO_BROWSABLE = 0x08000000,          // Supports IShellFolder, but only implements CreateViewObject() (non-folder view)
			SFGAO_NONENUMERATED = 0x00100000,      // Is a non-enumerated object
			SFGAO_NEWCONTENT = 0x00200000,         // Should show bold in explorer tree
			SFGAO_CANMONIKER = 0x00400000,         // Defunct
			SFGAO_HASSTORAGE = 0x00400000,         // Defunct
			SFGAO_STREAM = 0x00400000,             // Supports BindToObject(IID_IStream)
			SFGAO_STORAGEANCESTOR = 0x00800000,    // May contain children with SFGAO_STORAGE or SFGAO_STREAM
			SFGAO_STORAGECAPMASK = 0x70C50008,     // For determining storage capabilities, ie for open/save semantics
		}

		[Flags]
		public enum STRRET : uint {
			STRRET_WSTR = 0,
			STRRET_OFFSET = 0x1,
			STRRET_CSTR = 0x2,
		}

		[Flags]
		public enum SHGFI {
			SHGFI_ICON = 0x000000100,
			SHGFI_DISPLAYNAME = 0x000000200,
			SHGFI_TYPENAME = 0x000000400,
			SHGFI_ATTRIBUTES = 0x000000800,
			SHGFI_ICONLOCATION = 0x000001000,
			SHGFI_EXETYPE = 0x000002000,
			SHGFI_SYSICONINDEX = 0x000004000,
			SHGFI_LINKOVERLAY = 0x000008000,
			SHGFI_SELECTED = 0x000010000,
			SHGFI_ATTR_SPECIFIED = 0x000020000,
			SHGFI_LARGEICON = 0x000000000,
			SHGFI_SMALLICON = 0x000000001,
			SHGFI_OPENICON = 0x000000002,
			SHGFI_SHELLICONSIZE = 0x000000004,
			SHGFI_PIDL = 0x000000008,
			SHGFI_USEFILEATTRIBUTES = 0x000000010,
			SHGFI_ADDOVERLAYS = 0x000000020,
			SHGFI_OVERLAYINDEX = 0x000000040
		}

		/*
		[Flags]
		public enum SHGDNF : uint
		{
		  SHGDN_NORMAL = 0,
		  SHGDN_INFOLDER = 0x1,
		  SHGDN_FOREDITING = 0x1000,
		  SHGDN_FORADDRESSBAR = 0x4000,
		  SHGDN_FORPARSING = 0x8000
		}
		*/

		[Flags]
		public enum CSIDL : uint {
			CSIDL_DESKTOP = 0x0000,    // <desktop>
			CSIDL_INTERNET = 0x0001,    // Internet Explorer (icon on desktop)
			CSIDL_PROGRAMS = 0x0002,    // Start Menu\Programs
			CSIDL_CONTROLS = 0x0003,    // My Computer\Control Panel
			CSIDL_PRINTERS = 0x0004,    // My Computer\Printers
			CSIDL_PERSONAL = 0x0005,    // My Documents
			CSIDL_FAVORITES = 0x0006,    // <user name>\Favorites
			CSIDL_STARTUP = 0x0007,    // Start Menu\Programs\Startup
			CSIDL_RECENT = 0x0008,    // <user name>\Recent
			CSIDL_SENDTO = 0x0009,    // <user name>\SendTo
			CSIDL_BITBUCKET = 0x000a,    // <desktop>\Recycle Bin
			CSIDL_STARTMENU = 0x000b,    // <user name>\Start Menu
			CSIDL_MYDOCUMENTS = 0x000c,    // logical "My Documents" desktop icon
			CSIDL_MYMUSIC = 0x000d,    // "My Music" folder
			CSIDL_MYVIDEO = 0x000e,    // "My Videos" folder
			CSIDL_DESKTOPDIRECTORY = 0x0010,    // <user name>\Desktop
			CSIDL_DRIVES = 0x0011,    // My Computer
			CSIDL_NETWORK = 0x0012,    // Network Neighborhood (My Network Places)
			CSIDL_NETHOOD = 0x0013,    // <user name>\nethood
			CSIDL_FONTS = 0x0014,    // windows\fonts
			CSIDL_TEMPLATES = 0x0015,
			CSIDL_COMMON_STARTMENU = 0x0016,    // All Users\Start Menu
			CSIDL_COMMON_PROGRAMS = 0X0017,    // All Users\Start Menu\Programs
			CSIDL_COMMON_STARTUP = 0x0018,    // All Users\Startup
			CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019,    // All Users\Desktop
			CSIDL_APPDATA = 0x001a,    // <user name>\Application Data
			CSIDL_PRINTHOOD = 0x001b,    // <user name>\PrintHood

			CSIDL_LOCAL_APPDATA = 0x001c,    // <user name>\Local Settings\Applicaiton Data (non roaming)

			CSIDL_ALTSTARTUP = 0x001d,    // non localized startup
			CSIDL_COMMON_ALTSTARTUP = 0x001e,    // non localized common startup
			CSIDL_COMMON_FAVORITES = 0x001f,

			CSIDL_INTERNET_CACHE = 0x0020,
			CSIDL_COOKIES = 0x0021,
			CSIDL_HISTORY = 0x0022,
			CSIDL_COMMON_APPDATA = 0x0023,    // All Users\Application Data
			CSIDL_WINDOWS = 0x0024,    // GetWindowsDirectory()
			CSIDL_SYSTEM = 0x0025,    // GetSystemDirectory()
			CSIDL_PROGRAM_FILES = 0x0026,    // C:\Program Files
			CSIDL_MYPICTURES = 0x0027,    // C:\Program Files\My Pictures

			CSIDL_PROFILE = 0x0028,    // USERPROFILE
			CSIDL_SYSTEMX86 = 0x0029,    // x86 system directory on RISC
			CSIDL_PROGRAM_FILESX86 = 0x002a,    // x86 C:\Program Files on RISC

			CSIDL_PROGRAM_FILES_COMMON = 0x002b,    // C:\Program Files\Common

			CSIDL_PROGRAM_FILES_COMMONX86 = 0x002c,    // x86 Program Files\Common on RISC
			CSIDL_COMMON_TEMPLATES = 0x002d,    // All Users\Templates

			CSIDL_COMMON_DOCUMENTS = 0x002e,    // All Users\Documents
			CSIDL_COMMON_ADMINTOOLS = 0x002f,    // All Users\Start Menu\Programs\Administrative Tools
			CSIDL_ADMINTOOLS = 0x0030,    // <user name>\Start Menu\Programs\Administrative Tools

			CSIDL_CONNECTIONS = 0x0031,    // Network and Dial-up Connections
			CSIDL_COMMON_MUSIC = 0x0035,    // All Users\My Music
			CSIDL_COMMON_PICTURES = 0x0036,    // All Users\My Pictures
			CSIDL_COMMON_VIDEO = 0x0037,    // All Users\My Video

			CSIDL_CDBURN_AREA = 0x003b    // USERPROFILE\Local Settings\Application Data\Microsoft\CD Burning
		}

		/*
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHFILEINFO
		{
		  public IntPtr hIcon;
		  public int iIcon;
		  public uint dwAttributes;
		  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		  public string szDisplayName;
		  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		  public string szTypeName;
		}
		 */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHFILEINFO {
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}
		/// <summary>
		///  managed equivalent of IShellFolder interface
		///  Pinvoke.net / Mod by Arik Poznanski - pooya parsa
		///  Msdn:      http://msdn.microsoft.com/en-us/library/windows/desktop/bb775075(v=vs.85).aspx
		///  Pinvoke:   http://pinvoke.net/default.aspx/Interfaces/IShellFolder.html
		/// </summary>
		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214E6-0000-0000-C000-000000000046")]
		public interface IShellFolder {
			/// <summary>
			/// Translates a file object's or folder's display name into an item identifier list.
			/// Return value: error code, if any
			/// </summary>
			/// <param name="hwnd">Optional window handle</param>
			/// <param name="pbc">Optional bind context that controls the parsing operation. This parameter is normally set to NULL. </param>
			/// <param name="pszDisplayName">Null-terminated UNICODE string with the display name</param>
			/// <param name="pchEaten">Pointer to a ULONG value that receives the number of characters of the display name that was parsed.</param>
			/// <param name="ppidl"> Pointer to an ITEMIDLIST pointer that receives the item identifier list for the object.</param>
			/// <param name="pdwAttributes">Optional parameter that can be used to query for file attributes.this can be values from the SFGAO enum</param>
			//uint ParseDisplayName(IntPtr hwnd, IntPtr pbc, String pszDisplayName, out UInt32 pchEaten, out IntPtr ppidl, ref UInt32 pdwAttributes);
			uint ParseDisplayName(IntPtr hwnd, IntPtr pbc, String pszDisplayName, out UInt32 pchEaten, out IntPtr ppidl, ref SFGAOF pdwAttributes);


			/// <summary>
			///Allows a client to determine the contents of a folder by creating an item identifier enumeration object and returning its IEnumIDList interface. 
			///Return value: error code, if any
			/// </summary>
			/// <param name="hwnd">If user input is required to perform the enumeration, this window handle should be used by the enumeration object as the parent window to take user input.</param>
			/// <param name="grfFlags">Flags indicating which items to include in the  enumeration. For a list of possible values, see the SHCONTF enum. </param>
			/// <param name="ppenumIDList">Address that receives a pointer to the IEnumIDList interface of the enumeration object created by this method. </param>
			uint EnumObjects(IntPtr hwnd, SHCONTF grfFlags, out IEnumIDList ppenumIDList);
			//IEnumIDList EnumObjects(IntPtr hwnd, SHCONTF grfFlags); 

			/// <summary>
			///Retrieves an IShellFolder object for a subfolder.
			// Return value: error code, if any
			/// </summary>
			/// <param name="pidl">Address of an ITEMIDLIST structure (PIDL) that identifies the subfolder.</param>
			/// <param name="pbc">Optional address of an IBindCtx interface on a bind context object to be used during this operation.</param>
			/// <param name="riid">Identifier of the interface to return. </param>
			/// <param name="ppv">Address that receives the interface pointer.</param>
			uint BindToObject(IntPtr pidl, IntPtr pbc, [In]ref Guid riid, out IShellFolder ppv);

			/// <summary>
			/// Requests a pointer to an object's storage interface. 
			/// Return value: error code, if any
			/// </summary>
			/// <param name="pidl">Address of an ITEMIDLIST structure that identifies the subfolder relative to its parent folder. </param>
			/// <param name="pbc">Optional address of an IBindCtx interface on a bind context object to be  used during this operation.</param>
			/// <param name="riid">Interface identifier (IID) of the requested storage interface.</param>
			/// <param name="ppv"> Address that receives the interface pointer specified by riid.</param>
			uint BindToStorage(IntPtr pidl, IntPtr pbc, [In]ref Guid riid, out IntPtr ppv);

			/// <summary>
			/// Determines the relative order of two file objects or folders, given 
			/// their item identifier lists. Return value: If this method is 
			/// successful, the CODE field of the HRESULT contains one of the 
			/// following values (the code can be retrived using the helper function
			/// GetHResultCode): Negative A negative return value indicates that the first item should precede the second (pidl1 < pidl2). 
			//// 
			///Positive A positive return value indicates that the first item should
			///follow the second (pidl1 > pidl2).  Zero A return value of zero
			///indicates that the two items are the same (pidl1 = pidl2). 
			/// </summary>
			/// <param name="lParam">Value that specifies how the comparison  should be performed. The lower Sixteen bits of lParam define the sorting  rule. 
			///  The upper sixteen bits of lParam are used for flags that modify the sorting rule. values can be from  the SHCIDS enum
			/// </param>
			/// <param name="pidl1">Pointer to the first item's ITEMIDLIST structure.</param>
			/// <param name="pidl2"> Pointer to the second item's ITEMIDLIST structure.</param>
			/// <returns></returns>
			[PreserveSig]
			Int32 CompareIDs(Int32 lParam, IntPtr pidl1, IntPtr pidl2);

			/// <summary>
			/// Requests an object that can be used to obtain information from or interact
			/// with a folder object.
			/// Return value: error code, if any
			/// </summary>
			/// <param name="hwndOwner">Handle to the owner window.</param>
			/// <param name="riid">Identifier of the requested interface.</param>
			/// <param name="ppv">Address of a pointer to the requested interface. </param>
			uint CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);

			/// <summary>
			/// Retrieves the attributes of one or more file objects or subfolders. 
			/// Return value: error code, if any
			/// </summary>
			/// <param name="cidl">Number of file objects from which to retrieve attributes. </param>
			/// <param name="apidl">Address of an array of pointers to ITEMIDLIST structures, each of which  uniquely identifies a file object relative to the parent folder.</param>
			/// <param name="rgfInOut">Address of a single ULONG value that, on entry contains the attributes that the caller is 
			/// requesting. On exit, this value contains the requested attributes that are common to all of the specified objects. this value can be from the SFGAO enum
			/// </param>
			uint GetAttributesOf(UInt32 cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]IntPtr[] apidl, ref SFGAOF rgfInOut);

			/// <summary>
			/// Retrieves an OLE interface that can be used to carry out actions on the 
			/// specified file objects or folders. Return value: error code, if any
			/// </summary>
			/// <param name="hwndOwner">Handle to the owner window that the client should specify if it displays a dialog box or message box.</param>
			/// <param name="cidl">Number of file objects or subfolders specified in the apidl parameter. </param>
			/// <param name="apidl">Address of an array of pointers to ITEMIDLIST  structures, each of which  uniquely identifies a file object or subfolder relative to the parent folder.</param>
			/// <param name="riid">Identifier of the COM interface object to return.</param>
			/// <param name="rgfReserved"> Reserved. </param>
			/// <param name="ppv">Pointer to the requested interface.</param>
			uint GetUIObjectOf(IntPtr hwndOwner, UInt32 cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]IntPtr[] apidl, [In] ref Guid riid, UInt32 rgfReserved, out IntPtr ppv);

			/// <summary>
			/// Retrieves the display name for the specified file object or subfolder. 
			/// Return value: error code, if any
			/// </summary>
			/// <param name="pidl">Address of an ITEMIDLIST structure (PIDL)  that uniquely identifies the file  object or subfolder relative to the parent  folder. </param>
			/// <param name="uFlags">Flags used to request the type of display name to return. For a list of possible values. </param>
			/// <param name="pName"> Address of a STRRET structure in which to return the display name.</param>
			uint GetDisplayNameOf(IntPtr pidl, SHGDN uFlags, out STRRET pName);



			/// <summary>
			/// Sets the display name of a file object or subfolder, changing the item
			/// identifier in the process.
			/// Return value: error code, if any
			/// </summary>
			/// <param name="hwnd"> Handle to the owner window of any dialog or message boxes that the client displays.</param>
			/// <param name="pidl"> Pointer to an ITEMIDLIST structure that uniquely identifies the file object or subfolder relative to the parent folder. </param>
			/// <param name="pszName"> Pointer to a null-terminated string that specifies the new display name.</param>
			/// <param name="uFlags">Flags indicating the type of name specified by  the lpszName parameter. For a list of possible values, see the description of the SHGNO enum.</param>
			/// <param name="ppidlOut"></param>
			uint SetNameOf(IntPtr hwnd, IntPtr pidl, String pszName, SHGDN uFlags, out IntPtr ppidlOut);
		}


		[ComImport]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("000214F2-0000-0000-C000-000000000046")]
		public interface IEnumIDList {

			// Retrieves the specified number of item identifiers in the enumeration sequence and advances the current position by the number of items retrieved. 
			/*
			[PreserveSig()]
			uint Next(
				uint celt,                // Number of elements in the array pointed to by the rgelt parameter.
				out IntPtr rgelt,         // Address of an array of ITEMIDLIST pointers that receives the item identifiers. The implementation must allocate these item identifiers using the Shell's allocator (retrieved by the SHGetMalloc function). 
			  // The calling application is responsible for freeing the item identifiers using the Shell's allocator.
				out Int32 pceltFetched    // Address of a value that receives a count of the item identifiers actually returned in rgelt. The count can be smaller than the value specified in the celt parameter. This parameter can be NULL only if celt is one. 
				);
			*/
			[PreserveSig()]
			uint Next(
				uint celt,                // Number of elements in the array pointed to by the rgelt parameter.
				[In(), Out(), MarshalAs(UnmanagedType.LPArray)] IntPtr[] rgelt,         // Address of an array of ITEMIDLIST pointers that receives the item identifiers. The implementation must allocate these item identifiers using the Shell's allocator (retrieved by the SHGetMalloc function). 
																						// The calling application is responsible for freeing the item identifiers using the Shell's allocator.
				out Int32 pceltFetched    // Address of a value that receives a count of the item identifiers actually returned in rgelt. The count can be smaller than the value specified in the celt parameter. This parameter can be NULL only if celt is one. 
				);


			// Skips over the specified number of elements in the enumeration sequence. 
			[PreserveSig()]
			uint Skip(
				uint celt                 // Number of item identifiers to skip.
				);

			// Returns to the beginning of the enumeration sequence. 
			[PreserveSig()]
			uint Reset();

			// Creates a new item enumeration object with the same contents and state as the current one. 
			[PreserveSig()]
			uint Clone(
				out IEnumIDList ppenum    // Address of a pointer to the new enumeration object. The calling application must eventually free the new object by calling its Release member function. 
				);

			/*
			[PreserveSig()] 
			uint Next( 
			  uint celt, 
			  [In(), Out(), MarshalAs(UnmanagedType.LPArray)] IntPtr[] rgelt, 
			  out uint pceltFetched); 

			IEnumIDList Clone(); 
			*/
		}


		/// <summary>
		/// A standard OLE enumerator used by a client to determine the available search objects for a folder.
		/// </summary>
		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("0E700BE1-9DB6-11d1-A1CE-00C04FD75D13")]
		public interface IEnumExtraSearch {
			/// <summary>
			/// Used to request information on one or more search objects.
			/// </summary>
			/// <param name="celt">The number of search objects to be enumerated, starting from the current object. If celt is too large, the method should stop and return the actual number of search objects in pceltFetched.</param>
			/// <param name="rgelt">A pointer to an array of pceltFetched EXTRASEARCH structures containing information on the enumerated objects.</param>
			/// <param name="pceltFetched">The number of objects actually enumerated. This may be less than celt.</param>
			/// <returns>Returns S_OK if successful, or a COM-defined error code otherwise.</returns>
			[PreserveSig]
			int Next(uint celt, [MarshalAs(UnmanagedType.LPArray)] EXTRASEARCH[] rgelt, out uint pceltFetched);

			/// <summary>
			/// Skip a specified number of objects.
			/// </summary>
			/// <param name="celt">The number of objects to skip.</param>
			/// <returns>Returns S_OK if successful, or a COM-defined error code otherwise.</returns>
			[PreserveSig]
			int Skip(uint celt);

			/// <summary>
			/// Used to reset the enumeration index to zero.
			/// </summary>
			/// <returns>Returns S_OK if successful, or a COM-defined error code otherwise.</returns>
			[PreserveSig]
			int Reset();

			/// <summary>
			/// Used to request a duplicate of the enumerator object to preserve its current state.
			/// </summary>
			/// <param name="ppenum">A pointer to the IEnumExtraSearch interface of a new enumerator object.</param>
			/// <returns>Returns S_OK if successful, or a COM-defined error code otherwise.</returns>
			[PreserveSig]
			int Clone(out IEnumExtraSearch ppenum);
		}

		/// <summary>
		/// Used by an IEnumExtraSearch enumerator object to return information on the search objects supported by a Shell Folder object.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct EXTRASEARCH {
			/// <summary>
			/// A search object's GUID.
			/// </summary>
			public Guid guidSearch;

			/// <summary>
			/// A Unicode string containing the search object's friendly name. It will be used to identify the search engine on the Search Assistant menu.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string wszFriendlyName;

			/// <summary>
			/// The URL that will be displayed in the search pane.
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
			public string wszUrl;
		}

		// interface COM IImageList
		[ComImportAttribute()]
		[GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
		[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IImageList {
			[PreserveSig]
			int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

			[PreserveSig]
			int ReplaceIcon(int i, IntPtr hicon, ref int pi);

			[PreserveSig]
			int SetOverlayImage(int iImage, int iOverlay);

			[PreserveSig]
			int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

			[PreserveSig]
			int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

			[PreserveSig]
			int Draw(ref IMAGELISTDRAWPARAMS pimldp);

			[PreserveSig]
			int Remove(int i);

			[PreserveSig]
			int GetIcon(int i, int flags, ref IntPtr picon);
		};

		[StructLayout(LayoutKind.Sequential)]
		public struct IMAGELISTDRAWPARAMS {
			public int cbSize;
			public IntPtr himl;
			public int i;
			public IntPtr hdcDst;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int xBitmap;    // x offest from the upperleft of bitmap
			public int yBitmap;    // y offset from the upperleft of bitmap
			public int rgbBk;
			public int rgbFg;
			public int fStyle;
			public int dwRop;
			public int fState;
			public int Frame;
			public int crEffect;
		}


		public static class Icon {
			[DllImport("shell32.dll")]
			public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

			[DllImport("shell32.dll", EntryPoint = "#727")]
			public static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

			//[DllImport("shell32.dll", EntryPoint = "#727")]
			//public static extern int SHGetImageList(SHIL iImageList, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, ref IImageList ppv);

			[DllImport("shell32.dll", EntryPoint = "#727")]
			public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IImageList ppv);

			//
			[DllImport("shell32.dll", EntryPoint = "#727")]
			public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IntPtr ppv);

			[DllImport("comctl32.dll", SetLastError = true)]
			public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

			[DllImport("user32.dll")]
			public static extern bool DestroyIcon(IntPtr hIcon);

			[DllImport("comctl32.dll")]
			public static extern bool ImageList_Destroy(IntPtr himl);

			//SHFILEINFO
			[Flags]
			public enum SHGFI {
				SHGFI_ICON = 0x000000100,
				SHGFI_DISPLAYNAME = 0x000000200,
				SHGFI_TYPENAME = 0x000000400,
				SHGFI_ATTRIBUTES = 0x000000800,
				SHGFI_ICONLOCATION = 0x000001000,
				SHGFI_EXETYPE = 0x000002000,
				SHGFI_SYSICONINDEX = 0x000004000,
				SHGFI_LINKOVERLAY = 0x000008000,
				SHGFI_SELECTED = 0x000010000,
				SHGFI_ATTR_SPECIFIED = 0x000020000,
				SHGFI_LARGEICON = 0x000000000,
				SHGFI_SMALLICON = 0x000000001,
				SHGFI_OPENICON = 0x000000002,
				SHGFI_SHELLICONSIZE = 0x000000004,
				SHGFI_PIDL = 0x000000008,
				SHGFI_USEFILEATTRIBUTES = 0x000000010,
				SHGFI_ADDOVERLAYS = 0x000000020,
				SHGFI_OVERLAYINDEX = 0x000000040
			}

			public struct SHFILEINFO {
				public IntPtr hIcon;
				public int iIcon;
				public uint dwAttributes;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
				public string szDisplayName;
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
				public string szTypeName;
			}



			//IMAGE LIST
			public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
			public static Guid IID_IImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");

			[Flags]
			public enum SHIL {
				// 32
				SHIL_LARGE = 0x0,
				// 16
				SHIL_SMALL = 0x1,
				// 48
				SHIL_EXTRALARGE = 0x2,
				// 256
				SHIL_JUMBO = 0x4
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct POINT {
				public int x;
				public int y;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct RECT {
				public int left, top, right, bottom;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGEINFO {
				public IntPtr hbmImage;
				public IntPtr hbmMask;
				public int Unused1;
				public int Unused2;
				public RECT rcImage;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct IMAGELISTDRAWPARAMS {
				public int cbSize;
				public IntPtr himl;
				public int i;
				public IntPtr hdcDst;
				public int x;
				public int y;
				public int cx;
				public int cy;
				public int xBitmap;    // x offest from the upperleft of bitmap
				public int yBitmap;    // y offset from the upperleft of bitmap
				public int rgbBk;
				public int rgbFg;
				public int fStyle;
				public int dwRop;
				public int fState;
				public int Frame;
				public int crEffect;
			}

			[Flags]
			public enum ImageListDrawItemConstants : int {
				/// <summary>
				/// Draw item normally.
				/// </summary>
				ILD_NORMAL = 0x0,
				/// <summary>
				/// Draw item transparently.
				/// </summary>
				ILD_TRANSPARENT = 0x1,
				/// <summary>
				/// Draw item blended with 25% of the specified foreground colour
				/// or the Highlight colour if no foreground colour specified.
				/// </summary>
				ILD_BLEND25 = 0x2,
				/// <summary>
				/// Draw item blended with 50% of the specified foreground colour
				/// or the Highlight colour if no foreground colour specified.
				/// </summary>
				ILD_SELECTED = 0x4,
				/// <summary>
				/// Draw the icon's mask
				/// </summary>
				ILD_MASK = 0x10,
				/// <summary>
				/// Draw the icon image without using the mask
				/// </summary>
				ILD_IMAGE = 0x20,
				/// <summary>
				/// Draw the icon using the ROP specified.
				/// </summary>
				ILD_ROP = 0x40,
				/// <summary>
				/// ?
				/// </summary>
				ILD_OVERLAYMASK = 0xF00,
				/// <summary>
				/// Preserves the alpha channel in dest. XP only.
				/// </summary>
				ILD_PRESERVEALPHA = 0x1000, // 
											/// <summary>
											/// Scale the image to cx, cy instead of clipping it.  XP only.
											/// </summary>
				ILD_SCALE = 0x2000,
				/// <summary>
				/// Scale the image to the current DPI of the display. XP only.
				/// </summary>
				ILD_DPISCALE = 0x4000
			}


			// interface COM IImageList
			[ComImportAttribute()]
			[GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
			[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IImageList {
				[PreserveSig]
				int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

				[PreserveSig]
				int ReplaceIcon(int i, IntPtr hicon, ref int pi);

				[PreserveSig]
				int SetOverlayImage(int iImage, int iOverlay);

				[PreserveSig]
				int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

				[PreserveSig]
				int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

				[PreserveSig]
				int Draw(ref IMAGELISTDRAWPARAMS pimldp);

				[PreserveSig]
				int Remove(int i);

				[PreserveSig]
				int GetIcon(int i, int flags, ref IntPtr picon);
			};
		}
	}
}
