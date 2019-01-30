using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Yl.Cg.Model.Args.Check;
using YL.Check.Model;

namespace YL.Check.Panels
{
    public partial class DefaultUC : BaseFormTypeUC
    {
        public DefaultUC()
        {
            InitializeComponent();
          
        }
        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="e">实体</param>
        internal override void SetValue(CheckResultBM e)  //FormValueArg
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
    }
}
