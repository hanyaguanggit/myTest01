using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;
using System.Text.RegularExpressions;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    public partial class CheckFailUC : BaseFormTypeUC
    {
        public CheckFailUC()
        {
            InitializeComponent();
        }

        internal override void SetTipInfo(string message)
        {
            lbl_message.Visible = true;
            this.lbl_message.Text = message;
        }

        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="e">实体</param>
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
                Logger.WriteLog("无效票检票无效窗体赋值报错" + ex.Message);
            }
        }

        private void lbl_message_Click(object sender, EventArgs e)
        {

        }
    }
}
