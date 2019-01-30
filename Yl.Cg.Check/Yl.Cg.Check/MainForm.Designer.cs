namespace YL.Check
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblCheckInfo = new System.Windows.Forms.Label();
            this.picState = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picState)).BeginInit();
            this.SuspendLayout();
            // 
            // pnContainer
            // 
            this.pnContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnContainer.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnContainer.Location = new System.Drawing.Point(0, 0);
            this.pnContainer.Size = new System.Drawing.Size(1024, 768);
            // 
            // lblCheckInfo
            // 
            this.lblCheckInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCheckInfo.BackColor = System.Drawing.Color.Red;
            this.lblCheckInfo.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCheckInfo.ForeColor = System.Drawing.Color.White;
            this.lblCheckInfo.Location = new System.Drawing.Point(964, 718);
            this.lblCheckInfo.Margin = new System.Windows.Forms.Padding(0);
            this.lblCheckInfo.Name = "lblCheckInfo";
            this.lblCheckInfo.Size = new System.Drawing.Size(53, 43);
            this.lblCheckInfo.TabIndex = 0;
            this.lblCheckInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCheckInfo.Visible = false;
            // 
            // picState
            // 
            this.picState.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picState.Image = global::YL.Check.Properties.Resources.red;
            this.picState.Location = new System.Drawing.Point(971, 30);
            this.picState.Name = "picState";
            this.picState.Size = new System.Drawing.Size(20, 20);
            this.picState.TabIndex = 3;
            this.picState.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.picState);
            this.Controls.Add(this.lblCheckInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Text = "莱芜科技馆联网验票系统";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Controls.SetChildIndex(this.pnContainer, 0);
            this.Controls.SetChildIndex(this.lblCheckInfo, 0);
            this.Controls.SetChildIndex(this.picState, 0);
            ((System.ComponentModel.ISupportInitialize)(this.picState)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Label lblCheckInfo;
		private System.Windows.Forms.PictureBox picState;


	}
}

