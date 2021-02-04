using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Toolkit;
namespace desay.ProductData
{
    [Serializable]
    public class Position
    {
        public static Position Instance = new Position();

        /// <summary>
        /// 胶宽偏差
        /// </summary>
        public double GlueComspec = 0;
        /// <summary>
        /// 接点偏差
        /// </summary>
        public double GlueMarkspec = 0;
        /// <summary>
        /// 外工位X轴补偿
        /// </summary>
        public double LeftPos_Offset_X = 0;
        /// <summary>
        /// 外工位Y轴补偿
        /// </summary>
        public double LeftPos_Offset_Y = 0;
        /// <summary>
        /// 里工位X轴补偿
        /// </summary>
        public double RightPos_Offset_X = 0;
        /// <summary>
        /// 里工位Y轴补偿
        /// </summary>
        public double RightPos_Offset_Y = 0;
        /// <summary>
        /// 定位边缘阈值
        /// </summary>
        public double EdgeThreshold_Left = 30;
        /// <summary>
        /// 定位边缘阈值
        /// </summary>
        public double EdgeThreshold_Right = 30;
        /// <summary>
        /// 二值化阈值
        /// </summary>
        public int Threshold_Left = 140;
        /// <summary>
        /// 二值化阈值
        /// </summary>
        public int Threshold_Right = 140;
        /// <summary>
        /// 外工位X标准点
        /// </summary>
        public double[] SpecLeftPos_X = { 0, 0, 0, 0 };
        /// <summary>
        /// 外工位Y标准点
        /// </summary>
        public double[] SpecLeftPos_Y = { 0, 0, 0, 0 };
        /// <summary>
        /// 里工位X标准点
        /// </summary>
        public double[] SpecRightPos_X = { 0, 0, 0, 0 };
        /// <summary>
        /// 里工位Y标准点
        /// </summary>
        public double[] SpecRightPos_Y = { 0, 0, 0, 0 };
    }
}
