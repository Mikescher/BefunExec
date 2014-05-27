namespace BefungExec.View
{
	partial class CaptureForm
	{
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaptureForm));
			this.nudCount = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.btnPath = new System.Windows.Forms.Button();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.edPath = new System.Windows.Forms.TextBox();
			this.cbxAuto = new System.Windows.Forms.CheckBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.nuDelay = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.nudFinalDelay = new System.Windows.Forms.NumericUpDown();
			this.cbxRepeat = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.nudCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nuDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudFinalDelay)).BeginInit();
			this.SuspendLayout();
			// 
			// nudCount
			// 
			this.nudCount.Location = new System.Drawing.Point(59, 89);
			this.nudCount.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.nudCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudCount.Name = "nudCount";
			this.nudCount.Size = new System.Drawing.Size(213, 20);
			this.nudCount.TabIndex = 0;
			this.nudCount.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 91);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Frames:";
			// 
			// btnPath
			// 
			this.btnPath.Location = new System.Drawing.Point(231, 12);
			this.btnPath.Name = "btnPath";
			this.btnPath.Size = new System.Drawing.Size(41, 23);
			this.btnPath.TabIndex = 2;
			this.btnPath.Text = "...";
			this.btnPath.UseVisualStyleBackColor = true;
			this.btnPath.Click += new System.EventHandler(this.btnPath_Click);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.DefaultExt = "gif";
			// 
			// edPath
			// 
			this.edPath.BackColor = System.Drawing.Color.White;
			this.edPath.Location = new System.Drawing.Point(12, 12);
			this.edPath.Name = "edPath";
			this.edPath.ReadOnly = true;
			this.edPath.Size = new System.Drawing.Size(213, 20);
			this.edPath.TabIndex = 3;
			// 
			// cbxAuto
			// 
			this.cbxAuto.AutoSize = true;
			this.cbxAuto.Location = new System.Drawing.Point(12, 59);
			this.cbxAuto.Name = "cbxAuto";
			this.cbxAuto.Size = new System.Drawing.Size(132, 17);
			this.cbxAuto.TabIndex = 4;
			this.cbxAuto.Text = "Automatic Framecount";
			this.cbxAuto.UseVisualStyleBackColor = true;
			// 
			// btnStart
			// 
			this.btnStart.Enabled = false;
			this.btnStart.Location = new System.Drawing.Point(197, 205);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 117);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(37, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Delay:";
			// 
			// nuDelay
			// 
			this.nuDelay.Location = new System.Drawing.Point(59, 115);
			this.nuDelay.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.nuDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nuDelay.Name = "nuDelay";
			this.nuDelay.Size = new System.Drawing.Size(213, 20);
			this.nuDelay.TabIndex = 6;
			this.nuDelay.Value = new decimal(new int[] {
            256,
            0,
            0,
            0});
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 143);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Final Delay:";
			// 
			// nudFinalDelay
			// 
			this.nudFinalDelay.Location = new System.Drawing.Point(77, 141);
			this.nudFinalDelay.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.nudFinalDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudFinalDelay.Name = "nudFinalDelay";
			this.nudFinalDelay.Size = new System.Drawing.Size(195, 20);
			this.nudFinalDelay.TabIndex = 8;
			this.nudFinalDelay.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			// 
			// cbxRepeat
			// 
			this.cbxRepeat.AutoSize = true;
			this.cbxRepeat.Checked = true;
			this.cbxRepeat.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbxRepeat.Location = new System.Drawing.Point(12, 170);
			this.cbxRepeat.Name = "cbxRepeat";
			this.cbxRepeat.Size = new System.Drawing.Size(81, 17);
			this.cbxRepeat.TabIndex = 10;
			this.cbxRepeat.Text = "Repeat GIF";
			this.cbxRepeat.UseVisualStyleBackColor = true;
			// 
			// CaptureForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 240);
			this.Controls.Add(this.cbxRepeat);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.nudFinalDelay);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.nuDelay);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.cbxAuto);
			this.Controls.Add(this.edPath);
			this.Controls.Add(this.btnPath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.nudCount);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "CaptureForm";
			this.Text = "Capture GIF";
			((System.ComponentModel.ISupportInitialize)(this.nudCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nuDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudFinalDelay)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.NumericUpDown nudCount;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnPath;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.TextBox edPath;
		private System.Windows.Forms.CheckBox cbxAuto;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown nuDelay;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown nudFinalDelay;
		private System.Windows.Forms.CheckBox cbxRepeat;
	}
}