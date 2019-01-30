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
    public partial class CheckSingleSuccessUC : BaseFormTypeUC
    {
        public CheckSingleSuccessUC()
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

                try
                {
                    #region 票价类型处理

                    //string priceTypeName = string.Empty;
                    //int priceType = checkResultBM.ListCheckBM[0].PriceType.Value;
                    //if (priceType == 1)
                    //    priceTypeName = "成人票";
                    //if (priceType == 2)
                    //    priceTypeName = "学生票";
                    //if (priceType == 3)
                    //    priceTypeName = "老人票";

                    #endregion
                    if (!string.IsNullOrWhiteSpace(checkResultBM.VenueName))
                        lbl_title.Text = checkResultBM.VenueName;
                    else
                        lbl_title.Text = checkResultBM.ListCheckBM[0].VenueName;

                    if (!string.IsNullOrWhiteSpace(checkResultBM.TicketName))
                        lbl_ticketName.Text = checkResultBM.TicketName;
                    else
                        lbl_ticketName.Text = checkResultBM.ListCheckBM[0].TicketName;

                    if (checkResultBM.TicketOrderType == Yl.Cg.Model.TicketOrderTypeEnum.特殊票)
                    {
                        try
                        {
                            lbl_time.Text = "剩余次数：" + (checkResultBM.ListCheckBM[0].TotalCount - checkResultBM.ListCheckBM[0].InCount-1);
                        }
                        catch(Exception ex)
                        { }
                        
                    }
                    
                    if (checkResultBM.TicketOrderType == Yl.Cg.Model.TicketOrderTypeEnum.场次票)
                    {
                        lbl_time.Text = TextureHelper.IntToTime(checkResultBM.ListCheckBM[0].BeginTime.Value) + "~" + TextureHelper.IntToTime(checkResultBM.ListCheckBM[0].EndTime.Value);
                        TimeSpan ts = TextureHelper.IntToTime(checkResultBM.ListCheckBM[0].EndTime.Value) - TextureHelper.IntToTime(checkResultBM.ListCheckBM[0].BeginTime.Value);
                        lbl_long.Text = "影片时长："+(ts.Minutes+ts.Hours*60).ToString()+" 分";
                    }
                        
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
            }
            catch (Exception ex)
            {
                Logger.WriteLog("检票无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
