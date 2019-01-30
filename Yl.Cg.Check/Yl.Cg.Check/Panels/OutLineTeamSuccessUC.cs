using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;
using YL.Check.Utilities;

namespace YL.Check.Panels
{
    public partial class OutLineTeamSuccessUC : BaseFormTypeUC
    {
        public OutLineTeamSuccessUC()
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
            }
            catch (Exception ex)
            {
                Logger.WriteLog("检票无效窗体赋值报错" + ex.Message);
            }
        }
    }
}
