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

        /// <summary>
        /// 数字时间格式转化成为时分秒
        /// </summary>
        /// <param name="minute"></param>
        /// <returns></returns>
        public static string GetMinuteTime(this decimal minute)
        {
            TimeSpan time = TimeSpan.FromMinutes((double)minute);
            List<string> result = new List<string>();
            if (time.Hours > 0) result.Add(time.Hours.ToString());
            result.Add(time.ToString(@"mm\:ss"));
            return string.Join(":", result);
        }

        /// <summary>
        /// 获取时区
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public static DateTime GetTimeZone(this DateTime datetime, int timezone)
        {
            //#1 得到UTC-0 的时间
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(datetime);

            //#2 得到偏差值
            return utcTime.AddHours(timezone);
        }
    }
}
