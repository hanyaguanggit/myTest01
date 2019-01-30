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
    public partial class CheckTeamSuccessUC : BaseFormTypeUC
    {
        public CheckTeamSuccessUC()
        {
            InitializeComponent();
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
                //lblTitle.Text = "票纸号/身份证号 ";
                //lblTitle.Location = (lblTitle.Parent.Width > lblTitle.Width) ? new Point((lblTitle.Parent.Width - lblTitle.Width) / 2, lblTitle.Location.Y) : new Point(0, lblTitle.Location.Y);
               
                int actualCount = 0;
                if (checkResultBM.ListCheckBM.Count > 6)
                {
                    actualCount = 6;
                    label17.Visible = true;
                }
                    
                else
                {
                    actualCount = checkResultBM.ListCheckBM.Count;
                    label17.Visible = false;
                }

                int count = 0;
                foreach(OrderCheckBM checkBM in checkResultBM.ListCheckBM)
                {
                    count += checkBM.PersonCount.Value;
                }

                for (int i = 0; i < actualCount; i++)
                {
                    OrderCheckBM checkBM = checkResultBM.ListCheckBM[i];

                    #region 票价类型处理

                    //string priceTypeName = string.Empty;
                    //int priceType = checkBM.PriceType.Value;
                    //if (priceType == 1)
                    //    priceTypeName = "成人票";
                    //if (priceType == 2)
                    //    priceTypeName = "学生票";
                    //if (priceType == 3)
                    //    priceTypeName = "老人票";

                    #endregion

                    #region 团队票从人数倒序排列

                    switch (i)
                    {
                        case 0:
                            lbl_title1.Text = checkBM.TicketName;
                            lbl_num1.Text = checkBM.PersonCount.Value.ToString();
                            pl_1.Visible = true;
                            break;
                        case 1:
                            lbl_title2.Text = checkBM.TicketName;
                            lbl_num2.Text = checkBM.PersonCount.Value.ToString();
                            pl_2.Visible = true;
                            break;
                        case 2:
                            lbl_title3.Text = checkBM.TicketName;
                            lbl_num3.Text = checkBM.PersonCount.Value.ToString();
                            pl_3.Visible = true;
                            break;
                        case 3:
                            lbl_title4.Text = checkBM.TicketName;
                            lbl_num4.Text = checkBM.PersonCount.Value.ToString();
                            pl_4.Visible = true;
                            break;
                        case 4:
                            lbl_title5.Text = checkBM.TicketName;
                            lbl_num5.Text = checkBM.PersonCount.Value.ToString();
                            pl_5.Visible = true;
                            break;
                        case 5:
                            lbl_title6.Text = checkBM.TicketName;
                            lbl_num6.Text = checkBM.PersonCount.Value.ToString();
                            pl_6.Visible = true;
                            break;
                    }

                    #endregion
                }

                if (checkResultBM.TicketOrderType == Yl.Cg.Model.TicketOrderTypeEnum.特殊票)
                {
                    try
                    {
                        lbl_title2.Text = "剩余次数：" ;
                        lbl_num2.Text = (checkResultBM.ListCheckBM[0].TotalCount - checkResultBM.ListCheckBM[0].InCount - 1).ToString();
                        pl_2.Visible = true;
                    }
                    catch (Exception ex)
                    { }

                }


                lbl_total.Text = "共  " + count + "  人";


                if (checkResultBM.InUserCardNo != null && !string.IsNullOrEmpty(checkResultBM.InUserCardNo.ToString()))
                {
                    Regex reg = new Regex(@"^(\d{15}$|^\d{17}(\d|X|x))$");  //注意里面的引号 要用双引号表示，而不是用反斜杠

                    if (reg.IsMatch(checkResultBM.InUserCardNo.ToString()))
                    {
                        //证件编号
                        var idCard = checkResultBM.InUserCardNo;
                        var beforeIdCard = idCard.ToString().Substring(0, 6);
                        var afterIdCard = idCard.ToString().Substring(idCard.ToString().Length - 4);
                        //lblIDCard.Text = beforeIdCard + @"****" + afterIdCard;
                        lbl_id.Text = beforeIdCard + @"****" + afterIdCard;
                    }
                }
                else  //当前票纸编号
                {
                    if (checkResultBM.PaperCode != null && !string.IsNullOrEmpty(checkResultBM.PaperCode.ToString()))
                        lbl_id.Text = checkResultBM.PaperCode.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("团队票检票窗体赋值报错" + ex.Message);
            }
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
