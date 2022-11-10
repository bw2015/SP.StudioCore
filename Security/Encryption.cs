using SP.StudioCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
        public static string toMD5(this string input, Encoding? encoding = null, int length = 32)
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
        /// <returns>8位编码</returns>
        public static string toMD5Short(this string md5)
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
                    int charIndex = MD5CHAR.IndexOf(str[n]) * (int)Math.Pow(MD5CHAR.Length, str.Length - n - 1);
                    num += charIndex;
                }
                value.Push(MathHelper.HEX_62[num % MathHelper.HEX_62.Length]);
            }
            return string.Join(string.Empty, value);
        }

        /// <summary>
        /// 加盐的MD5（8位）
        /// </summary>
        /// <param name="str">原文</param>
        /// <param name="slat">加盐字符串</param>
        /// <returns></returns>
        public static string toMD5Short(this string str, string slat)
        {
            string md5 = toMD5($"{str}&{slat}");
            return md5.toMD5Short();
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


        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="data">加密数据</param>
        /// <param name="key">8位字符的密钥字符串</param>
        /// <param name="iv">8位字符的初始化向量字符串</param>
        /// <returns></returns>
        public static string DESEncrypt(string data, string key, string iv)
        {
            byte[] byKey = Encoding.ASCII.GetBytes(key);
            byte[] byIV = Encoding.ASCII.GetBytes(iv);

            using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
            {
                int i = cryptoProvider.KeySize;
                MemoryStream ms = new MemoryStream();
                CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

                StreamWriter sw = new(cst);
                sw.Write(data);
                sw.Flush();
                cst.FlushFinalBlock();
                sw.Flush();
                return Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="data">解密数据</param>
        /// <param name="key">8位字符的密钥字符串(需要和加密时相同)</param>
        /// <param name="iv">8位字符的初始化向量字符串(需要和加密时相同)</param>
        /// <returns></returns>
        public static string? DESDecrypt(string data, string key, string iv)
        {
            byte[] byKey = Encoding.ASCII.GetBytes(key);
            byte[] byIV = Encoding.ASCII.GetBytes(iv);

            byte[] byEnc;
            try
            {
                byEnc = Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }

        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string? AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
            byte[] keyData = Encoding.UTF8.GetBytes(key);
            using (RijndaelManaged rm = new()
            {
                Key = keyData,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {

                ICryptoTransform cTransform = rm.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray);
            }
        }

        /// <summary>
        ///  AES 解密（如果解密失败则返回null）
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string? AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            try
            {
                byte[] toEncryptArray = Convert.FromBase64String(str);

                using (RijndaelManaged rm = new()
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                })
                {

                    ICryptoTransform cTransform = rm.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                    return Encoding.UTF8.GetString(resultArray);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
