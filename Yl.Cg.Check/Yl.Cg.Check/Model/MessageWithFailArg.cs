using System;
using System.Data;
using YL.Check.Model;
using YL.Check.Utilities;

namespace YL.Check.Model
{
    /// <summary>
    /// 功能：检票无效消息提示
    /// </summary>
    /*
     * 创建人： 彭莉莉
     * 创建时间：2014年5月30日
     *
     * 修改人：贾增义
     * 描述：
     * 修改时间：2016年4月24日
    */

    public class MessageWithFailArg : FormValueArg
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 身份证编号/票纸编号
        /// </summary>
        public string IDCard { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string InfoTitle { get; set; }

        /// <summary>
        /// 信息说明
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// 警告
        /// </summary>
        public string WarnInfo { get; set; }

        /// <summary>
        /// 通过本地数据转为实体
        /// </summary>
        /// <param name="row">DataTable中的一行数据</param>
        /// <returns></returns>
        public override FormValueArg FromRow(DataRow row)
        {
            try
            {
                Title = SqlTypeConvertor.ToString(row["Title"]);
                IDCard = SqlTypeConvertor.ToString(row["IDCard"]);
                InfoTitle = SqlTypeConvertor.ToString(row["InfoTitle"]);
                Info = SqlTypeConvertor.ToString(row["Info"]);
                WarnInfo = SqlTypeConvertor.ToString(row["WarnInfo"]);
                return this;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("数据转换错误2" + ex.Message);
                return null;
            }
        }
    }
}