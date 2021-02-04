using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using HalconDotNet;
using System.IO;
using NationalInstruments.Vision;
using Vision_Assistant;
using desay.AAVision.Algorithm;
using NationalInstruments.Vision.Analysis;
using desay.ProductData;

namespace desay
{

    public partial class AcqToolEdit : UserControl
    {
        static ILog log = LogManager.GetLogger(typeof(AcqToolEdit));

        public static double offset_x;
        public static double offset_y;

        public string ReceiveCmd;
        public string SendCmd;       
        private TabPage[] tabpage;
        Bitmap bmp;
        VisionImage VI;

        private BaumerCamera _Subject;
        public BaumerCamera Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }

        private string _CamSN;


        public AcqToolEdit()
        {
            InitializeComponent();
            tabpage = new TabPage[] { this.tab_set };


        }

        public void AcqToolEdit_Load(object sender, EventArgs e)
        {

            UpdataControl(false);

            if (_Subject != null)
            {
                this.CB_CamsList.Enabled = !_Subject.IsOpen;
                _Subject.Ran += _Subject_Ran;

                var camlist = BaumerCameraSystem.listCamera.Select((c) => c.pDevice.SerialNumber).ToList();
                CB_CamsList.DataSource = camlist;
                this.CB_CamsList.SelectedIndex = BaumerCameraSystem.listCamera.FindIndex((c) => c.pDevice.SerialNumber == _Subject.SerialNumber);
            }

            if (_Subject != null)
            {
                _Subject.Initialize(_CamSN);

                _Subject.TriggerSource = TriggerSource.Software;
            }
        }

        #region 相机采集

        void _Subject_Ran(ImageData imageData)
        {
            try
            {
                bmp = ImageFactory.CreateBitmap(_Subject.OutputImageData);
                bmp.Save($"{ @"./ImageTemp/temp.jpg"}");

                VI = new VisionImage(ImageType.Rgb32);
                VI.ReadFile($"{ @"./ImageTemp/temp.jpg"}");

                log.Debug("相机完成" + ReceiveCmd + "图像采集");
            }
            catch
            {
                bmp = null;
            }
            try
            {
                log.Debug("程序执行：" + ReceiveCmd + "处理流程");
                switch (ReceiveCmd)
                {
                    case "C3"://外工位定位
                        LeftPosProcess(VI, bmp);
                      
                        break;
                    case "C4"://外工位点胶检查
                        LeftGlueCheck(VI, bmp, Position.Instance.GlueComspec, Position.Instance.GlueMarkspec);

                        break;
                    case "C1"://里工位定位
                        RightPosProcess(VI, bmp);

                        break;
                    case "C2"://里工位点胶检查                        
                        RightGlueCheck(VI, bmp, Position.Instance.GlueComspec, Position.Instance.GlueMarkspec);
                        
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                log.Debug(ReceiveCmd + "图像识别异常！");                  
            }
            finally
            {
                frmAAVision.Writestring(frmAAVision.SendHandle, SendCmd);
            }

        }

        private void UpdataControl(bool status)
        {
            if (status)
            {
                int Index = CB_CamsList.SelectedIndex;
                if (Index >= 0)
                {
                    _CamSN = CB_CamsList.Items[Index].ToString();
                    CB_CamsList.Text = CB_CamsList.Items[Index].ToString();
                }
            }
            else
            {
                if (BaumerCameraSystem.listCamera != null && BaumerCameraSystem.listCamera.Count > 0)
                {
                    for (int i = 0; i < BaumerCameraSystem.listCamera.Count; i++)
                    {
                        //CB_CamsList.Items.Add(BaumerCameraSystem.listCamera[i].strSN);
                    }
                }
            }

        }

        private void CB_CamsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdataControl(true);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (_Subject != null)
            {
                _Subject.Initialize(_CamSN);

                _Subject.TriggerSource = TriggerSource.Software;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_Subject != null)
            {
                _Subject.Close();
            }

            if (_Subject == null)
            {
                MessageBox.Show("关闭成功");
            }
        }

        #endregion

        #region 图像处理显示

