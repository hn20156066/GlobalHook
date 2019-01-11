using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GH {

	public class MysetXml {
		
		public class MysetItemInfo {
			public string Path { get; set; }

			public MysetItemInfo() {
				Path = "";
			}

			public MysetItemInfo(string path) {
				Path = path;
			}

			public MysetItemInfo(MysetItemInfo info) {
				Path = info.Path;
			}
		}

		public class MysetInfo {
			public SerializableList<MysetItemInfo> MysetItems;

			public MysetInfo() {
				MysetItems = new SerializableList<MysetItemInfo>();
			}

			public MysetInfo(List<MysetItemInfo> infos) {
				MysetItems = new SerializableList<MysetItemInfo>();
				MysetItems.AddRange(infos);
			}

			public MysetInfo(MysetInfo info) {
				MysetItems = new SerializableList<MysetItemInfo>(info.MysetItems);
			}

		}

		public SerializableList<MysetInfo> Mysets;

		public MysetXml() {
			Mysets = new SerializableList<MysetInfo>();
		}

		public MysetXml(MysetXml xml) {
			Mysets = new SerializableList<MysetInfo>(xml.Mysets);
		}

		public void MysetClear() {
			Mysets.Clear();
			Mysets = null;
			Mysets = new SerializableList<MysetInfo>();
		}

		public void AddMyset(List<MysetItemInfo> infos) {
			Mysets.Add(new MysetInfo(infos));
		}

	}
}
