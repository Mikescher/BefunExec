namespace BefunExec.View
{
	partial class AboutForm
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
			this.btnOK = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.linkLabel2 = new System.Windows.Forms.LinkLabel();
			this.redLicense = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.linkLabel3 = new System.Windows.Forms.LinkLabel();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(227, 291);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(194, 29);
			this.label1.TabIndex = 1;
			this.label1.Text = "BefunExec 1.0";
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkLabel1.Location = new System.Drawing.Point(12, 275);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(67, 20);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "OpenTK";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// linkLabel2
			// 
			this.linkLabel2.AutoSize = true;
			this.linkLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkLabel2.Location = new System.Drawing.Point(12, 297);
			this.linkLabel2.Name = "linkLabel2";
			this.linkLabel2.Size = new System.Drawing.Size(82, 20);
			this.linkLabel2.TabIndex = 3;
			this.linkLabel2.TabStop = true;
			this.linkLabel2.Text = "QuickFont";
			this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
			// 
			// redLicense
			// 
			this.redLicense.BackColor = System.Drawing.Color.White;
			this.redLicense.Location = new System.Drawing.Point(17, 69);
			this.redLicense.Name = "redLicense";
			this.redLicense.ReadOnly = true;
			this.redLicense.Size = new System.Drawing.Size(285, 203);
			this.redLicense.TabIndex = 4;
			this.redLicense.Text = "";
			this.redLicense.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.redLicense_LinkClicked);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(17, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(92, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "by Mike Schwörer";
			// 
			// linkLabel3
			// 
			this.linkLabel3.AutoSize = true;
			this.linkLabel3.Location = new System.Drawing.Point(116, 41);
			this.linkLabel3.Name = "linkLabel3";
			this.linkLabel3.Size = new System.Drawing.Size(103, 13);
			this.linkLabel3.TabIndex = 6;
			this.linkLabel3.TabStop = true;
			this.linkLabel3.Text = "(www.mikescher.de)";
			this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(256, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "(c) 2014";
			// 
			// AboutForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 326);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.linkLabel3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.redLicense);
			this.Controls.Add(this.linkLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About BefunExec";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.LinkLabel linkLabel2;
		private System.Windows.Forms.RichTextBox redLicense;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel3;
		private System.Windows.Forms.Label label3;
	}
}