        public static void PosCheck_C(Bitmap bmp, HWindow window, string[] str)
        {
            try
            {
                HObject image, img_old;
                HOperatorSet.GenEmptyObj(out image);
                HOperatorSet.GenEmptyObj(out img_old);
                img_old.Dispose();
                Bitmap2HObject.Bitmap2HObj(bmp, out img_old);
                HTuple htuple;
                HOperatorSet.CountChannels(img_old, out htuple);
                image.Dispose();
                if (htuple == 3) HOperatorSet.Rgb1ToGray(img_old, out image);
                else image = img_old.Clone();
                HTuple width, height;
                HOperatorSet.GetImageSize(image, out width, out height);
                HOperatorSet.SetPart(window, 0, 0, height - 1, width - 1);
                HOperatorSet.DispObj(image, window);
                HOperatorSet.SetLineWidth(window, 3);
                HOperatorSet.SetColor(window, "red");
                HTuple font;
                HOperatorSet.QueryFont(window, out font);
                HOperatorSet.SetFont(window, font[0]+"-Normal-12");
                HOperatorSet.DispText(window, "左上偏差：" + str[0], "window", 30, 7, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "右上偏差：" + str[1], "window", 50, 7, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "右下偏差：" + str[2], "window", 70, 7, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "左下偏差：" + str[3], "window", 90, 7, "blue", new HTuple(), new HTuple());
            }
            catch
            {
                log.Debug("PosCheck_C，图像处理异常！");
            }
        }


        public static void GlueCheck_C(Bitmap bmp, HWindow window, bool[] result, double[] data, PointContour[] MaxDisContours)
        {
            try
            {
                HObject image, img_old;
                HOperatorSet.GenEmptyObj(out image);
                HOperatorSet.GenEmptyObj(out img_old);
                img_old.Dispose();
                Bitmap2HObject.Bitmap2HObj(bmp, out img_old);
                HTuple htuple;
                HOperatorSet.CountChannels(img_old, out htuple);
                image.Dispose();
                if (htuple == 3) HOperatorSet.Rgb1ToGray(img_old, out image);
                else image = img_old.Clone();
                HTuple width, height;
                HOperatorSet.GetImageSize(image, out width, out height);
                HOperatorSet.SetPart(window, 0, 0, height - 1, width - 1);
                HOperatorSet.DispObj(image, window);
                HOperatorSet.SetLineWidth(window, 3);
                HTuple font;
                HOperatorSet.QueryFont(window, out font);
                HOperatorSet.SetFont(window, font[0]+"-Normal-12");
                HOperatorSet.DispText(window, "外胶宽偏差：" + data[0].ToString("f3"), "window", 20, 3, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "内胶宽偏差：" + data[1].ToString("f3"), "window", 40, 3, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "外接点偏差：" + data[2].ToString("f3"), "window", 60, 3, "blue", new HTuple(), new HTuple());
                HOperatorSet.DispText(window, "内接点偏差：" + data[3].ToString("f3"), "window", 80, 3, "blue", new HTuple(), new HTuple());

                HObject region;
                HOperatorSet.SetColor(window, result[0]?"blue":"red");
                HOperatorSet.SetLineWidth(window, 1);
                HOperatorSet.SetDraw(window, "margin");
                HTuple row1 = MaxDisContours[0].Y;
                HTuple column1 = MaxDisContours[0].X;
                HTuple phi = 0;
                HTuple Length1 = 100;
                HTuple Length2 = 100;
                HOperatorSet.GenRectangle2(out region, row1, column1, phi, Length1, Length2);
                HOperatorSet.DispObj(region, window);

                HObject region1;
                HOperatorSet.SetColor(window, result[1]?"blue":"red");
                HOperatorSet.SetLineWidth(window, 1);
                HOperatorSet.SetDraw(window, "margin");
                row1 = MaxDisContours[1].Y;
                column1 = MaxDisContours[1].X;
                HOperatorSet.GenRectangle2(out region1, row1, column1, phi, Length1, Length2);
                HOperatorSet.DispObj(region1, window);

                HObject region2;
                HOperatorSet.SetColor(window, result[2]?"blue":"red");
                HOperatorSet.SetLineWidth(window, 1);
                HOperatorSet.SetDraw(window, "margin");
                row1 = MaxDisContours[2].Y;
                column1 = MaxDisContours[2].X;
                HOperatorSet.GenRectangle2(out region2, row1, column1, phi, Length1, Length2);
                HOperatorSet.DispObj(region2, window);

                HObject region3;
                HOperatorSet.SetColor(window, result[3]?"blue":"red");
                HOperatorSet.SetLineWidth(window, 1);
                HOperatorSet.SetDraw(window, "margin");
                row1 = MaxDisContours[3].Y;
                column1 = MaxDisContours[3].X;
                HOperatorSet.GenRectangle2(out region3, row1, column1, phi, Length1, Length2);
                HOperatorSet.DispObj(region3, window);

                if (result[0] && result[1] && result[2] && result[3])
                {
                    SetString(window, "OK", "green", 60);
                }
                else
                {
                    SetString(window, "NG", "red", 60);
                }
            }
            catch
            {
                log.Debug("GlueCheck_C，图像处理异常！");
            }
        }

