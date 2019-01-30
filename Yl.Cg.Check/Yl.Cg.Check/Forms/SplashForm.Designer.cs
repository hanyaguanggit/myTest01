namespace YL.Check.Forms
{
    partial class SplashForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.pb = new System.Windows.Forms.ProgressBar();
            this.lbVersion = new System.Windows.Forms.Label();
            this.tpnlSplashForm = new System.Windows.Forms.TableLayoutPanel();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnContainer.SuspendLayout();
            this.tpnlSplashForm.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnContainer
            // 
            this.pnContainer.Controls.Add(this.tpnlSplashForm);
            this.pnContainer.Controls.Add(this.pb);
            this.pnContainer.Location = new System.Drawing.Point(0, 0);
            this.pnContainer.Size = new System.Drawing.Size(1024, 768);
            // 
            // pb
            // 
            this.pb.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pb.Location = new System.Drawing.Point(0, 755);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(1024, 13);
            this.pb.TabIndex = 2;
            // 
            // lbVersion
            // 
            this.lbVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbVersion.BackColor = System.Drawing.Color.Transparent;
            this.lbVersion.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbVersion.ForeColor = System.Drawing.Color.DimGray;
            this.lbVersion.Location = new System.Drawing.Point(899, 720);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(122, 35);
            this.lbVersion.TabIndex = 11;
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tpnlSplashForm
            // 
            this.tpnlSplashForm.BackColor = System.Drawing.Color.Transparent;
            this.tpnlSplashForm.BackgroundImage = global::YL.Check.Properties.Resources.backgroud;
            this.tpnlSplashForm.ColumnCount = 1;
            this.tpnlSplashForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tpnlSplashForm.Controls.Add(this.lbVersion, 0, 2);
            this.tpnlSplashForm.Controls.Add(this.pnlInfo, 0, 1);
            this.tpnlSplashForm.Controls.Add(this.pnlTop, 0, 0);
            this.tpnlSplashForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tpnlSplashForm.Location = new System.Drawing.Point(0, 0);
            this.tpnlSplashForm.Name = "tpnlSplashForm";
            this.tpnlSplashForm.RowCount = 3;
            this.tpnlSplashForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tpnlSplashForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 422F));
            this.tpnlSplashForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tpnlSplashForm.Size = new System.Drawing.Size(1024, 755);
            this.tpnlSplashForm.TabIndex = 12;
            // 
            // pnlInfo
            // 
            this.pnlInfo.AutoScroll = true;
            this.pnlInfo.BackColor = System.Drawing.Color.Transparent;
            this.pnlInfo.Controls.Add(this.lblInfo);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInfo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pnlInfo.Location = new System.Drawing.Point(0, 298);
            this.pnlInfo.Margin = new System.Windows.Forms.Padding(0);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInfo.Size = new System.Drawing.Size(1024, 422);
            this.pnlInfo.TabIndex = 12;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblInfo.Location = new System.Drawing.Point(382, 24);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 20);
            this.lblInfo.TabIndex = 0;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Transparent;
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1024, 298);
            this.pnlTop.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(365, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(282, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "验票程序正在启动，请稍候...";
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SplashForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Text = "中国科技馆联网验票系统";
            this.Load += new System.EventHandler(this.SplashForm_Load);
            this.pnContainer.ResumeLayout(false);
            this.tpnlSplashForm.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.ProgressBar pb;
        private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.TableLayoutPanel tpnlSplashForm;
		private System.Windows.Forms.Panel pnlInfo;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.Label label1;
    }
}