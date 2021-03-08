using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// 字符串的扩展处理
    /// </summary>
    public static class StringExtendsions
    {
        /// <summary>
        /// 从左侧截取字符串（自动增加省略号）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string str, int length)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (str.Length > length)
            {
                if (length > 3) return str.Substring(0, length - 3) + "...";
                return str.Substring(0, length);
            }

            return str;
        }
        /// <summary>
        /// 获取字符串中出现某字符串的所有索引位置
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int[] NthIndexOf(this string str, char c)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            Queue<int> buff = new Queue<int>();
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    buff.Enqueue(i);
                }
            }
            return buff.ToArray();
        }
        /// <summary>
        /// 获取时间日期，非法日期及空字符为null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime? GetDateTime(this string str)
        {
            if (DateTime.TryParse(str, out DateTime result))
            {
                return result;
            }
            return null;
        }
        /// <summary>
        /// 获取字符串的唯一Hash值（比MD5快）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long GetLongHashCode(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            byte[] byteContents = Encoding.UTF8.GetBytes(str);
            SHA256 hash = new SHA256CryptoServiceProvider();
            byte[] hashText = hash.ComputeHash(byteContents);
            long hashCodeStart = BitConverter.ToInt64(hashText, 0);
            long hashCodeMedium = BitConverter.ToInt64(hashText, 8);
            long hashCodeEnd = BitConverter.ToInt64(hashText, 24);
            return hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
        }
    }
}
