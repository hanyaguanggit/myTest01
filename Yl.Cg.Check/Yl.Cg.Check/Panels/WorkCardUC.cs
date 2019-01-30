using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;

namespace YL.Check.Panels
{
    public partial class WorkCardUC : BaseFormTypeUC
    {
        public WorkCardUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="e">实体</param>
        internal override void SetValue(CheckResultBM checkResultBM)
        {
            if (checkResultBM != null)
            {
                lbl_id.Text = checkResultBM.PaperCode;
                try
                {
                    if (!string.IsNullOrWhiteSpace(checkResultBM.TicketName))
                        lbl_title.Text = checkResultBM.TicketName;
                    else
                        lbl_title.Text = checkResultBM.ListCheckBM[0].TicketName;
                }
                catch(Exception ex)
                {

                }
            }
                
        }
    }
}
