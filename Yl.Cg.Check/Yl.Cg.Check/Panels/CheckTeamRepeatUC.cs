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
using Yl.Ticket5.Common40.Utilities;

namespace YL.Check.Panels
{
    public partial class CheckTeamRepeatUC : BaseFormTypeUC
    {
        public CheckTeamRepeatUC()
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

                try
                {
                    #region 票价类型处理

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
                    foreach (OrderCheckBM checkBM in checkResultBM.ListCheckBM)
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

                    lbl_total.Text = "共  " + count + "  人";

                    //string priceTypeName = string.Empty;
                    //int priceType = checkResultBM.ListCheckBM[0].PriceType.Value;
                    //if (priceType == 1)
                    //    priceTypeName = "成人票";
                    //if (priceType == 2)
                    //    priceTypeName = "学生票";
                    //if (priceType == 3)
                    //    priceTypeName = "老人票";

                    #endregion
                }
                catch(Exception ex)
                {

                }

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

                try
                {
                    if (checkResultBM.InTime != null && checkResultBM.InTime != 0)
                    {
                        lbl_order_time.Text = TextureHelper.IntToDateTime(checkResultBM.InTime.Value).ToString("HH:mm:ss");
                        lbl_ticket_time.Text = TextureHelper.IntToDateTime(checkResultBM.InTime.Value).ToString("HH:mm:ss");
                    }
                    else
                    {
                        lbl_order_time.Text = TextureHelper.IntToDateTime(checkResultBM.ListCheckBM[0].OrderInTime.Value).ToString("HH:mm:ss");
                        lbl_ticket_time.Text = TextureHelper.IntToDateTime(checkResultBM.ListCheckBM[0].InTime.Value).ToString("HH:mm:ss");
                    }
                    }
                    
                catch(Exception ex)
                {

                }
                
            }
            catch (Exception ex)
            {
                Logger.WriteLog("检票团队票无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