        public static void SetString(HWindow window, HTuple str, HTuple color, HTuple size)
        {
            HOperatorSet.SetColor(window, color);
            set_display_font(window, size, "mono", "true", "false");
            HOperatorSet.SetTposition(window, -100, 2000);
            HOperatorSet.WriteString(window, str);
            set_display_font(window, 15, "mono", "true", "false");
        }

        public static void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font, HTuple hv_Bold, HTuple hv_Slant)
        {
            // Local iconic variables 
            // Local control variab();les 
            HTuple hv_OS = null, hv_Fonts = new HTuple();
            HTuple hv_Style = null, hv_Exception = new HTuple(), hv_AvailableFonts = null;
            HTuple hv_Fdx = null, hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            // dev_get_preferences(...); only in hdevelop
            // dev_set_preferences(...); only in hdevelop
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Restore previous behaviour
                hv_Size_COPY_INP_TMP = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt();
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Courier";
                hv_Fonts[1] = "Courier 10 Pitch";
                hv_Fonts[2] = "Courier New";
                hv_Fonts[3] = "CourierNew";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Consolas";
                hv_Fonts[1] = "Menlo";
                hv_Fonts[2] = "Courier";
                hv_Fonts[3] = "Courier 10 Pitch";
                hv_Fonts[4] = "FreeMono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Luxi Sans";
                hv_Fonts[1] = "DejaVu Sans";
                hv_Fonts[2] = "FreeSans";
                hv_Fonts[3] = "Arial";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Times New Roman";
                hv_Fonts[1] = "Luxi Serif";
                hv_Fonts[2] = "DejaVu Serif";
                hv_Fonts[3] = "FreeSerif";
                hv_Fonts[4] = "Utopia";
            }
            else
            {
                hv_Fonts = hv_Font_COPY_INP_TMP.Clone();
            }
            hv_Style = "";
            if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Bold";
            }
            else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Bold";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Italic";
            }
            else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Slant";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
            {
                hv_Style = "Normal";
            }
            HOperatorSet.QueryFont(hv_WindowHandle, out hv_AvailableFonts);
            hv_Font_COPY_INP_TMP = "";
            for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
            {
                hv_Indices = hv_AvailableFonts.TupleFind(hv_Fonts.TupleSelect(hv_Fdx));
                if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                    {
                        hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_Fdx);
                        break;
                    }
                }
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                throw new HalconException("Wrong value of control parameter Font");
            }
            hv_Font_COPY_INP_TMP = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
            HOperatorSet.SetFont(hv_WindowHandle, hv_Font_COPY_INP_TMP);
            // dev_set_preferences(...); only in hdevelop

            return;
        }

        #endregion

        #region 里外工位 定位检查

        public void LeftPosProcess(VisionImage visionImage, Bitmap bitmap)
        {
            switch (Config.Instance.CurrentProductType)
            {
                case "Special_Shape":
                    LeftPos.ProcessImage(visionImage, frmAAVision.LeftOffset);
                    break;

                case "Rectangle":
                    LeftPos.RectLeftPos(visionImage, frmAAVision.LeftOffset);
                    break;

                default:
                    break;
            }
            SendCmd = LeftPos.LeftCali;
            PosCheck_C(bitmap, frmAAVision.acq.hWindowControl3.HalconWindow, LeftPos.LeftCaliArrary);

            if (frmAAVision.acq.SaveImage)
            {
                SaveImage.SaveResult(frmAAVision.acq.hWindowControl3.HalconWindow, "C3");
            }
            ReceiveCmd = "";
        }

        public void RightPosProcess(VisionImage visionImage, Bitmap bitmap)
        {
            switch (Config.Instance.CurrentProductType)
            {
                case "Special_Shape":
                    RightPos.ProcessImage(visionImage, frmAAVision.RightOffset);
                    break;

                case "Rectangle":
                    RightPos.RectRightPos(visionImage, frmAAVision.RightOffset);
                    break;

                default:
                    break;
            }
            
            SendCmd = RightPos.RightCali;
            PosCheck_C(bitmap, frmAAVision.acq.hWindowControl1.HalconWindow, RightPos.RightCaliArrary);

            ClearOtherBmp();
            if (frmAAVision.acq.SaveImage)
            {
                SaveImage.SaveResult(frmAAVision.acq.hWindowControl1.HalconWindow, "C1");
            }
            ReceiveCmd = "";
        }

        public void LeftGlueCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            switch (Config.Instance.CurrentProductType)
            {
                case "Special_Shape":
                    ShapeLeftCheck(visionImage, bitmap, Comspec, Markspec);
                    break;

                case "Rectangle":
                    RectLeftCheck(visionImage, bitmap, Comspec, Markspec);
                    break;

                default:
                    break;
            }

            log.Debug("处理流程结束！");
            if (frmAAVision.acq.SaveImage)
            {
                SaveImage.SaveResult(frmAAVision.acq.hWindowControl4.HalconWindow, "C4");
            }
            ReceiveCmd = "";
        }

        public void RightGlueCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            switch (Config.Instance.CurrentProductType)
            {
                case "Special_Shape":
                    ShapeRightCheck(visionImage, bitmap, Comspec, Markspec);
                    break;

                case "Rectangle":
                    RectRightCheck(visionImage, bitmap, Comspec, Markspec);
                    break;

                default:
                    break;
            }

            log.Debug("处理流程结束！");
            if (frmAAVision.acq.SaveImage)
            {
                SaveImage.SaveResult(frmAAVision.acq.hWindowControl2.HalconWindow, "C2");
            }
            ReceiveCmd = "";
        }

        #endregion

        #region 异形胶水检查

        public void ShapeLeftCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            PointContour[] MaxDisContour = new PointContour[4];
            VisionImage Copyimage = new VisionImage();
            Algorithms.Copy(visionImage, Copyimage);

            try
            {
                LeftOut_Processing.ProcessImage(visionImage);
                log.Debug("程序处理：C4_OUT" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C4_OUT" + "流程" + ex.ToString());
            }

            List<double> listout = LeftOut_Processing.distances.ToList();
            List<PointContour> listoutcontours = LeftOut_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_out = listout[9];
            listout.RemoveAt(9);
            MaxDisContour[0] = listoutcontours[9];
            listoutcontours.RemoveAt(9);
            //其他部分胶水
            double MaxComDis_out = GetMaxCommonDistance(listout, listoutcontours, out MaxDisContour[1]);

            try
            {
                LeftIn_Processing.ProcessImage(Copyimage);
                log.Debug("程序处理：C4_IN" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C4_IN" + "流程" + ex.ToString());
            }

            List<double> listin = LeftIn_Processing.distances.ToList();
            List<PointContour> listincontours = LeftIn_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_in = listin[7];
            listin.RemoveAt(7);
            MaxDisContour[2] = listincontours[7];
            listincontours.RemoveAt(7);
            //其他部分胶水
            double MaxComDis_in = GetMaxCommonDistance(listin, listincontours, out MaxDisContour[3]);

            bool[] DistanceResult = new bool[4] { false, false, false, false };
            DistanceResult[0] = ConnectPos_out < Markspec;
            DistanceResult[1] = MaxComDis_out < Comspec;
            DistanceResult[2] = ConnectPos_in < Markspec;
            DistanceResult[3] = MaxComDis_in < Comspec;

            if (MaxComDis_out < Comspec && MaxComDis_in < Comspec && ConnectPos_out < Markspec && ConnectPos_in < Markspec)
            {
                SendCmd = "OK";
            }
            else
            {
                SendCmd = "NG";
            }

            double[] content = new double[] { MaxComDis_out, MaxComDis_in, ConnectPos_out, ConnectPos_in };
            GlueCheck_C(bitmap, frmAAVision.acq.hWindowControl4.HalconWindow, DistanceResult, content, MaxDisContour);

        }

        public void ShapeRightCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            PointContour[] MaxDisContour = new PointContour[4];
            VisionImage Copyimage = new VisionImage();
            Algorithms.Copy(visionImage, Copyimage);

            try
            {
                Rightout_Processing.ProcessImage(visionImage);
                log.Debug("程序处理：C2_OUT" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C2_OUT" + "流程" + ex.ToString());
            }

            List<double> listout = Rightout_Processing.distances.ToList();
            List<PointContour> listoutcontours = Rightout_Processing.contours.ToList();
            double ConnectPos_out = listout[8];
            listout.RemoveAt(8);
            MaxDisContour[0] = listoutcontours[8];
            listoutcontours.RemoveAt(8);
            double MaxComDis_out = GetMaxCommonDistance(listout, listoutcontours, out MaxDisContour[1]);

            try
            {
                Rightin_Processing.ProcessImage(Copyimage);
                log.Debug("程序处理：C2_IN" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C2_IN" + "流程" + ex.ToString());
            }

            List<double> listin = Rightin_Processing.distances.ToList();
            List<PointContour> listincontours = Rightin_Processing.contours.ToList();
            double ConnectPos_in = listin[7];
            listin.RemoveAt(7);
            MaxDisContour[2] = listincontours[7];
            listincontours.RemoveAt(7);
            double MaxComDis_in = GetMaxCommonDistance(listin, listincontours, out MaxDisContour[3]);

            bool[] DistanceResult = new bool[4] { false, false, false, false };
            DistanceResult[0] = ConnectPos_out < Markspec;
            DistanceResult[1] = MaxComDis_out < Comspec;
            DistanceResult[2] = ConnectPos_in < Markspec;
            DistanceResult[3] = MaxComDis_in < Comspec;

            if (MaxComDis_out < Comspec && MaxComDis_in < Comspec && ConnectPos_out < Markspec && ConnectPos_in < Markspec)
            {
                SendCmd = "OK";
            }
            else
            {
                SendCmd = "NG";
            }

            double[] content = new double[] { MaxComDis_out, MaxComDis_in, ConnectPos_out, ConnectPos_in };
            GlueCheck_C(bitmap, frmAAVision.acq.hWindowControl2.HalconWindow, DistanceResult, content, MaxDisContour);

        }

        #endregion

        #region 矩形胶水检查

        public void RectLeftCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            PointContour[] MaxDisContour = new PointContour[4];
            VisionImage Copyimage = new VisionImage();
            Algorithms.Copy(visionImage, Copyimage);

            try
            {
                LeftOut_Processing.RectLeftOutCheck(visionImage);
                log.Debug("程序处理：C4_OUT" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C4_OUT" + "流程" + ex.ToString());
            }

            List<double> listout = LeftOut_Processing.distances.ToList();
            List<PointContour> listoutcontours = LeftOut_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_out = listout[3];
            listout.RemoveAt(3);
            MaxDisContour[0] = listoutcontours[3];
            listoutcontours.RemoveAt(3);
            //其他部分胶水
            double MaxComDis_out = GetMaxCommonDistance(listout, listoutcontours, out MaxDisContour[1]);

            try
            {
                LeftIn_Processing.RectLeftInCheck(Copyimage);
                log.Debug("程序处理：C4_IN" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C4_IN" + "流程" + ex.ToString());
            }

            List<double> listin = LeftIn_Processing.distances.ToList();
            List<PointContour> listincontours = LeftIn_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_in = listin[3];
            listin.RemoveAt(7);
            MaxDisContour[2] = listincontours[3];
            listincontours.RemoveAt(3);
            //其他部分胶水
            double MaxComDis_in = GetMaxCommonDistance(listin, listincontours, out MaxDisContour[3]);

            bool[] DistanceResult = new bool[4] { false, false, false, false };
            DistanceResult[0] = ConnectPos_out < Markspec;
            DistanceResult[1] = MaxComDis_out < Comspec;
            DistanceResult[2] = ConnectPos_in < Markspec;
            DistanceResult[3] = MaxComDis_in < Comspec;

            if (MaxComDis_out < Comspec && MaxComDis_in < Comspec && ConnectPos_out < Markspec && ConnectPos_in < Markspec)
            {
                SendCmd = "OK";
            }
            else
            {
                SendCmd = "NG";
            }

            double[] content = new double[] { MaxComDis_out, MaxComDis_in, ConnectPos_out, ConnectPos_in };
            GlueCheck_C(bitmap, frmAAVision.acq.hWindowControl4.HalconWindow, DistanceResult, content, MaxDisContour);

        }

        public void RectRightCheck(VisionImage visionImage, Bitmap bitmap, double Comspec, double Markspec)
        {
            PointContour[] MaxDisContour = new PointContour[4];
            VisionImage Copyimage = new VisionImage();
            Algorithms.Copy(visionImage, Copyimage);

            try
            {
                Rightout_Processing.RectRightOutCheck(visionImage);
                log.Debug("程序处理：C2_OUT" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C2_OUT" + "流程" + ex.ToString());
            }

            List<double> listout = Rightout_Processing.distances.ToList();
            List<PointContour> listoutcontours = Rightout_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_out = listout[3];
            listout.RemoveAt(3);
            MaxDisContour[0] = listoutcontours[3];
            listoutcontours.RemoveAt(3);
            //其他部分胶水
            double MaxComDis_out = GetMaxCommonDistance(listout, listoutcontours, out MaxDisContour[1]);

            try
            {
                Rightin_Processing.RectRightInCheck(Copyimage);
                log.Debug("程序处理：C2_IN" + "流程");
            }
            catch (Exception ex)
            {
                log.Debug("程序处理：C2_IN" + "流程" + ex.ToString());
            }

            List<double> listin = Rightin_Processing.distances.ToList();
            List<PointContour> listincontours = Rightin_Processing.contours.ToList();
            //连接处胶水
            double ConnectPos_in = listin[3];
            listin.RemoveAt(3);
            MaxDisContour[2] = listincontours[3];
            listincontours.RemoveAt(3);
            //其他部分胶水
            double MaxComDis_in = GetMaxCommonDistance(listin, listincontours, out MaxDisContour[3]);

            bool[] DistanceResult = new bool[4] { false, false, false, false };
            DistanceResult[0] = ConnectPos_out < Markspec;
            DistanceResult[1] = MaxComDis_out < Comspec;
            DistanceResult[2] = ConnectPos_in < Markspec;
            DistanceResult[3] = MaxComDis_in < Comspec;

            if (MaxComDis_out < Comspec && MaxComDis_in < Comspec && ConnectPos_out < Markspec && ConnectPos_in < Markspec)
            {
                SendCmd = "OK";
            }
            else
            {
                SendCmd = "NG";
            }

            double[] content = new double[] { MaxComDis_out, MaxComDis_in, ConnectPos_out, ConnectPos_in };
            GlueCheck_C(bitmap, frmAAVision.acq.hWindowControl2.HalconWindow, DistanceResult, content, MaxDisContour);

        }

        #endregion

        public double GetMaxCommonDistance(List<double> list, List<PointContour> contours, out PointContour MaxCommonPoint)
        {
            double MaxCommonDis = 0;
            foreach (var item in list)
            {
                if (item > MaxCommonDis)
                {
                    MaxCommonDis = item;
                }
            }
            MaxCommonPoint = contours[list.IndexOf(MaxCommonDis)];
            return MaxCommonDis;
        }

        public static void ClearOtherBmp()
        {
            try
            {
                HOperatorSet.ClearWindow(frmAAVision.acq.hWindowControl2.HalconWindow);
                HOperatorSet.ClearWindow(frmAAVision.acq.hWindowControl3.HalconWindow);
                HOperatorSet.ClearWindow(frmAAVision.acq.hWindowControl4.HalconWindow);
            }
            catch { }
        }


    }
}
