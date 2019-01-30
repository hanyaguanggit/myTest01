using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Yl.Cg.Model.Args.Check;
using Yl.Ticket5.Common40.Utilities;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    /// <summary>
    /// 功能说明：离线检票散客票已入馆
    /// 创建人：ys
    /// 创建日期：2016-07-06 11:23
    /// </summary>
    public partial class OutLineSingleRepeatUC : BaseFormTypeUC
    {
        public OutLineSingleRepeatUC()
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

                if (checkResultBM.PaperCode != null && !string.IsNullOrEmpty(checkResultBM.PaperCode.ToString()))
                    lbl_id.Text = checkResultBM.PaperCode.ToString();
                //入馆时间
                lbl_ticket_time.Text = TextureHelper.IntToDateTime(checkResultBM.InTime.Value).ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                Logger.WriteLog("离线检票散客票重复无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
