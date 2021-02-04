namespace desay
{
    partial class frmAAVision
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
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_ShowImgInfo = new System.Windows.Forms.CheckBox();
            this.hWindowControl1 = new HalconDotNet.HWindowControl();
            this.groupbox4 = new System.Windows.Forms.GroupBox();
            this.cboxLocal = new System.Windows.Forms.CheckBox();
            this.RightCheck = new System.Windows.Forms.Button();
            this.LeftCheck = new System.Windows.Forms.Button();
            this.LeftPos = new System.Windows.Forms.Button();
            this.RightPos = new System.Windows.Forms.Button();
            this.btnplcwrite = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numRight_Y = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numRight_X = new System.Windows.Forms.NumericUpDown();
            this.numLeft_Y = new System.Windows.Forms.NumericUpDown();
            this.numLeft_X = new System.Windows.Forms.NumericUpDown();
            this.btnsave = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numMarkspec = new System.Windows.Forms.NumericUpDown();
            this.numComspec = new System.Windows.Forms.NumericUpDown();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.机种选择ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hWindowControl2 = new HalconDotNet.HWindowControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.hWindowControl3 = new HalconDotNet.HWindowControl();
            this.hWindowControl4 = new HalconDotNet.HWindowControl();
            this.groupBox2.SuspendLayout();
            this.groupbox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRight_Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight_X)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft_Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft_X)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarkspec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numComspec)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(20, 87);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "一次获取";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(12, 328);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(137, 312);
            this.panel1.TabIndex = 12;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.cb_ShowImgInfo);
            this.groupBox2.Location = new System.Drawing.Point(12, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(137, 122);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "相机采集";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(22, 28);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 13;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "保存图片";
            // 
            // cb_ShowImgInfo
            // 
            this.cb_ShowImgInfo.AutoSize = true;
            this.cb_ShowImgInfo.Location = new System.Drawing.Point(22, 56);
            this.cb_ShowImgInfo.Name = "cb_ShowImgInfo";
            this.cb_ShowImgInfo.Size = new System.Drawing.Size(96, 16);
            this.cb_ShowImgInfo.TabIndex = 13;
            this.cb_ShowImgInfo.Text = "显示图像信息";
            this.cb_ShowImgInfo.UseVisualStyleBackColor = true;
            this.cb_ShowImgInfo.CheckedChanged += new System.EventHandler(this.cb_ShowImgInfo_CheckedChanged);
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.BackColor = System.Drawing.Color.Black;
            this.hWindowControl1.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.Location = new System.Drawing.Point(319, 33);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(400, 300);
            this.hWindowControl1.TabIndex = 18;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(400, 300);
            // 
            // groupbox4
            // 
            this.groupbox4.Controls.Add(this.cboxLocal);
            this.groupbox4.Controls.Add(this.RightCheck);
            this.groupbox4.Controls.Add(this.LeftCheck);
            this.groupbox4.Controls.Add(this.LeftPos);
            this.groupbox4.Controls.Add(this.RightPos);
            this.groupbox4.Location = new System.Drawing.Point(166, 33);
            this.groupbox4.Name = "groupbox4";
            this.groupbox4.Size = new System.Drawing.Size(137, 202);
            this.groupbox4.TabIndex = 20;
            this.groupbox4.TabStop = false;
            this.groupbox4.Text = "功能测试";
            // 
            // cboxLocal
            // 
            this.cboxLocal.AutoSize = true;
            this.cboxLocal.Location = new System.Drawing.Point(22, 29);
            this.cboxLocal.Name = "cboxLocal";
            this.cboxLocal.Size = new System.Drawing.Size(96, 16);
            this.cboxLocal.TabIndex = 4;
            this.cboxLocal.Text = "本地图片测试";
            this.cboxLocal.UseVisualStyleBackColor = true;
            // 
            // RightCheck
            // 
            this.RightCheck.Location = new System.Drawing.Point(21, 158);
            this.RightCheck.Name = "RightCheck";
            this.RightCheck.Size = new System.Drawing.Size(97, 23);
            this.RightCheck.TabIndex = 3;
            this.RightCheck.Text = "里工位检测";
            this.RightCheck.UseVisualStyleBackColor = true;
            this.RightCheck.Click += new System.EventHandler(this.RightCheck_Click);
            // 
            // LeftCheck
            // 
            this.LeftCheck.Location = new System.Drawing.Point(21, 92);
            this.LeftCheck.Name = "LeftCheck";
            this.LeftCheck.Size = new System.Drawing.Size(97, 23);
            this.LeftCheck.TabIndex = 2;
            this.LeftCheck.Text = "外工位检测";
            this.LeftCheck.UseVisualStyleBackColor = true;
            this.LeftCheck.Click += new System.EventHandler(this.LeftCheck_Click);
            // 
            // LeftPos
            // 
            this.LeftPos.Location = new System.Drawing.Point(21, 59);
            this.LeftPos.Name = "LeftPos";
            this.LeftPos.Size = new System.Drawing.Size(97, 23);
            this.LeftPos.TabIndex = 1;
            this.LeftPos.Text = "外工位定位";
            this.LeftPos.UseVisualStyleBackColor = true;
            this.LeftPos.Click += new System.EventHandler(this.LeftPos_Click);
            // 
            // RightPos
            // 
            this.RightPos.Location = new System.Drawing.Point(21, 125);
            this.RightPos.Name = "RightPos";
            this.RightPos.Size = new System.Drawing.Size(97, 23);
            this.RightPos.TabIndex = 0;
            this.RightPos.Text = "里工位定位";
            this.RightPos.UseVisualStyleBackColor = true;
            this.RightPos.Click += new System.EventHandler(this.RightPos_Click);
            // 
            // btnplcwrite
            // 
            this.btnplcwrite.Location = new System.Drawing.Point(17, 122);
            this.btnplcwrite.Name = "btnplcwrite";
            this.btnplcwrite.Size = new System.Drawing.Size(97, 23);
            this.btnplcwrite.TabIndex = 20;
            this.btnplcwrite.Text = "通讯测试";
            this.btnplcwrite.UseVisualStyleBackColor = true;
            this.btnplcwrite.Click += new System.EventHandler(this.btnplcwrite_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(21, 163);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 12);
            this.label8.TabIndex = 19;
            this.label8.Text = "里工位Y补偿";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 120);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "里工位X补偿";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 77);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "外工位Y补偿";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 35);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "外工位X补偿";
            // 
            // numRight_Y
            // 
            this.numRight_Y.Location = new System.Drawing.Point(21, 178);
            this.numRight_Y.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numRight_Y.Name = "numRight_Y";
            this.numRight_Y.Size = new System.Drawing.Size(97, 21);
            this.numRight_Y.TabIndex = 15;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(17, 42);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(97, 21);
            this.textBox1.TabIndex = 10;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "接收指令";
            // 
            // numRight_X
            // 
            this.numRight_X.Location = new System.Drawing.Point(21, 135);
            this.numRight_X.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numRight_X.Name = "numRight_X";
            this.numRight_X.Size = new System.Drawing.Size(97, 21);
            this.numRight_X.TabIndex = 14;
            // 
            // numLeft_Y
            // 
            this.numLeft_Y.Location = new System.Drawing.Point(21, 92);
            this.numLeft_Y.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numLeft_Y.Name = "numLeft_Y";
            this.numLeft_Y.Size = new System.Drawing.Size(97, 21);
            this.numLeft_Y.TabIndex = 13;
            // 
            // numLeft_X
            // 
            this.numLeft_X.Location = new System.Drawing.Point(21, 50);
            this.numLeft_X.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numLeft_X.Name = "numLeft_X";
            this.numLeft_X.Size = new System.Drawing.Size(96, 21);
            this.numLeft_X.TabIndex = 12;
            // 
            // btnsave
            // 
            this.btnsave.Location = new System.Drawing.Point(21, 356);
            this.btnsave.Name = "btnsave";
            this.btnsave.Size = new System.Drawing.Size(97, 23);
            this.btnsave.TabIndex = 9;
            this.btnsave.Text = "保存参数";
            this.btnsave.UseVisualStyleBackColor = true;
            this.btnsave.Click += new System.EventHandler(this.btnsave_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 249);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "接点偏差";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 206);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "胶宽偏差";
            // 
            // numMarkspec
            // 
            this.numMarkspec.Location = new System.Drawing.Point(21, 264);
            this.numMarkspec.Name = "numMarkspec";
            this.numMarkspec.Size = new System.Drawing.Size(96, 21);
            this.numMarkspec.TabIndex = 6;
            // 
            // numComspec
            // 
            this.numComspec.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.numComspec.Location = new System.Drawing.Point(21, 221);
            this.numComspec.Name = "numComspec";
            this.numComspec.Size = new System.Drawing.Size(97, 21);
            this.numComspec.TabIndex = 5;
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.机种选择ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1141, 25);
            this.menuStrip1.TabIndex = 21;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 机种选择ToolStripMenuItem
            // 
            this.机种选择ToolStripMenuItem.Name = "机种选择ToolStripMenuItem";
            this.机种选择ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.机种选择ToolStripMenuItem.Text = "机种选择";
            this.机种选择ToolStripMenuItem.Click += new System.EventHandler(this.机种选择ToolStripMenuItem_Click);
            // 
            // hWindowControl2
            // 
            this.hWindowControl2.BackColor = System.Drawing.Color.Black;
            this.hWindowControl2.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl2.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl2.Location = new System.Drawing.Point(319, 339);
            this.hWindowControl2.Name = "hWindowControl2";
            this.hWindowControl2.Size = new System.Drawing.Size(400, 300);
            this.hWindowControl2.TabIndex = 18;
            this.hWindowControl2.WindowSize = new System.Drawing.Size(400, 300);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnplcwrite);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(12, 161);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(137, 162);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PLC通讯";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 24;
            this.label9.Text = "发送指令";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(17, 85);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(97, 21);
            this.textBox2.TabIndex = 23;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnsave);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.numMarkspec);
            this.groupBox3.Controls.Add(this.numComspec);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.numLeft_X);
            this.groupBox3.Controls.Add(this.numLeft_Y);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.numRight_X);
            this.groupBox3.Controls.Add(this.numRight_Y);
            this.groupBox3.Location = new System.Drawing.Point(166, 246);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(137, 393);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "参数设置";
            // 
            // hWindowControl3
            // 
            this.hWindowControl3.BackColor = System.Drawing.Color.Black;
            this.hWindowControl3.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl3.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl3.Location = new System.Drawing.Point(725, 33);
            this.hWindowControl3.Name = "hWindowControl3";
            this.hWindowControl3.Size = new System.Drawing.Size(400, 300);
            this.hWindowControl3.TabIndex = 18;
            this.hWindowControl3.WindowSize = new System.Drawing.Size(400, 300);
            // 
            // hWindowControl4
            // 
            this.hWindowControl4.BackColor = System.Drawing.Color.Black;
            this.hWindowControl4.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl4.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl4.Location = new System.Drawing.Point(725, 339);
            this.hWindowControl4.Name = "hWindowControl4";
            this.hWindowControl4.Size = new System.Drawing.Size(400, 300);
            this.hWindowControl4.TabIndex = 18;
            this.hWindowControl4.WindowSize = new System.Drawing.Size(400, 300);
            // 
            // frmAAVision
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1141, 651);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupbox4);
            this.Controls.Add(this.hWindowControl4);
            this.Controls.Add(this.hWindowControl2);
            this.Controls.Add(this.hWindowControl3);
            this.Controls.Add(this.hWindowControl1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmAAVision";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "视觉模块";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmAAVision_FormClosing);
            this.Load += new System.EventHandler(this.frmAAVision_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupbox4.ResumeLayout(false);
            this.groupbox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRight_Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRight_X)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft_Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLeft_X)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMarkspec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numComspec)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        public HalconDotNet.HWindowControl hWindowControl1;
        private System.Windows.Forms.CheckBox cb_ShowImgInfo;
        private System.Windows.Forms.GroupBox groupbox4;
        private System.Windows.Forms.Button RightCheck;
        private System.Windows.Forms.Button LeftCheck;
        private System.Windows.Forms.Button LeftPos;
        private System.Windows.Forms.Button RightPos;
        private System.Windows.Forms.CheckBox cboxLocal;
        private System.Windows.Forms.Button btnsave;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numMarkspec;
        private System.Windows.Forms.NumericUpDown numComspec;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numRight_Y;
        private System.Windows.Forms.NumericUpDown numRight_X;
        private System.Windows.Forms.NumericUpDown numLeft_Y;
        private System.Windows.Forms.NumericUpDown numLeft_X;
        private System.Windows.Forms.Button btnplcwrite;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 机种选择ToolStripMenuItem;
        public HalconDotNet.HWindowControl hWindowControl2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        public HalconDotNet.HWindowControl hWindowControl3;
        public HalconDotNet.HWindowControl hWindowControl4;
    }
}

