using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    public partial class CheckErrorVenue : BaseFormTypeUC
    {
        public CheckErrorVenue()
        {
            InitializeComponent();
        }


        internal override void SetValue(CheckResultBM checkResultBM)
        {
            try
            {
                if (checkResultBM == null) return;

                //标题
                //lblTitle.Location = (lblTitle.Parent.Width > lblTitle.Width) ? new Point((lblTitle.Parent.Width - lblTitle.Width) / 2, lblTitle.Location.Y) : new Point(0, lblTitle.Location.Y);
                Regex reg = new Regex(@"^(\d{15}$|^\d{17}(\d|X|x))$");  //注意里面的引号 要用双引号表示，而不是用反斜杠
                if (checkResultBM.InUserCardNo != null && reg.IsMatch(checkResultBM.InUserCardNo.ToString()))
                {
                    //证件编号
                    var idCard = checkResultBM.InUserCardNo;
                    var beforeIdCard = idCard.ToString().Substring(0, 6);
                    var afterIdCard = idCard.ToString().Substring(idCard.ToString().Length - 4);
                    //lblIDCard.Text = beforeIdCard + @"****" + afterIdCard;
                    lbl_id.Text = beforeIdCard + @"****" + afterIdCard;
                }
                else
                {
                    //当前身份证
                    lbl_id.Text = checkResultBM.PaperCode.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("非本闸机检票无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
