using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using YL.Check.Model;
using YL.Check.Utilities;
using Yl.Cg.Model.Args.Check;

namespace YL.Check.Panels
{
    /// <summary>
    /// 功能：验票成功
    /// </summary>
    /*
     * 创建人： 贾增义
     * 创建时间：2016年4月24日
     *
     * 修改人：
     * 描述：
     * 修改时间：
    */

    public partial class MessageWithSuccessUC : BaseFormTypeUC
    {
        #region 构造函数 & 属性 & 变量

        /// <summary>
        /// 构造函数
        /// </summary>
        public MessageWithSuccessUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 票价类型用户控件每页加载数量
        /// </summary>
        private const int _pageSizePriceType = 4;

        /// <summary>
        /// 证件编号用户控件每页加载数量
        /// </summary>
        private const int _pageSizeInUserNo = 10;

        /// <summary>
        /// 票价类型用户控件占的总页数
        /// </summary>
        private int PriceTypePageNum { get; set; }

        /// <summary>
        /// 证件编号用户控件占的总页数
        /// </summary>
        private int InUserNoPageNum { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        private int TotalPapeNum { get; set; }

        /// <summary>
        /// 当前页数
        /// </summary>
        private int CurrentPapeNum { get; set; }

        #endregion 构造函数 & 属性 & 变量

        public void SetInCount(int inCount)
        {
            lblInPerson.Text = inCount.ToString();
        }

        #region 内部类型或成员可访问

        // <summary>
        // 通过实体设置控件值
        // </summary>
        // <param name = "e" > 实体 </ param >
        internal override void SetValue(CheckResultBM e)
        {
            //try
            //{
            //    //实体转换
            //    var arg = e as MessageWithSuccessArg;
            //    if (arg == null) return;
            //    lblTitle.Text = "票纸号/身份证号";
            //    Regex reg = new Regex(@"^(\d{15}$|^\d{17}(\d|X|x))$");  //注意里面的引号 要用双引号表示，而不是用反斜杠
            //    if (reg.IsMatch(arg.IDCard))
            //    {
            //        //证件编号
            //        var idCard = arg.IDCard;
            //        var beforeIdCard = idCard.Substring(0, idCard.Length - 8);
            //        var afterIdCard = idCard.Substring(idCard.Length - 4);
            //        lblIDCard.Text = beforeIdCard + @"****" + afterIdCard;
            //    }
            //    else
            //    {
            //        //当前身份证
            //        lblIDCard.Text = arg.IDCard;
            //    }
            //    //总价
            //    this.lbprice.Text = arg.Amount.ToString("C0");
            //    //总人数
            //    lblPersonCount.Text = arg.PersonCount.ToString();
            //    //已验人数
            //    lblInPerson.Text = string.Format("{0}", arg.CheckedCount);

            //    string strRealCheckInfo = string.Empty;

            //    string strOrderName = null;
            //    switch (arg.OrderType)
            //    {
            //        case OrderTypeEnum.WAP团队购票:
            //            strOrderName = "手机团队";
            //            break;
            //        case OrderTypeEnum.现场售票:
            //            strOrderName = "窗口散客";
            //            break;
            //        case OrderTypeEnum.WAP散客购票:
            //            strOrderName = "手机散客";
            //            break;

            //        case OrderTypeEnum.预售旅行社网站售票:
            //            strOrderName = "预售团队";
            //            break;

            //        case OrderTypeEnum.网上预约:
            //            strOrderName = "网上预约";
            //            break;
            //        case OrderTypeEnum.电话下单:
            //            strOrderName = "电话预约";
            //            break;
            //        case OrderTypeEnum.期效票_预售网站已支付:
            //        case OrderTypeEnum.期效票_窗口售票:
            //            lblPersonCount.Text = "----";
            //            strOrderName = "工作票";
            //            break;
            //        default:
            //            strOrderName = "未知类型";
            //            break;
            //    }
            //    lblPriceType.Text = strOrderName;
               
            //}
            //catch (Exception ex)
            //{
            //   Logger.WriteLog("检票成功窗体赋值报错" + ex.Message);
            //}
        }

        #endregion 内部类型或成员可访问

    }
}