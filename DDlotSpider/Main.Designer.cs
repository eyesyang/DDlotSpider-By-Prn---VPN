namespace DDlotSpider
{
    partial class Main
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LogTxtBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblVPNStatus = new System.Windows.Forms.Label();
            this.btnVPN = new System.Windows.Forms.Button();
            this.lblIP = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(12, 49);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1000, 449);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Url = new System.Uri("", System.UriKind.Relative);
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.LogTxtBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 504);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1006, 196);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log Output";
            // 
            // LogTxtBox
            // 
            this.LogTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTxtBox.Location = new System.Drawing.Point(6, 11);
            this.LogTxtBox.Name = "LogTxtBox";
            this.LogTxtBox.Size = new System.Drawing.Size(994, 179);
            this.LogTxtBox.TabIndex = 0;
            this.LogTxtBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(661, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "VPN Status: ";
            // 
            // lblVPNStatus
            // 
            this.lblVPNStatus.AutoSize = true;
            this.lblVPNStatus.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblVPNStatus.Location = new System.Drawing.Point(731, 13);
            this.lblVPNStatus.Name = "lblVPNStatus";
            this.lblVPNStatus.Size = new System.Drawing.Size(67, 12);
            this.lblVPNStatus.TabIndex = 3;
            this.lblVPNStatus.Text = "Disconnected";
            // 
            // btnVPN
            // 
            this.btnVPN.Location = new System.Drawing.Point(843, 8);
            this.btnVPN.Name = "btnVPN";
            this.btnVPN.Size = new System.Drawing.Size(75, 23);
            this.btnVPN.TabIndex = 4;
            this.btnVPN.Text = "Connect";
            this.btnVPN.UseVisualStyleBackColor = true;
            this.btnVPN.Click += new System.EventHandler(this.btnVPN_Click);
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(733, 29);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(9, 12);
            this.lblIP.TabIndex = 5;
            this.lblIP.Text = "-";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1030, 712);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.btnVPN);
            this.Controls.Add(this.lblVPNStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.webBrowser1);
            this.Name = "Main";
            this.Text = "IRIS_DDlot - ByPrn";
            this.Load += new System.EventHandler(this.Main_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.WebBrowser SearchSuggestWb;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox LogTxtBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVPNStatus;
        private System.Windows.Forms.Button btnVPN;
        private System.Windows.Forms.Label lblIP;
    }
}

