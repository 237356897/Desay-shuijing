using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Toolkit;
using System.Toolkit.Helpers;
namespace desay.ProductData
{
    [Serializable]
    public class Config
    {
        [NonSerialized]
        public static Config Instance = new Config();
        //用户相关信息
        public string userName, AdminPassword = SecurityHelper.TextToMd5("321"), OperatePassword = SecurityHelper.TextToMd5("123");
        public UserLevel userLevel = UserLevel.None;

        /// <summary>
        /// 当前产品型号
        /// </summary>
        public string CurrentProductType  = "defualt";
        /// <summary>
        /// PLC地址
        /// </summary>
        public string AMSNETID = "5.79.176.170.1.1";
        /// <summary>
        /// PLC端口
        /// </summary>
        public int port = 851;
        /// <summary>
        /// 接收变量名称
        /// </summary>
        public string ReceiceHandle = "AUTO.str_pic_new";
        /// <summary>
        /// 发送变量名称
        /// </summary>
        public string SendHandle = "AUTO.str_pic_data_new";

        
    }
}
