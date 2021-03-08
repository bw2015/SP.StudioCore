using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 日期类型的扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 获取两个时间里面较大的一个
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="time1"></param>
        /// <param name="time2">如果不填写则为1900-1-1</param>
        /// <returns></returns>
        public static DateTime Max(this DateTime time1, DateTime time2 = default)
        {
            if (time2 == default) time2 = new DateTime(1900, 1, 1);
            return time1 > time2 ? time1 : time2;
        }

        /// <summary>
        /// 获取两个时间里面较小的一个
        /// </summary>
        /// <param name="time1"></param>
        /// <param name="time2"></param>
        /// <returns></returns>
        public static DateTime Min(this DateTime time1, DateTime time2 = default)
        {
            return time1 < time2 ? time1 : time2;
        }

        /// <summary>
        /// 判断时间是否是正常值，如果为默认值的话就等于现在的时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetValue(this DateTime time)
        {
            return time.Year <= 1900 ? DateTime.Now : time;
        }
    }
}
