using System.Data;

namespace YL.Check.Model
{
    /// <summary>
    /// 功能：实体父类
    /// </summary>
    /*
     * 创建人： 彭莉莉
     * 创建时间：2014-08-20 23:21:26
     *
     * 修改人：
     * 描述：
     * 修改时间：
    */

    public class FormValueArg
    {
        /// <summary>
        /// 虚方法 - 子类实现
        /// </summary>
        /// <param name="row">DataTable中的一行数据</param>
        /// <returns></returns>
        public virtual FormValueArg FromRow(DataRow row)
        {
            return this;
        }
    }
}