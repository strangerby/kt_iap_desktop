namespace IAP_Demo
{
    partial class CommModem
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommModem));
            this.lblBaudRate = new System.Windows.Forms.Label();
            this.cboBaudRate = new System.Windows.Forms.ComboBox();
            this.cboPorts = new System.Windows.Forms.ComboBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.lblPorts = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.tmrPortCheck = new System.Windows.Forms.Timer(this.components);
            this.btnWriteInfo = new System.Windows.Forms.Button();
            this.btnReadInfo = new System.Windows.Forms.Button();
            this.grpBoxUartConfigure = new System.Windows.Forms.GroupBox();
            this.grpBoxFlash = new System.Windows.Forms.GroupBox();
            this.btnBrowserFile = new System.Windows.Forms.Button();
            this.lblTransSchedule = new System.Windows.Forms.Label();
            this.prgBarTransSchedule = new System.Windows.Forms.ProgressBar();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.txtDataLength = new System.Windows.Forms.TextBox();
            this.txtDestAddress = new System.Windows.Forms.TextBox();
            this.lblDataLength = new System.Windows.Forms.Label();
            this.lblDownAddress = new System.Windows.Forms.Label();
            this.lblFileProgressBar = new System.Windows.Forms.Label();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.stsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stsPromptInfo = new System.Windows.Forms.StatusStrip();
            this.grpBoxUartConfigure.SuspendLayout();
            this.grpBoxFlash.SuspendLayout();
            this.stsPromptInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBaudRate
            // 
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBaudRate.Location = new System.Drawing.Point(251, 32);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new System.Drawing.Size(72, 16);
            this.lblBaudRate.TabIndex = 3;
            this.lblBaudRate.Text = "波特率：";
            // 
            // cboBaudRate
            // 
            this.cboBaudRate.BackColor = System.Drawing.SystemColors.Menu;
            this.cboBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBaudRate.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboBaudRate.FormattingEnabled = true;
            this.cboBaudRate.Location = new System.Drawing.Point(325, 27);
            this.cboBaudRate.Name = "cboBaudRate";
            this.cboBaudRate.Size = new System.Drawing.Size(140, 24);
            this.cboBaudRate.TabIndex = 4;
            // 
            // cboPorts
            // 
            this.cboPorts.BackColor = System.Drawing.SystemColors.Menu;
            this.cboPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPorts.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboPorts.FormattingEnabled = true;
            this.cboPorts.Location = new System.Drawing.Point(82, 27);
            this.cboPorts.Name = "cboPorts";
            this.cboPorts.Size = new System.Drawing.Size(140, 24);
            this.cboPorts.TabIndex = 4;
            this.cboPorts.DropDown += new System.EventHandler(this.cboPorts_DropDown);
            // 
            // btnOpen
            // 
            this.btnOpen.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpen.Location = new System.Drawing.Point(507, 26);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(90, 26);
            this.btnOpen.TabIndex = 5;
            this.btnOpen.Text = "连接";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // lblPorts
            // 
            this.lblPorts.AutoSize = true;
            this.lblPorts.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPorts.Location = new System.Drawing.Point(23, 30);
            this.lblPorts.Name = "lblPorts";
            this.lblPorts.Size = new System.Drawing.Size(56, 16);
            this.lblPorts.TabIndex = 3;
            this.lblPorts.Text = "端口：";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVersion.Location = new System.Drawing.Point(562, 244);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(88, 16);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "版本：V0.3";
            // 
            // tmrPortCheck
            // 
            this.tmrPortCheck.Tick += new System.EventHandler(this.tmrPortChackHandle);
            // 
            // btnWriteInfo
            // 
            this.btnWriteInfo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnWriteInfo.Location = new System.Drawing.Point(221, 26);
            this.btnWriteInfo.Name = "btnWriteInfo";
            this.btnWriteInfo.Size = new System.Drawing.Size(80, 25);
            this.btnWriteInfo.TabIndex = 5;
            this.btnWriteInfo.Text = "下载";
            this.btnWriteInfo.UseVisualStyleBackColor = true;
            this.btnWriteInfo.Click += new System.EventHandler(this.btnWriteInfo_Click);
            // 
            // btnReadInfo
            // 
            this.btnReadInfo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnReadInfo.Location = new System.Drawing.Point(533, 25);
            this.btnReadInfo.Name = "btnReadInfo";
            this.btnReadInfo.Size = new System.Drawing.Size(80, 25);
            this.btnReadInfo.TabIndex = 5;
            this.btnReadInfo.Text = "上传";
            this.btnReadInfo.UseVisualStyleBackColor = true;
            this.btnReadInfo.Visible = false;
            this.btnReadInfo.Click += new System.EventHandler(this.btnReadInfo_Click);
            // 
            // grpBoxUartConfigure
            // 
            this.grpBoxUartConfigure.Controls.Add(this.btnOpen);
            this.grpBoxUartConfigure.Controls.Add(this.lblPorts);
            this.grpBoxUartConfigure.Controls.Add(this.lblBaudRate);
            this.grpBoxUartConfigure.Controls.Add(this.cboBaudRate);
            this.grpBoxUartConfigure.Controls.Add(this.cboPorts);
            this.grpBoxUartConfigure.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.grpBoxUartConfigure.Location = new System.Drawing.Point(21, 11);
            this.grpBoxUartConfigure.Name = "grpBoxUartConfigure";
            this.grpBoxUartConfigure.Size = new System.Drawing.Size(629, 63);
            this.grpBoxUartConfigure.TabIndex = 10;
            this.grpBoxUartConfigure.TabStop = false;
            this.grpBoxUartConfigure.Text = "串口配置";
            // 
            // grpBoxFlash
            // 
            this.grpBoxFlash.Controls.Add(this.btnBrowserFile);
            this.grpBoxFlash.Controls.Add(this.lblTransSchedule);
            this.grpBoxFlash.Controls.Add(this.prgBarTransSchedule);
            this.grpBoxFlash.Controls.Add(this.btnReadInfo);
            this.grpBoxFlash.Controls.Add(this.txtFilePath);
            this.grpBoxFlash.Controls.Add(this.txtDataLength);
            this.grpBoxFlash.Controls.Add(this.txtDestAddress);
            this.grpBoxFlash.Controls.Add(this.btnWriteInfo);
            this.grpBoxFlash.Controls.Add(this.lblDataLength);
            this.grpBoxFlash.Controls.Add(this.lblDownAddress);
            this.grpBoxFlash.Controls.Add(this.lblFileProgressBar);
            this.grpBoxFlash.Controls.Add(this.lblFilePath);
            this.grpBoxFlash.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.grpBoxFlash.Location = new System.Drawing.Point(21, 91);
            this.grpBoxFlash.Name = "grpBoxFlash";
            this.grpBoxFlash.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grpBoxFlash.Size = new System.Drawing.Size(629, 141);
            this.grpBoxFlash.TabIndex = 11;
            this.grpBoxFlash.TabStop = false;
            this.grpBoxFlash.Text = "文件传输";
            // 
            // btnBrowserFile
            // 
            this.btnBrowserFile.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnBrowserFile.Image = global::IAP_Demo.Properties.Resources.folder;
            this.btnBrowserFile.Location = new System.Drawing.Point(481, 62);
            this.btnBrowserFile.Name = "btnBrowserFile";
            this.btnBrowserFile.Size = new System.Drawing.Size(40, 30);
            this.btnBrowserFile.TabIndex = 6;
            this.btnBrowserFile.UseVisualStyleBackColor = true;
            this.btnBrowserFile.Click += new System.EventHandler(this.btnBrowserFile_Click);
            // 
            // lblTransSchedule
            // 
            this.lblTransSchedule.AutoSize = true;
            this.lblTransSchedule.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTransSchedule.Location = new System.Drawing.Point(562, 109);
            this.lblTransSchedule.Name = "lblTransSchedule";
            this.lblTransSchedule.Size = new System.Drawing.Size(24, 16);
            this.lblTransSchedule.TabIndex = 4;
            this.lblTransSchedule.Text = "0%";
            // 
            // prgBarTransSchedule
            // 
            this.prgBarTransSchedule.Location = new System.Drawing.Point(99, 105);
            this.prgBarTransSchedule.Name = "prgBarTransSchedule";
            this.prgBarTransSchedule.Size = new System.Drawing.Size(440, 23);
            this.prgBarTransSchedule.TabIndex = 3;
            // 
            // txtFilePath
            // 
            this.txtFilePath.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtFilePath.Location = new System.Drawing.Point(99, 65);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(366, 26);
            this.txtFilePath.TabIndex = 2;
            // 
            // txtDataLength
            // 
            this.txtDataLength.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtDataLength.Location = new System.Drawing.Point(411, 26);
            this.txtDataLength.Name = "txtDataLength";
            this.txtDataLength.Size = new System.Drawing.Size(112, 26);
            this.txtDataLength.TabIndex = 2;
            this.txtDataLength.Visible = false;
            // 
            // txtDestAddress
            // 
            this.txtDestAddress.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtDestAddress.Location = new System.Drawing.Point(99, 26);
            this.txtDestAddress.Name = "txtDestAddress";
            this.txtDestAddress.Size = new System.Drawing.Size(112, 26);
            this.txtDestAddress.TabIndex = 2;
            // 
            // lblDataLength
            // 
            this.lblDataLength.AutoSize = true;
            this.lblDataLength.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDataLength.Location = new System.Drawing.Point(328, 30);
            this.lblDataLength.Name = "lblDataLength";
            this.lblDataLength.Size = new System.Drawing.Size(88, 16);
            this.lblDataLength.TabIndex = 1;
            this.lblDataLength.Text = "数据长度：";
            this.lblDataLength.Visible = false;
            // 
            // lblDownAddress
            // 
            this.lblDownAddress.AutoSize = true;
            this.lblDownAddress.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDownAddress.Location = new System.Drawing.Point(12, 30);
            this.lblDownAddress.Name = "lblDownAddress";
            this.lblDownAddress.Size = new System.Drawing.Size(88, 16);
            this.lblDownAddress.TabIndex = 1;
            this.lblDownAddress.Text = "目标地址：";
            // 
            // lblFileProgressBar
            // 
            this.lblFileProgressBar.AutoSize = true;
            this.lblFileProgressBar.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFileProgressBar.Location = new System.Drawing.Point(12, 108);
            this.lblFileProgressBar.Name = "lblFileProgressBar";
            this.lblFileProgressBar.Size = new System.Drawing.Size(88, 16);
            this.lblFileProgressBar.TabIndex = 1;
            this.lblFileProgressBar.Text = "传输进度：";
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFilePath.Location = new System.Drawing.Point(12, 69);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(88, 16);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "文件路径：";
            // 
            // stsLabel
            // 
            this.stsLabel.Name = "stsLabel";
            this.stsLabel.Size = new System.Drawing.Size(55, 17);
            this.stsLabel.Text = "串口关闭";
            // 
            // stsPromptInfo
            // 
            this.stsPromptInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stsLabel});
            this.stsPromptInfo.Location = new System.Drawing.Point(0, 269);
            this.stsPromptInfo.Name = "stsPromptInfo";
            this.stsPromptInfo.Size = new System.Drawing.Size(675, 22);
            this.stsPromptInfo.TabIndex = 9;
            // 
            // CommModem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 291);
            this.Controls.Add(this.grpBoxFlash);
            this.Controls.Add(this.grpBoxUartConfigure);
            this.Controls.Add(this.stsPromptInfo);
            this.Controls.Add(this.lblVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "CommModem";
            this.Text = "IAP_Demo";
            this.Load += new System.EventHandler(this.CommModem_Load);
            this.grpBoxUartConfigure.ResumeLayout(false);
            this.grpBoxUartConfigure.PerformLayout();
            this.grpBoxFlash.ResumeLayout(false);
            this.grpBoxFlash.PerformLayout();
            this.stsPromptInfo.ResumeLayout(false);
            this.stsPromptInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBaudRate;
        private System.Windows.Forms.ComboBox cboBaudRate;
        private System.Windows.Forms.ComboBox cboPorts;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label lblPorts;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Timer tmrPortCheck;
        private System.Windows.Forms.Button btnWriteInfo;
        private System.Windows.Forms.Button btnReadInfo;
        private System.Windows.Forms.GroupBox grpBoxUartConfigure;
        private System.Windows.Forms.GroupBox grpBoxFlash;
        private System.Windows.Forms.Label lblDownAddress;
        private System.Windows.Forms.Label lblFileProgressBar;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.TextBox txtDestAddress;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.ProgressBar prgBarTransSchedule;
        private System.Windows.Forms.Label lblTransSchedule;
        private System.Windows.Forms.TextBox txtDataLength;
        private System.Windows.Forms.Label lblDataLength;
        private System.Windows.Forms.Button btnBrowserFile;
        private System.Windows.Forms.ToolStripStatusLabel stsLabel;
        private System.Windows.Forms.StatusStrip stsPromptInfo;
    }
}

