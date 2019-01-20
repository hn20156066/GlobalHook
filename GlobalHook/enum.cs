using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH {

	/// <summary>
	/// スキンのファイル別のファイル名
	/// </summary>
	public enum SkinImage {
		/// <summary>
		/// マイセットのアイコン
		/// </summary>
		Myset_Icon,

		/// <summary>
		/// マイセットを開いた時のアイコン
		/// </summary>
		Myset_Open_Icon,

		/// <summary>
		/// ランチャーの背景
		/// </summary>
		Launcher_Background,

		/// <summary>
		/// ランチャーのアイテム (透明)
		/// </summary>
		Launcher_Item,

		/// <summary>
		/// ランチャーのアイテムの背景
		/// </summary>
		Launcher_Item_Background,

		Launcher_Item_Background_Push,

		/// <summary>
		/// アイテムリストの背景
		/// </summary>
		Group_Background,

		/// <summary>
		/// アイテムリストのアイコン
		/// </summary>
		Group_Item,

		/// <summary>
		/// アイテムリストのアイテムの背景
		/// </summary>
		Group_Item_Background,

		Group_Item_Background_Push,

		/// <summary>
		/// マイセットの背景
		/// </summary>
		Myset_Background,

		/// <summary>
		/// マイセットのアイテム
		/// </summary>
		Myset_Item,

		/// <summary>
		/// マイセットのアイテムの背景
		/// </summary>
		Myset_Item_Background,

		Myset_Item_Background_Push,

		/// <summary>
		/// NULL
		/// </summary>
		Kind_Null
	};

	/// <summary>
	/// ウィンドウの種類
	/// </summary>
	public enum FormType {
		/// <summary>
		/// ランチャー
		/// </summary>
		Launcher,

		/// <summary>
		/// マイセットリスト
		/// </summary>
		MysetList,

		/// <summary>
		/// アイテムリスト
		/// </summary>
		ItemList,

		None
	};

}
