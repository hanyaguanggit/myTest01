using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;
using Yl.Ticket5.Common40.Utilities;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    public partial class OutLineTeamRepeatUC : BaseFormTypeUC
    {
        public OutLineTeamRepeatUC()
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

                lbl_order_time.Text = TextureHelper.IntToDateTime(checkResultBM.InTime.Value).ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                Logger.WriteLog("检票团队票无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
