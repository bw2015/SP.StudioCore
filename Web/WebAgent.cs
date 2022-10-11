using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SP.StudioCore.Cache.Memory;
using SP.StudioCore.Enums;
using SP.StudioCore.Model;
using SP.StudioCore.Properties;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// 常用的web工具
    /// </summary>
    public static class WebAgent
    {
        #region ======== 字符串处理  ========

        /// <summary>
        /// 隐藏字符串，只保留前后
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static string Hidden(string text)
        {
            if (text.Length < 3) return "*".PadLeft(text.Length, '*');
            return string.Concat(text.First(), "*".PadLeft(text.Length - 2, '*'), text.Last());
        }

        /// <summary>
        /// 隐藏字符串，保留指定的开头长度
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Hidden(string text, int length)
        {
            if (string.IsNullOrEmpty(text)) return null;
            if (text.Length <= length) return Hidden(text);
            return text.Substring(0, length) + string.Empty.PadRight(text.Length - length, '*');
        }

        /// <summary>
        /// 隐藏IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="length">要隐藏的段数 =1 隐藏最后一位</param>
        /// <returns></returns>
        public static string? HiddenIP(string ip, int length)
        {
            if (!IPAddress.TryParse(ip, out IPAddress? address)) return null;
            string[] ips = ip.Split('.', ':');
            for (int i = 1; i <= length; i++)
            {
                ips[ips.Length - i] = "*";
            }
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return string.Join(".", ips);
            }
            else
            {
                return string.Join(":", ips);
            }
        }

        /// <summary>
        /// 生成校验位
        /// </summary>
        public static int GetValidNumber(string input)
        {
            int[] cardArr = new int[input.Length];
            for (int i = 0; i < cardArr.Length; i++)
            {
                cardArr[i] = int.Parse(input[i].ToString());
            }

            for (int i = cardArr.Length - 2; i >= 0; i -= 2)
            {
                cardArr[i] <<= 1;
                cardArr[i] = cardArr[i] / 10 + cardArr[i] % 10;
            }

            int sum = 0;
            for (int i = 0; i < cardArr.Length; i++)
            {
                sum += cardArr[i];
            }
            return sum % 10;
        }

        /// <summary>
        /// 验证校验位
        /// </summary>
        public static bool IsValidNumver(string input)
        {
            var orderPre17 = input.Substring(0, input.Length - 1);
            int validNumber = GetValidNumber(orderPre17);
            return input.Last() == validNumber.ToString().First();
        }

        /// <summary>
        /// 判断用户名是否符合规则
        /// 只允许数字、字母、下划线
        /// </summary>
        /// <param name="username"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsUserName(string username, int min = 5, int max = 16)
        {
            if (string.IsNullOrEmpty(username)) return false;
            Regex regex = new Regex(@$"^[a-zA-Z0-9_\-]{{{min},{max}}}$");
            return regex.IsMatch(username);
        }

        /// <summary>
        /// 是否符合手机号码规则
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsMobile(string mobile, string code = "+86")
        {
            Regex regex = new Regex(@"^(?<Code>\+\d+)\.(?<Mobile>\d+)$");
            if (regex.IsMatch(mobile))
            {
                code = regex.Match(mobile).Groups["Code"].Value;
                mobile = regex.Match(mobile).Groups["Mobile"].Value;
            }
            switch (code)
            {
                case "+86": // 中国
                    regex = new Regex(@"^1[3-9]\d{9}$");
                    break;
                case "+1":  // 美国
                    regex = new Regex(@"^[2-9]\d{2}[2-9](?!11)\d{6}$");
                    break;
                case "+63": //菲律宾
                    regex = new Regex(@"^0\d{10}$");
                    break;
                case "+886":// 台湾
                    regex = new Regex(@"^9\d{8}$");
                    break;
                default:
                    regex = new Regex(@"^\d{8,11}$");
                    break;
            }
            return regex.IsMatch(mobile);
        }

        /// <summary>
        /// 是否是移动平台
        /// </summary>
        /// <returns></returns>
        public static bool IsMobile()
        {
            if (Context.Current == null) return false;
            string userAgent = Context.Current.Request.Headers[HeaderNames.UserAgent];
            if (string.IsNullOrEmpty(userAgent)) return false;
            return Regex.IsMatch(userAgent, "Mobile|iPad|iPhone|Android", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 检查密码是否符合规则
        /// </summary>
        /// <param name="password"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsPassword(string password, int min = 5, int max = 16)
        {
            if (string.IsNullOrEmpty(password)) return false;
            int len = password.Length;
            return len >= min && len <= max;
        }

        /// <summary>
        /// 检查密码强度
        /// 0：无效密码
        /// 1：低强度
        /// 2：中强度
        /// 3：高强度
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static int CheckPasswordStrong(string password)
        {
            //空字符串强度值为0
            if (string.IsNullOrEmpty(password)) return 0;
            //字符统计
            int iNum = 0, iLtt = 0, iSym = 0;
            foreach (char c in password)
            {
                if (c >= '0' && c <= '9') iNum++;
                else if (c >= 'a' && c <= 'z') iLtt++;
                else if (c >= 'A' && c <= 'Z') iLtt++;
                else iSym++;
            }
            if (iLtt == 0 && iSym == 0) return 1; //纯数字密码
            if (iNum == 0 && iLtt == 0) return 1; //纯符号密码
            if (iNum == 0 && iSym == 0) return 1; //纯字母密码
            if (password.Length <= 8) return 1; //长度不大于8的密码
            if (iLtt == 0) return 2; //数字和符号构成的密码
            if (iSym == 0) return 2; //数字和字母构成的密码
            if (iNum == 0) return 2; //字母和符号构成的密码
            if (password.Length <= 10) return 2; //长度不大于10的密码
            return 3; //由数字、字母、符号构成的密码
        }

        /// <summary>
        /// 判断int?是否为空或者0
        /// </summary>
        /// <param name="password"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsEmptyn(this int? obj)
        {
            if (obj == 0 || obj == null) return true;

            return false;
        }

        /// <summary>
        /// 检查邮箱是否符合规则（允许中文的邮箱地址)
        /// </summary>
        public static bool IsEMail(string email)
        {
            return Regex.IsMatch(email, @"^[A-Za-z0-9\u4e00-\u9fa5]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$");
        }

        /// <summary>
        /// 中英文的名字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsName(string name)
        {
            return Regex.IsMatch(name, @"^[\u4E00-\u9FA5A-Za-z\s]+(·[\u4E00-\u9FA5A-Za-z]+)*$");
        }

        /// <summary>
        /// 中文姓名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsChineseName(string name)
        {
            return Regex.IsMatch(name, @"^[\u4E00-\u9FA5]{2,4}$");
        }

        #endregion

        /// <summary>
        /// 把字符串转化成为数字数组
        /// </summary>
        /// <param name="str">用逗号隔开的数字</param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static T[] GetArray<T>(string str, char split = ',')
        {
            if (str == null) return System.Array.Empty<T>();
            str = str.Replace(" ", string.Empty);
            string regex = null;
            T[] result = System.Array.Empty<T>();
            switch (typeof(T).Name)
            {
                case "Int32":
                case "Byte":
                    regex = string.Format(@"(\d+{0})?\d$", split);
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).Where(t => t.IsType<T>()).ToList().ConvertAll(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    }
                    break;
                case "Guid":
                    regex = @"([0-9a-f]{8}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{12}" + split + @")?([0-9a-f]{8}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{4}\-[0-9a-f]{12})$";
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).ToList().ConvertAll(t => (T)((object)Guid.Parse(t))).ToArray();
                    }
                    break;
                case "Int64":
                    result = str.Split(split).Where(t => t.IsType<T>()).Select(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    break;
                case "Decimal":
                    regex = string.Format(@"([0-9\.]+{0})?\d+$", split);
                    if (Regex.IsMatch(str, regex, RegexOptions.IgnoreCase))
                    {
                        result = str.Split(split).ToList().ConvertAll(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    }
                    break;
                case "Double":
                    result = str.Split(split).Where(t => t.IsType<T>()).Select(t => (T)Convert.ChangeType(t, typeof(T))).ToArray();
                    break;
                case "String":
                    result = str.Split(split).ToList().FindAll(t => !string.IsNullOrEmpty(t.Trim())).ConvertAll(t => (T)((object)t.Trim())).ToArray();
                    break;
                case "DateTime":
                    result = str.Split(split).ToList().FindAll(t => t.IsType<T>()).ConvertAll(t => (T)((object)DateTime.Parse(t))).ToArray();
                    break;
                default:
                    if (typeof(T).IsEnum)
                    {
                        result = str.Split(split).Where(t => Enum.IsDefined(typeof(T), t)).Select(t => (T)Enum.Parse(typeof(T), t)).ToArray();
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 产生随机整数
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int GetRandom(int min = 0, int max = 100)
        {
            return new Random().Next(min, max);
        }

        /// <summary>
        /// 获取一个指定长度内的随机数
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int GetRandom(int length)
        {
            long number = Guid.NewGuid().ToNumber();
            long quotient = (long)Math.Pow(10, length);
            return (int)(number % quotient);
        }

        /// <summary>
        /// 判断是否是英文和数字组成的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsLetterAndNumber(string input, int min, int max)
        {
            string pattern = @"^[a-zA-Z0-9]{" + min + "," + max + "}$";
            return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// 是否符合C#类名的规则
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsClassName(string input)
        {
            string pattern = @"^[_a-z][_a-z0-9]+$";
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 创建一个随机的昵称
        /// </summary>
        /// <returns></returns>
        public static string? GenerateNickName(Language language)
        {
            NickNameModel? model = MemoryUtils.Get("GenerateNickName", TimeSpan.FromMinutes(10), () =>
            {
                return JsonConvert.DeserializeObject<NickNameModel>(Resources.nickname);
            });
            if (model == null) return null;
            return model.Generate(language);
        }

        #region ========  时间戳  ========

        /// <summary>
        /// 获取当前的时间戳(秒,GTM+0）
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            return GetTimestamp(DateTime.Now);
        }
        public static long GetDayNumber(DateTime startAt, DateTime endAt)
        {
            return (GetTimestamp(startAt) - GetTimestamp(endAt)) / 86400;
        }
        /// <summary>
        /// 获取时间戳(秒）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimestamp(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 时间戳转化成为本地时间（秒）
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime GetTimestamp(long timestamp)
        {
            return new DateTime(1970, 1, 1).Add(TimeZoneInfo.Local.BaseUtcOffset).AddSeconds(timestamp);
        }

        /// <summary>
        /// 获取时间戳（毫秒，GTM+0）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimestamps(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        public static long GetTimestamps(DateTime time, TimeZoneInfo timeZone)
        {
            return (time.Subtract(timeZone.BaseUtcOffset).Ticks - 621355968000000000) / 10000;
        }

        public static long GetTimestamps(DateTime time, TimeSpan offsetTime)
        {
            return (time.Subtract(offsetTime).Ticks - 621355968000000000) / 10000;
        }

        /// <summary>
        /// 时间戳转化成为本地时间（毫秒）
        /// </summary>
        /// <param name="timestamp">时间戳（毫秒）</param>
        /// <returns></returns>
        public static DateTime GetTimestamps(long timestamp)
        {
            return new DateTime(1970, 1, 1).Add(TimeZoneInfo.Local.BaseUtcOffset).AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 时间戳转化成为指定时区的时间格式（毫秒)
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static DateTime GetTimestamps(long timestamp, TimeZoneInfo timeZone)
        {
            return new DateTime(1970, 1, 1).Add(timeZone.BaseUtcOffset).AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 时间戳转化成为指定时区的时间格式（毫秒)
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="offsetTime"></param>
        /// <returns></returns>
        public static DateTime GetTimestamps(long timestamp, TimeSpan offsetTime)
        {
            return new DateTime(1970, 1, 1).Add(offsetTime).AddMilliseconds(timestamp);
        }

        /// <summary>
        /// 获取当前的时间戳（毫秒，GTM+0）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimestamps()
        {
            return GetTimestamps(DateTime.Now);
        }

        /// <summary>
        /// 获取当前的时间戳（毫秒，GTM+0），2020年1月1日开始算
        /// </summary>
        public static long GetTimestampsBy2020()
        {
            return GetTimestamps(DateTime.Now) - 1577808000000; //GetTimestamps(new DateTime(2020, 01, 01));
        }

        public static long GetTimestampsBy2020(DateTime time)
        {
            return GetTimestamps(time) - 1577808000000; //GetTimestamps(new DateTime(2020, 01, 01));
        }

        /// <summary>
        /// 获取当前的时间戳（毫秒，GTM+0），2020年1月1日开始算
        /// </summary>
        public static DateTime GetTimestampsBy2020(long timestamp)
        {
            return GetTimestamps(timestamp + 1577808000000);
        }

       

        #endregion

        #region ========  域名处理  ========

        private static string[]? _domain;
        /// <summary>
        /// 顶级域名的类型（从资源文件读取）
        /// </summary>
        public static string[] TopDomain
        {
            get
            {
                if (_domain == null)
                {
                    _domain = XElement.Parse(Resources.Domain).Elements().OrderByDescending(t => t.Value.Length).Select(t => t.Value).ToArray();
                }
                return _domain;
            }
        }

        /// <summary>
        /// 获取域名的顶级域
        /// </summary>
        /// <param name="domain">传入的域名（小写）</param>
        /// <returns>返回null表示无法识别</returns>
        public static string? GetTopDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) return null;
            foreach (string name in TopDomain)
            {
                if (domain.EndsWith(name))
                {
                    Regex regex = new Regex($@"[\w\-]+?\{name}$", RegexOptions.IgnoreCase);
                    return regex.Match(domain).Value;
                }
            }
            return null;
        }

        #endregion
    }
}
