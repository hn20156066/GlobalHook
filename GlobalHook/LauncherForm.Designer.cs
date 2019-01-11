namespace GH
{
    partial class LauncherForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.DrawTimer = new System.Windows.Forms.Timer(this.components);
			this.notifyIconMenu = new System.Windows.Forms.ContextMenu();
			this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// DrawTimer
			// 
			this.DrawTimer.Enabled = false;
			this.DrawTimer.Interval = 10;
			this.DrawTimer.Tick += new System.EventHandler(this.DrawTimer_Tick);
			// 
			// UpdateTimer
			// 
			this.UpdateTimer.Enabled = false;
			this.UpdateTimer.Interval = 500;
			this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
			// 
			// Launcher
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Name = "Launcher";
			this.Text = "Launcher";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Launcher_FormClosing);
			this.Load += new System.EventHandler(this.Launcher_Load);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Launcher_MouseUp);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Launcher_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Launcher_MouseMove);
			this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Timer DrawTimer;
		private System.Windows.Forms.Timer UpdateTimer;
		private System.Windows.Forms.ContextMenu notifyIconMenu;
	}
}

