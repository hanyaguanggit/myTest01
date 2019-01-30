using System;

namespace YL.Check.Utilities
{
    /// <summary>
    /// 数据库类型转换类。
    /// </summary>
    /*
     * 创建人：姚睿
     * 创建时间：2014-06-09
     *
     * 修改人：彭莉莉
     * 描述：
     * 修改时间：2014-06-10
     */

    public static class SqlTypeConvertor
    {
        #region Int32

        /// <summary>
        /// ToInt32
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int ToInt32(object data)
        {
            return Convert.ToInt32(data.ToString());
        }

        /// <summary>
        /// ToInt32N
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int? ToInt32N(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToInt32(data.ToString());
        }

        #endregion Int32

        #region UInt16

        /// <summary>
        /// ToInt32
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(object data)
        {
            return Convert.ToUInt16(data.ToString());
        }

        /// <summary>
        /// ToInt32N
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static UInt16? ToUInt16N(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToUInt16(data.ToString());
        }

        #endregion UInt16

        #region Boolean

        /// <summary>
        /// Boolean
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool ToBoolean(object data)
        {
            return Convert.ToBoolean(Convert.ToInt32(data));
        }

        /// <summary>
        /// BooleanN
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool? ToBooleanN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToBoolean(data.ToString());
        }

        #endregion Boolean

        #region Decimal

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static decimal ToDecimal(object data)
        {
            return Convert.ToDecimal(data.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static decimal? ToDecimalN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToDecimal(data.ToString());
        }

        #endregion Decimal

        #region Long

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long ToLong(object data)
        {
            return Convert.ToInt64(data.ToString());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static long? ToLongN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToInt64(data.ToString());
        }

        #endregion Long

        #region DateTime

        /// <summary>
        /// ToDateTime
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object data)
        {
            return Convert.ToDateTime(data.ToString());
        }

        /// <summary>
        /// ToDateTimeN
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime? ToDateTimeN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToDateTime(data.ToString());
        }

        #endregion DateTime

        #region string

        /// <summary>
        /// ToDateTimeN
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToString(object data)
        {
            return data.Equals(DBNull.Value) ? null : Convert.ToString(data.ToString());
        }

        #endregion string

        #region byte

        /// <summary>
        /// ToDateTime
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte ToByte(object data)
        {
            return Convert.ToByte(data.ToString());
        }

        /// <summary>
        /// ToDateTimeN
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte? ToByteN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToByte(data.ToString());
        }

        #endregion byte

        #region Double

        /// <summary>
        /// ToDouble
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double ToDouble(object data)
        {
            return Convert.ToDouble(data.ToString());
        }

        /// <summary>
        /// ToDateTimeN
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double? ToDoubleN(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return Convert.ToDouble(data.ToString());
        }

        #endregion Double

        #region byte[]

        /// <summary>
        /// ToByteArray
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(object data)
        {
            if (data.Equals(DBNull.Value)) return null;
            return (byte[])(data);
        }

        #endregion byte[]
    }
}