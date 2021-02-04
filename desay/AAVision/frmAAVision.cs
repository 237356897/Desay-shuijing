using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using desay.AAVision;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HalconDotNet;
using desay.AAVision.Algorithm;
using System.IO;
using NationalInstruments.Vision;
using Vision_Assistant;
using TwinCAT.Ads;
using System.Collections;
using log4net;
using desay.ProductData;
using System.Toolkit.Helpers;

namespace desay
{
    public partial class frmAAVision : Form
    {
        private string _filePath = @"./Image";
        //该模块为独立模块，暂不与主线程代码融合（但功能已集成）,该模块可以单独调试（在program中启动）
        public ILog log1 = LogManager.GetLogger(typeof(frmAAVision));

        public static bool GetBmp = false;
        public static frmAAVision acq;
        public VisionImage LocalVI;
        private BaumerCamera _Subject;
        public AcqToolEdit edit;
        public bool SaveImage = true;

        Bitmap InputImage;
        HObject ho_Image = null;//原图像
        HTuple hv_Width;
        HTuple hv_Height;
        private string str_imgSize;
        public bool bShowImagePoint = false;

        public static Point LeftOffset;
        public static Point RightOffset;
        public static string MachineType;

        private string AMSNETID = "127.0.0.1.1.1";//目标的ID
        private int PORT = 851;//目标的端口号
        public static TcAdsClient adsClient;//ads客户端
        public static int ReceiceHandle;//main.str变量的句柄
        public static int SendHandle;//main.str变量的句柄
        public static string Sendtemp;


        public frmAAVision()
        {
            InitializeComponent();
            MachineType = Config.Instance.CurrentProductType;
            HOperatorSet.GenEmptyObj(out ho_Image);
            acq = this;
            _Subject = new BaumerCamera();
            DeleteFiles();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Acquire();
            try
            {
                //ho_Image.Dispose();
                Bitmap2HObject.Bitmap2HObj(ImageFactory.CreateBitmap(_Subject.OutputImageData), out ho_Image);
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                str_imgSize = string.Format("{0}X{1}", hv_Width, hv_Height);
                HOperatorSet.SetPart(hWindowControl1.HalconWindow, 0, 0, hv_Height - 1, hv_Width - 1);
                HOperatorSet.DispObj(ho_Image, hWindowControl1.HalconWindow);
            }
            catch { MessageBox.Show("无图像"); }
        }

        private void Acquire()
        {
            if (_Subject == null)
            {
                return;
            }
            _Subject.Acquire();
            if (SaveImage)
            {
                try
                {
                    ho_Image.Dispose();
                    Bitmap2HObject.Bitmap2HObj(ImageFactory.CreateBitmap(_Subject.OutputImageData), out ho_Image);
                    if (!Directory.Exists(_filePath))
                    { Directory.CreateDirectory(_filePath); }
                    //SaveImg($"{_filePath }//{ DateTime.Now.ToString("yy_MM_dd_HH_mm_ss")}.bmp");
                    SaveImg_JPG($"{_filePath }//{ DateTime.Now.ToString("yy_MM_dd_HH_mm_ss_fff")}.jpg");
                }
                catch {; }
            }

        }

        public void frmAAVision_Load(object sender, EventArgs e)
        {
            try
            {
                numComspec.DecimalPlaces = 3;
                numComspec.Increment = 0.001M;
                numMarkspec.DecimalPlaces = 3;
                numMarkspec.Increment = 0.001M;

                numComspec.Value = (decimal)Position.Instance.GlueComspec;
                numMarkspec.Value = (decimal)Position.Instance.GlueMarkspec;
                numLeft_X.Value = (decimal)Position.Instance.LeftPos_Offset_X;
                numLeft_Y.Value = (decimal)Position.Instance.LeftPos_Offset_Y;
                numRight_X.Value = (decimal)Position.Instance.RightPos_Offset_X;
                numRight_Y.Value = (decimal)Position.Instance.RightPos_Offset_Y;

                LeftOffset = new Point((int)numLeft_X.Value, (int)numLeft_Y.Value);
                RightOffset = new Point((int)numRight_X.Value, (int)numRight_Y.Value);


                //直接创建避免影响自动化流程中调用
                edit = new AcqToolEdit();
                edit.Subject = _Subject;
                this.panel1.Controls.Add(edit);
                edit.Dock = DockStyle.Fill;
                //this.WindowState = FormWindowState.Maximized;
                edit.AcqToolEdit_Load(null, null);
                DeleteFiles();

                ConnectPLC();
            }
            catch
            {

            }
        }

