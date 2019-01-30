using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using YL.Check.Model;
using YL.Check.Utilities;

namespace YL.Check.Model
{
    /// <summary>
    /// 功能：验票成功
    /// </summary>
    /*
     * 创建人： 彭莉莉
     * 创建时间：2014年5月29日
     *
     * 修改人：贾增义
     * 描述：
     * 修改时间：2016年4月24日
    */
    public class MessageWithSuccessArg : FormValueArg
    {
        /// <summary>
        /// 订单类型。
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }

        /// <summary>
        /// 身份证编号/票纸编号
        /// </summary>
        public string IDCard { get; set; }

        /// <summary>
        /// 预售购买人姓名
        /// </summary>
        public string YuShouUserName { get; set; }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 总人数
        /// </summary>
        public int PersonCount { get; set; }

        /// <summary>
        /// 已验票数量
        /// </summary>
        public int CheckedCount { get; set; }

        /// <summary>
        /// 警告信息
        /// </summary>
        public string WarnInfo { get; set; }

        /// <summary>
        /// 是否是第一次验票
        /// </summary>
        public int IsShowWarnBox { get; set; }

        /// <summary>
        /// 实名制验证次数
        /// </summary>
        public int CurrentCardCheckedNum { get; set; }

        /// <summary>
        /// 实名制验票说明
        /// </summary>
        public string RealCheckInfo { get; set; }

       
        /// <summary>
        /// 通过本地数据转为实体
        /// </summary>
        /// <param name="row">DataTable中的一行数据</param>
        /// <returns></returns>
        public override FormValueArg FromRow(DataRow row)
        {
            try
            {
                IDCard = SqlTypeConvertor.ToString(row["IDCard"]);
                YuShouUserName = SqlTypeConvertor.ToString(row["YuShouUserName"]);
                Amount = SqlTypeConvertor.ToDecimal(row["Amount"]);
                PersonCount = SqlTypeConvertor.ToInt32(row["PersonCount"]);
                CheckedCount = SqlTypeConvertor.ToInt32(row["CheckedCount"]);
                WarnInfo = SqlTypeConvertor.ToString(row["WarnInfo"]);
                string strIsShowWarnBox = SqlTypeConvertor.ToString(row["Buckup1"]);
                int isShowWarnBox = 0;
                if (int.TryParse(strIsShowWarnBox, out isShowWarnBox))
                {
                    IsShowWarnBox = isShowWarnBox;
                }
                else
                {
                    IsShowWarnBox = 0;
                }
               
                return this;
            }
            catch (Exception ex)
            {
                Logger.WriteLog("数据转换错误1" + ex.Message);
                return null;
            }
        }
    }
}