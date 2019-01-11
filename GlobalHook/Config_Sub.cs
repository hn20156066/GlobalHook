using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GH {

	public partial class Config_Sub : Form {

		public Config_Sub(ref GHPadding srcPadding)
		{
			InitializeComponent();

			RefPadding = srcPadding;
		}

		private GHPadding RefPadding { get; set; }

		private void Config_Sub_Load(object sender, EventArgs e) {
			UpDownRight.Value = RefPadding.Right;
			UpDownTop.Value = RefPadding.Top;
			UpDownLeft.Value = RefPadding.Left;
			UpDownBottom.Value = RefPadding.Bottom;
		}

		private void UpDownUp_ValueChanged(object sender, EventArgs e) {
			RefPadding.Top = Config.UpDownCheckOfLimit(ref sender);
		}

		private void UpDownLeft_ValueChanged(object sender, EventArgs e) {
			RefPadding.Left = Config.UpDownCheckOfLimit(ref sender);
		}

		private void UpDownRight_ValueChanged(object sender, EventArgs e) {
			RefPadding.Right = Config.UpDownCheckOfLimit(ref sender);
		}

		private void UpDownBottom_ValueChanged(object sender, EventArgs e) {
			RefPadding.Bottom = Config.UpDownCheckOfLimit(ref sender);
		}

		private void ButtonOK_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
			Close();
		}

		private void ButtonCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}

	}
}
