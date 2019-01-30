using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Yl.Cg.Model.Args.Check;
using YL.Check.Model;
using YL.Check.Utilities;
namespace YL.Check.Panels
{
    /// <summary>
    /// 功能：重复检票提示窗口
    /// </summary>
    /*
     * 创建人： 彭莉莉
     * 创建时间：2014年5月30日
     *
     * 修改人：贾增义
     * 描述：
     * 修改时间：2016年4月24日
    */

    public partial class MessageWithRepeatUC : BaseFormTypeUC
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MessageWithRepeatUC()
        {
            InitializeComponent();
        }

        #endregion 构造函数

        #region 内部类型或成员可访问

        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="e">实体</param>
        internal override void SetValue(CheckResultBM e)
        {
            //try
            //{
            //    var arg = e as MessageWithFailArg;
            //    if (arg == null) return;

            //    //标题
            //    lblTitle.Text = "票纸号/身份证号 ";
            //    lblTitle.Location = (lblTitle.Parent.Width > lblTitle.Width) ? new Point((lblTitle.Parent.Width - lblTitle.Width) / 2, lblTitle.Location.Y) : new Point(0, lblTitle.Location.Y);
            //    //票纸编号
            //    //lblIDCard.Text = arg.IDCard.ToString();

            //    Regex reg = new Regex(@"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$");  
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

            //    //说明
            //    lblInfoTitle.Text = arg.InfoTitle;
            //    lblInfo.Text = arg.Info;
            //}
            //catch (Exception ex)
            //{
            //    Logger.WriteLog("重复检票窗体赋值报错" + ex.Message);
            //}
        }

        #endregion 内部类型或成员可访问

    }
}