using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YL.Check.Model;
using Yl.Cg.Model.Args.Check;

namespace YL.Check.Panels
{
    /// <summary>
    /// 功能说明：闸机被禁用状态UC
    /// 创建人：ys
    /// 创建日期：2016-04-25 11:18
    /// </summary>
    public partial class DisableUC : BaseFormTypeUC
    {
        public DisableUC()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="e">实体</param>
        internal override void SetValue(CheckResultBM e)
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
