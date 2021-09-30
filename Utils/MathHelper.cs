using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.StudioCore.Utils
{
    /// <summary>
    /// 数学扩展类
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// 阶乘
        /// 一个正整数的阶乘（factorial）是所有小于及等于该数的正整数的积，并且0的阶乘为1。自然数n的阶乘写作n!
        /// </summary>
        /// <param name="n">正整数</param>
        /// <returns></returns>
        public static int Factorial(int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        /// <summary>
        /// 乘积
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static decimal Product(this IEnumerable<decimal> list)
        {
            decimal value = decimal.One;
            foreach (decimal num in list)
            {
                value *= num;
            }
            return value;
        }

        /// <summary>
        /// 组合数量
        /// 从n个不同的元素中，任取m（m≤n）个元素为一组
        /// </summary>
        /// <returns></returns>
        public static int Combination(int n, int m)
        {
            if (m > n) throw new Exception("发生错误,m>n");
            if (m == 0 || n == m || n == 0) return 1;
            return Factorial(n) / (Factorial(m) * Factorial(n - m));
        }

        /// <summary>
        /// 重复组合
        /// 从n个不同元素中可重复地选取m个元素。不管其顺序合成一组，称为从n个元素中取m个元素的可重复组合
        /// </summary>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int CombinationWithRepetiton(int n, int m)
        {
            return Factorial(n + m - 1) / (Factorial(m) * Factorial(n - 1));
        }

        /// <summary>
        /// 获取排列组合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> Combination<T>(T[] items, int length)
        {
            T[] result = new T[length];
            return CombinationUtil(items, result);
        }

        private static IEnumerable<T[]> CombinationUtil<T>(T[] items, T[] result, int start = 0, int depth = 0)
        {
            for (int i = start; i < items.Length; i++)
            {
                result[depth] = items[i];
                if (depth == result.Length - 1)
                {
                    yield return result;
                }
                else
                {
                    foreach (T[] item in CombinationUtil(items, result, i + 1, depth + 1))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// 取一个二维数组的交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> Intersect<T>(this T[][] list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (list.Length == 1) return list[0];
            IEnumerable<T> result = list[0];
            for (int index = 1; index < list.Length; index++)
            {
                result = result.Intersect(list[index]);
            }
            return result;
        }

        /// <summary>
        /// 16进制
        /// </summary>
        public const string HEX = "0123456789ABCDEF";

        /// <summary>
        /// 62进制
        /// </summary>
        public const string HEX_62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 任意进制的转换
        /// </summary>
        /// <param name="source">来源数值</param>
        /// <param name="sourceSize">来源进制</param>
        /// <param name="targetSize">转换进制</param>
        /// <returns></returns>
        public static string Convert(string source, string sourceSize, string targetSize)
        {
            long num = 0;
            for (int i = source.Length; i > 0; i--)
            {
                num += sourceSize.IndexOf(source[i - 1]) * (long)Math.Pow(source.Length, i - 1);
            }
            return Convert(num, targetSize);
        }

        /// <summary>
        /// 十进制转换成为任意进制
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public static string Convert(long source, string targetSize)
        {
            List<char> list = new List<char>();
            List<long> pow = new List<long>();
            int length = 0;
            while (true)
            {
                long size = (long)Math.Pow(targetSize.Length, length);
                if (source < size) break;
                length++;
                pow.Add(size);
            }
            pow.Reverse();
            for (int i = 0; i < pow.Count; i++)
            {
                long size = source / pow[i];
                list.Add(targetSize[(int)size]);
                source -= size * pow[i];
            }
            return string.Join(string.Empty, list);
        }
    }
}
