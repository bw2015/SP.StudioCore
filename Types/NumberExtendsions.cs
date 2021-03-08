using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 数字的扩展方法
    /// </summary>
    public static class NumberExtendsions
    {
        /// <summary>
        /// 有正负号的数字值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals">小数位数 为-1表示原有数字，不做变动</param>
        /// <returns></returns>
        public static string ToNumber(this decimal value, int decimals = -1)
        {
            if (value == decimal.Zero) return "0";
            string prefix = value > decimal.Zero ? "+" : "-";
            value = Math.Abs(value);
            return $"{prefix}{ (decimals < 0 ? value : Math.Round(value, decimals)) }";
        }

        /// <summary>
        /// 保留小数位数（舍位处理，不做四舍五入）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static decimal ToFixed(this decimal value, int decimals)
        {
            return Math.Round(value, decimals, MidpointRounding.ToNegativeInfinity);
            //AwayFromZero  当一个数字是其他两个数字的中间值时，会将其舍入为两个数字中从零开始较接近的数字。
            //ToEven  当一个数字是其他两个数字的中间值时，会将其舍入为最接近的偶数。
            //ToNegativeInfinity  当一个数字是其他两个数字的中间值时，会将其舍入为最接近且不大于无限精确的结果。
            //ToPositiveInfinity 当一个数字是其他两个数字的中间值时，会将其舍入为最接近且不小于无限精确的结果。
            //ToZero  当一个数字是其他两个数字的中间值时，会将其舍入为最接近结果，而不是无限精确的结果。
        }
    }
}
