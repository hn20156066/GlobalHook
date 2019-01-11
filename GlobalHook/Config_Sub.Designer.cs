namespace GH {
	partial class Config_Sub {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UpDownLeft = new System.Windows.Forms.NumericUpDown();
			this.LabelLeft = new System.Windows.Forms.Label();
			this.LabelBottom = new System.Windows.Forms.Label();
			this.UpDownBottom = new System.Windows.Forms.NumericUpDown();
			this.LabelTop = new System.Windows.Forms.Label();
			this.UpDownTop = new System.Windows.Forms.NumericUpDown();
			this.LabelRight = new System.Windows.Forms.Label();
			this.UpDownRight = new System.Windows.Forms.NumericUpDown();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.UpDownLeft)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownBottom)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownTop)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownRight)).BeginInit();
			this.SuspendLayout();
			// 
			// UpDownLeft
			// 
			this.UpDownLeft.Location = new System.Drawing.Point(10, 69);
			this.UpDownLeft.Margin = new System.Windows.Forms.Padding(1);
			this.UpDownLeft.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.UpDownLeft.Name = "UpDownLeft";
			this.UpDownLeft.Size = new System.Drawing.Size(50, 23);
			this.UpDownLeft.TabIndex = 2;
			this.UpDownLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UpDownLeft.ValueChanged += new System.EventHandler(this.UpDownLeft_ValueChanged);
			// 
			// LabelLeft
			// 
			this.LabelLeft.Location = new System.Drawing.Point(10, 44);
			this.LabelLeft.Margin = new System.Windows.Forms.Padding(1);
			this.LabelLeft.Name = "LabelLeft";
			this.LabelLeft.Size = new System.Drawing.Size(50, 23);
			this.LabelLeft.TabIndex = 3;
			this.LabelLeft.Text = "左(&L)";
			this.LabelLeft.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LabelBottom
			// 
			this.LabelBottom.Location = new System.Drawing.Point(90, 80);
			this.LabelBottom.Margin = new System.Windows.Forms.Padding(1);
			this.LabelBottom.Name = "LabelBottom";
			this.LabelBottom.Size = new System.Drawing.Size(50, 23);
			this.LabelBottom.TabIndex = 5;
			this.LabelBottom.Text = "下(&B)";
			this.LabelBottom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UpDownBottom
			// 
			this.UpDownBottom.Location = new System.Drawing.Point(90, 105);
			this.UpDownBottom.Margin = new System.Windows.Forms.Padding(1);
			this.UpDownBottom.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.UpDownBottom.Name = "UpDownBottom";
			this.UpDownBottom.Size = new System.Drawing.Size(50, 23);
			this.UpDownBottom.TabIndex = 4;
			this.UpDownBottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UpDownBottom.ValueChanged += new System.EventHandler(this.UpDownBottom_ValueChanged);
			// 
			// LabelTop
			// 
			this.LabelTop.Location = new System.Drawing.Point(90, 10);
			this.LabelTop.Margin = new System.Windows.Forms.Padding(1);
			this.LabelTop.Name = "LabelTop";
			this.LabelTop.Size = new System.Drawing.Size(50, 23);
			this.LabelTop.TabIndex = 7;
			this.LabelTop.Text = "上(&U)";
			this.LabelTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UpDownTop
			// 
			this.UpDownTop.Location = new System.Drawing.Point(90, 35);
			this.UpDownTop.Margin = new System.Windows.Forms.Padding(1);
			this.UpDownTop.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.UpDownTop.Name = "UpDownTop";
			this.UpDownTop.Size = new System.Drawing.Size(50, 23);
			this.UpDownTop.TabIndex = 6;
			this.UpDownTop.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UpDownTop.ValueChanged += new System.EventHandler(this.UpDownUp_ValueChanged);
			// 
			// LabelRight
			// 
			this.LabelRight.Location = new System.Drawing.Point(162, 44);
			this.LabelRight.Margin = new System.Windows.Forms.Padding(1);
			this.LabelRight.Name = "LabelRight";
			this.LabelRight.Size = new System.Drawing.Size(50, 23);
			this.LabelRight.TabIndex = 9;
			this.LabelRight.Text = "右(&R)";
			this.LabelRight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UpDownRight
			// 
			this.UpDownRight.Location = new System.Drawing.Point(162, 69);
			this.UpDownRight.Margin = new System.Windows.Forms.Padding(1);
			this.UpDownRight.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.UpDownRight.Name = "UpDownRight";
			this.UpDownRight.Size = new System.Drawing.Size(50, 23);
			this.UpDownRight.TabIndex = 8;
			this.UpDownRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UpDownRight.ValueChanged += new System.EventHandler(this.UpDownRight_ValueChanged);
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ButtonOK.Location = new System.Drawing.Point(46, 145);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(85, 25);
			this.ButtonOK.TabIndex = 100;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.ButtonCancel.Location = new System.Drawing.Point(137, 145);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(85, 25);
			this.ButtonCancel.TabIndex = 101;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// Config_Sub
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(234, 182);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.LabelRight);
			this.Controls.Add(this.UpDownRight);
			this.Controls.Add(this.LabelTop);
			this.Controls.Add(this.UpDownTop);
			this.Controls.Add(this.LabelBottom);
			this.Controls.Add(this.UpDownBottom);
			this.Controls.Add(this.LabelLeft);
			this.Controls.Add(this.UpDownLeft);
			this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Config_Sub";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "設定";
			this.Load += new System.EventHandler(this.Config_Sub_Load);
			((System.ComponentModel.ISupportInitialize)(this.UpDownLeft)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownBottom)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownTop)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UpDownRight)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.NumericUpDown UpDownLeft;
		private System.Windows.Forms.Label LabelLeft;
		private System.Windows.Forms.Label LabelBottom;
		private System.Windows.Forms.NumericUpDown UpDownBottom;
		private System.Windows.Forms.Label LabelTop;
		private System.Windows.Forms.NumericUpDown UpDownTop;
		private System.Windows.Forms.Label LabelRight;
		private System.Windows.Forms.NumericUpDown UpDownRight;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
	}
}