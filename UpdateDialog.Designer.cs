namespace SharpUpdater {
    partial class UpdateDialog {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateDialog));
            this.UpdateButton = new System.Windows.Forms.Button();
            this.InfoTextBox = new System.Windows.Forms.RichTextBox();
            this.AProgressBar = new System.Windows.Forms.ProgressBar();
            this.LoadingPic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).BeginInit();
            this.SuspendLayout();
            // 
            // UpdateButton
            // 
            this.UpdateButton.Enabled = false;
            this.UpdateButton.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UpdateButton.Location = new System.Drawing.Point(25, 432);
            this.UpdateButton.Margin = new System.Windows.Forms.Padding(16);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(724, 72);
            this.UpdateButton.TabIndex = 0;
            this.UpdateButton.TabStop = false;
            this.UpdateButton.Text = "开始更新";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // InfoTextBox
            // 
            this.InfoTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.InfoTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.InfoTextBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.InfoTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.InfoTextBox.Location = new System.Drawing.Point(25, 25);
            this.InfoTextBox.Margin = new System.Windows.Forms.Padding(16);
            this.InfoTextBox.Name = "InfoTextBox";
            this.InfoTextBox.ReadOnly = true;
            this.InfoTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.InfoTextBox.Size = new System.Drawing.Size(724, 329);
            this.InfoTextBox.TabIndex = 2;
            this.InfoTextBox.TabStop = false;
            this.InfoTextBox.Text = "";
            this.InfoTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.InfoTextBox_LinkClicked);
            // 
            // AProgressBar
            // 
            this.AProgressBar.Location = new System.Drawing.Point(25, 373);
            this.AProgressBar.Name = "AProgressBar";
            this.AProgressBar.Size = new System.Drawing.Size(724, 40);
            this.AProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.AProgressBar.TabIndex = 3;
            this.AProgressBar.Visible = false;
            // 
            // LoadingPic
            // 
            this.LoadingPic.Image = ((System.Drawing.Image)(resources.GetObject("LoadingPic.Image")));
            this.LoadingPic.Location = new System.Drawing.Point(339, 254);
            this.LoadingPic.Margin = new System.Windows.Forms.Padding(16);
            this.LoadingPic.Name = "LoadingPic";
            this.LoadingPic.Size = new System.Drawing.Size(100, 100);
            this.LoadingPic.TabIndex = 4;
            this.LoadingPic.TabStop = false;
            this.LoadingPic.Visible = false;
            // 
            // UpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 529);
            this.Controls.Add(this.LoadingPic);
            this.Controls.Add(this.AProgressBar);
            this.Controls.Add(this.InfoTextBox);
            this.Controls.Add(this.UpdateButton);
            this.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "应用更新";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateDialog_FormClosing);
            this.Load += new System.EventHandler(this.UpdateDialog_Load);
            this.Shown += new System.EventHandler(this.UpdateDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.RichTextBox InfoTextBox;
        private System.Windows.Forms.ProgressBar AProgressBar;
        private System.Windows.Forms.PictureBox LoadingPic;
    }
}

