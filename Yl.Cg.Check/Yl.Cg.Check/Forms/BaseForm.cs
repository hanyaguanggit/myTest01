using System.Drawing;
using System.Windows.Forms;

namespace YL.Check
{
    /// <summary>
    /// BaseForm
    /// </summary>
    /*
     * 创建人：贾增义
     * 创建时间：2016-04-23
     */

    public partial class BaseForm : Form
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseForm()
        {
            InitializeComponent();
        }

        #endregion 构造函数

        #region 公共变量以及方法

        /// <summary>
        /// 设置窗体状态
        /// </summary>
        /// <param name="status">窗体类型</param>
        public void SetFormStatus(FormStatusEnum status)
        {
            switch (status)
            {
                case FormStatusEnum.Normal:
                    //BackColor = Color.FromArgb(84, 128, 157);
                    BackColor = Color.FromArgb(36, 36, 36);
                    BackgroundImage = null;
                    break;

                case FormStatusEnum.Warn:
                    BackColor = Color.FromArgb(36, 36, 36);
                    //BackgroundImage = Properties.Resources.form_bg_warn;
                    BackgroundImage = null;
                  
                    break;
            }
        }

        #endregion 公共变量以及方法
    }

    #region 枚举

    /// <summary>
    /// 窗体类型
    /// </summary>
    public enum FormStatusEnum
    {
        Normal,
        Warn
    }

    #endregion 枚举
}