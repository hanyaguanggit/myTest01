using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Yl.Cg.Model.Args.Check;
using YL.Check.Model;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    /// <summary>
    /// 功能：检票无效提示窗口
    /// </summary>
    /*
     * 创建人： 贾增义     
     * 创建时间：2016年4月24日
     *
     * 修改人：
     * 描述：
     * 修改时间：
    */

    public partial class MessageWithFailUC : BaseFormTypeUC
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MessageWithFailUC()
        {
            InitializeComponent();
        }
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

            //    //说明
            //    lblInfoTitle.Text = arg.InfoTitle;
            //    lblInfo.Text = arg.Info;


            //    lbprice.Text = "总 价：¥  0";
            //}
            //catch (Exception ex)
            //{
            //    Logger.WriteLog("检票无效窗体赋值报错" + ex.Message);                
            //}
        }
        #endregion 构造函数

    }
}