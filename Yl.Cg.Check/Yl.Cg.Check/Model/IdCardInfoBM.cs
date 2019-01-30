using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace YL.Check.Model
{
    /// <summary>
    /// CVR 扫描结构。
    /// </summary>
    //[Serializable]
    //[StructLayout(LayoutKind.Sequential, Size = 16, CharSet = CharSet.Ansi)]
    public class IdCardInfoBM
    {/// <summary>
     /// 身份证号码
     /// </summary>
        public string idcode { set; get; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// 性别
        /// </summary>
        public string sex { set; get; }

        /// <summary>
        /// 民族
        /// </summary>
        public string nation { set; get; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public string dateofbirth { set; get; }

        /// <summary>
        /// 家庭住址
        /// </summary>
        public string address { set; get; }

        /// <summary>
        /// 签发机关
        /// </summary>
        public string department { set; get; }

        /// <summary>
        /// 有效开始日期
        /// </summary>
        public string startdate { set; get; }

        /// <summary>
        /// 有效截止日期
        /// </summary>
        public string enddate { set; get; }
    }
}
