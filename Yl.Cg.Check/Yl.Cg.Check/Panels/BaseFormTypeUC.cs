using System;
using System.Drawing;
using System.Windows.Forms;
using YL.Check.Utilities;
using YL.Check.Model;
using Yl.Cg.Model.Args.Check;

namespace YL.Check.Panels
{
    /// <summary>
    /// 验证结果窗体父类
    /// </summary>
    /*
     * 创建人：姚睿
     * 创建时间：2014-05-21
     *
     * 修改人：彭莉莉
     * 描述：
     * 修改时间：2014年5月22日
     */

    public partial class BaseFormTypeUC : UserControl
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseFormTypeUC()
        {
            InitializeComponent();
        }

        #endregion 构造函数

        #region 内部类型或成员可访问

        /// <summary>
        /// 功能说明：界面展示提示信息
        /// 创建人：ys
        /// 创建日期：2016-05-07 13:27
        /// </summary>
        /// <param name="arg">实体</param>
        internal virtual void SetTipInfo(string message)
        {
        }

        /// <summary>
        /// 通过实体设置控件值
        /// </summary>
        /// <param name="arg">实体</param>
        internal virtual void SetValue(CheckResultBM arg)
        {
        }

        #endregion 内部类型或成员可访问


    }
}