using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Security
{
    public static class Encryption
    {
        /// <summary>
        /// MD5编码(32位大写）
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding">默认UTF-8</param>
        /// <returns>默认大写</returns>
        public static string toMD5(string input, Encoding encoding = null, int length = 32)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            string md5 = toMD5(encoding.GetBytes(input ?? string.Empty));
            if (length == 32) return md5;
            return md5.Substring(0, length);
        }



        /// <summary>
        /// 获取一个二进制流的MD5值（大寫）
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string toMD5(byte[] buffer)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(buffer);
                return string.Join(string.Empty, data.Select(t => t.ToString("x2"))).ToUpper();
            }
        }

        private const string MD5CHAR = "0123456789ABCDEF";
        /// <summary>
        /// 返回簡寫的MD5值（64位編碼，4位16進制轉化成爲1位）
        /// </summary>
        /// <param name="md5">必須是大寫的MD5</param>
        /// <returns></returns>
        public static string toMD5Short(string md5)
        {
            md5 = md5.ToUpper();
            md5 = Regex.Replace(md5, $@"[^{MD5CHAR}]", "0");
            int unit = 4;
            if (md5.Length % 4 != 0) md5 = md5.Substring(0, md5.Length / 4 * 4);
            Stack<char> value = new Stack<char>();
            for (int i = 0; i < md5.Length / 4; i++)
            {
                string str = md5.Substring(i * unit, unit);
                int num = 0;
                for (int n = 0; n < str.Length; n++)
                {
                    int charIndex = MD5CHAR.IndexOf(str[n]);
                    num += charIndex;
                }
                value.Push(MathHelper.HEX_62[num % MathHelper.HEX_62.Length]);
            }
            return string.Join(string.Empty, value);
        }

        /// <summary>
        /// 使用系统自带的SHA1加密(40位大写）
        /// </summary>
        /// <param name="text"></param>
        /// <returns>大写</returns>
        public static string toSHA1(string text, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            SHA1 algorithm = SHA1.Create();
            byte[] data = algorithm.ComputeHash(encoding.GetBytes(text));
            string sh1 = string.Join(string.Empty, data.Select(t => t.ToString("x2")));
            return sh1.ToUpper();
        }

        /// <summary>
        /// MD5与SHA1的双重加密算法（40位密文）
        /// 大写MD5加SHA1
        /// </summary>
        /// <param name="text">要加密的明文</param>
        /// <returns>加密之后的字串符</returns>
        public static string SHA1WithMD5(string text)
        {
            return toSHA1(toMD5(text));
        }

        /// <summary>
        /// 获取字符串的Hash值
        /// 用于集群Redis的key值计算
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetHash(this string input, int length = 2)
        {
            return toMD5(input).Substring(0, length);
        }
    }
}