        private void cb_ShowImgInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_ShowImgInfo.Checked)
            {
                bShowImagePoint = true;
            }
            else bShowImagePoint = false;
        }

        /// 保存图片 直接保存
        /// </summary>
        /// <param name="sSaveName"></param>
        /// <returns></returns>
        public bool SaveImg_JPG(string sSaveName)
        {
            try
            {

                HOperatorSet.WriteImage((HObject)this.ho_Image, ("jpg"), (0), (sSaveName));
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;
        }
        private void DeleteFiles()
        {
            DateTime nowTime = DateTime.Now;
            DirectoryInfo root = new DirectoryInfo(@"./Image");
            FileInfo[] dics = root.GetFiles();
            foreach (FileInfo file in dics)//遍历文件夹
            {
                TimeSpan t = nowTime - file.CreationTime;  //当前时间  减去 文件创建时间
                int day = t.Days;
                if (day >= 3)   //保存的时间 ；  单位：天
                {

                    File.Delete(file.FullName);  //删除超过时间的文件
                }
            }
        }

        private void frmAAVision_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerializerManager<Config>.Instance.Save(AppConfig.ConfigFileName, Config.Instance);
            SerializerManager<Position>.Instance.Save(AppConfig.ConfigPositionName, Position.Instance);
            DisconnectPLC();
            hWindowControl1.Dispose();
            this.Dispose();

        }

        private void LeftPos_Click(object sender, EventArgs e)
        {
            if (cboxLocal.Checked)
            {
                ImagePreviewFileDialog imageDialog = new ImagePreviewFileDialog();
                if (imageDialog.ShowDialog() == DialogResult.OK)
                {
                    LocalVI = new VisionImage(ImageType.Rgb32);
                    LocalVI.ReadFile(imageDialog.FileName);
                    InputImage = new Bitmap(Image.FromFile(imageDialog.FileName));
                    edit.LeftPosProcess(LocalVI, InputImage);
                }
            }
            else
            {
                edit.ReceiveCmd = "C3";
                Acquire();
            }
        }

        private void RightPos_Click(object sender, EventArgs e)
        {
            if (cboxLocal.Checked)
            {
                ImagePreviewFileDialog imageDialog = new ImagePreviewFileDialog();
                if (imageDialog.ShowDialog() == DialogResult.OK)
                {
                    LocalVI = new VisionImage(ImageType.Rgb32);
                    LocalVI.ReadFile(imageDialog.FileName);
                    InputImage = new Bitmap(Image.FromFile(imageDialog.FileName));
                    edit.RightPosProcess(LocalVI, InputImage);
                }
            }
            else
            {
                edit.ReceiveCmd = "C1";
                Acquire();
            }

        }

        private void LeftCheck_Click(object sender, EventArgs e)
        {
            if (cboxLocal.Checked)
            {
                ImagePreviewFileDialog imageDialog = new ImagePreviewFileDialog();
                if (imageDialog.ShowDialog() == DialogResult.OK)
                {
                    LocalVI = new VisionImage(ImageType.Rgb32);
                    LocalVI.ReadFile(imageDialog.FileName);
                    InputImage = new Bitmap(Image.FromFile(imageDialog.FileName));
                    edit.LeftGlueCheck(LocalVI, InputImage, (double)numComspec.Value, (double)numMarkspec.Value);
                }
            }
            else
            {
                edit.ReceiveCmd = "C4";
                Acquire();
            }
        }

        private void RightCheck_Click(object sender, EventArgs e)
        {
            if (cboxLocal.Checked)
            {
                ImagePreviewFileDialog imageDialog = new ImagePreviewFileDialog();
                if (imageDialog.ShowDialog() == DialogResult.OK)
                {
                    LocalVI = new VisionImage(ImageType.Rgb32);
                    LocalVI.ReadFile(imageDialog.FileName);
                    InputImage = new Bitmap(Image.FromFile(imageDialog.FileName));
                    edit.RightGlueCheck(LocalVI, InputImage, (double)numComspec.Value, (double)numMarkspec.Value);
                }
            }
            else
            {
                edit.ReceiveCmd = "C2";
                Acquire();
            }
        }

        private void btnsave_Click(object sender, EventArgs e)
        {

            LeftOffset = new Point((int)numLeft_X.Value, (int)numLeft_Y.Value);
            RightOffset = new Point((int)numRight_X.Value, (int)numRight_Y.Value);

            Position.Instance.GlueComspec = (double)numComspec.Value;
            Position.Instance.GlueMarkspec = (double)numMarkspec.Value;
            Position.Instance.LeftPos_Offset_X = (double)numLeft_X.Value;
            Position.Instance.LeftPos_Offset_Y = (double)numLeft_Y.Value;
            Position.Instance.RightPos_Offset_X = (double)numRight_X.Value;
            Position.Instance.RightPos_Offset_Y = (double)numRight_Y.Value;
            SerializerManager<Position>.Instance.Save(AppConfig.ConfigPositionName, Position.Instance);
            MessageBox.Show("参数保存成功！", "提示");
        }

        public void ConnectPLC()
        {
            adsClient = new TcAdsClient();
            AMSNETID = Config.Instance.AMSNETID;
            PORT = Config.Instance.port;
            try
            {
                adsClient.Connect(AMSNETID, PORT);
                ReceiceHandle = adsClient.CreateVariableHandle(Config.Instance.ReceiceHandle);
                SendHandle = adsClient.CreateVariableHandle(Config.Instance.SendHandle);
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void DisconnectPLC()
        {
            timer1.Enabled = false;
            adsClient.Dispose();
        }

        public string Readstring(int variableHandle)
        {
            try
            {
                return adsClient.ReadAny(variableHandle, typeof(String), new int[] { 5 }).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "";
            }

        }

        public static void Writestring(int variableHandle, string content)
        {
            try
            {
                Sendtemp = content;
                adsClient.WriteAny(variableHandle, content, new int[] { content.Length });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = adsClient.ReadAny(ReceiceHandle, typeof(String), new int[] { 5 }).ToString();
            }
            catch
            {
                log1.Debug("PLC通讯异常！");
            }

            textBox2.Text = Sendtemp;

            if (MachineType != Config.Instance.CurrentProductType)
            {
                ClearBmp();
                MachineType = Config.Instance.CurrentProductType;
                numComspec.Value = (decimal)Position.Instance.GlueComspec;
                numMarkspec.Value = (decimal)Position.Instance.GlueMarkspec;
                numLeft_X.Value = (decimal)Position.Instance.LeftPos_Offset_X;
                numLeft_Y.Value = (decimal)Position.Instance.LeftPos_Offset_Y;
                numRight_X.Value = (decimal)Position.Instance.RightPos_Offset_X;
                numRight_Y.Value = (decimal)Position.Instance.RightPos_Offset_Y;

                LeftOffset = new Point((int)numLeft_X.Value, (int)numLeft_Y.Value);
                RightOffset = new Point((int)numRight_X.Value, (int)numRight_Y.Value);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            log1.Debug("接收到PLC命令：" + textBox1.Text.Trim());
            switch (textBox1.Text.Trim())
            {
                case "C1":
                case "C2":
                case "C3":
                case "C4":
                    edit.ReceiveCmd = textBox1.Text.Trim();
                    Acquire();
                    break;
                default:
                    break;
            }
            textBox2.Text = "";
        }

        private void btnplcwrite_Click(object sender, EventArgs e)
        {
            Writestring(ReceiceHandle, textBox2.Text);
        }

        private void 机种选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmRecipe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\"),
               Config.Instance.CurrentProductType,
               () =>
               {
                   try
                   {
                       Config.Instance.CurrentProductType = frmRecipe.CurrentProductType;
                       Position.Instance = SerializerManager<Position>.Instance.Load(AppConfig.ConfigPositionName);
                   }
                   catch (Exception ex)
                   {
                       MessageBox.Show("加载数据失败！", "错误");
                   }
               },
               () =>
               {
                   try
                   {
                       Config.Instance.CurrentProductType = frmRecipe.CurrentProductType;
                       SerializerManager<Position>.Instance.Save(AppConfig.ConfigPositionName, Position.Instance);
                   }
                   catch (Exception ex)
                   {
                       MessageBox.Show("保存数据失败！", "错误");
                   }

               }).ShowDialog();
        }

        public static void ClearBmp()
        {
            try
            {
                HOperatorSet.ClearWindow(acq.hWindowControl2.HalconWindow);
                HOperatorSet.ClearWindow(acq.hWindowControl3.HalconWindow);
                HOperatorSet.ClearWindow(acq.hWindowControl4.HalconWindow);
            }
            catch { }
        }
    }
}
