using ipdb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SP.StudioCore.Ioc;
using SP.StudioCore.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Elasticsearch.Net;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// 获取IP
    /// </summary>
    public static class IPAgent
    {

        private static IPHeader header = IocCollection.GetService<IPHeader>();

        /// <summary>
        /// 没有IP地址
        /// </summary>
        private const string NO_IP = "0.0.0.0";

        /// <summary>
        /// 不支持的IP（可能是IPv6）
        /// </summary>
        private const string ERROR_IP = "255.255.255.255";

        /// <summary>
        /// IP库的路径
        /// </summary>
        private const string IPDATA_PATH = "ipipfree.ipdb";

        /// <summary>
        /// 获取当前访问的IP
        /// </summary>
        public static string IP
        {
            get
            {
                return Context.Current.GetIP();
            }
        }

        /// <summary>
        /// IPv4的正则验证
        /// </summary>
        public static readonly Regex regex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
        public static string GetIP(this HttpContext context)
        {
            if (context == null) return NO_IP;
            string ip = string.Empty;

            string[] keys = header == null ?
                new[] { "Ali-CDN-Real-IP", "X-Real-IP", "X-Forwarded-IP", "X-Forwarded-For" }
                :
                header.Headers;

            foreach (string key in keys)
            {
                if (key == null || !context.Request.Headers.ContainsKey(key)) continue;
                string value = context.Request.Headers[key];
                if (regex.IsMatch(value))
                {
                    ip = regex.Match(value).Value;
                    break;
                }
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.MapToIPv4().ToString();
            }
            if (!regex.IsMatch(ip))
            {
                ip = ERROR_IP;
            }
            return ip;
        }



        /// <summary>
        /// 本地缓存库
        /// </summary>
        private static readonly Dictionary<string, CityInfo> addressCache = new Dictionary<string, CityInfo>();

        public static CityInfo GetAddress(string ip)
        {
            if (!regex.IsMatch(ip) || ip == NO_IP) return ip ?? string.Empty;
            if (addressCache.ContainsKey(ip)) return addressCache[ip];
            lock (addressCache)
            {
                if (!File.Exists(IPDATA_PATH)) return new CityInfo(); ;
                City db = new(IPDATA_PATH);
                CityInfo info = db.findInfo(ip, "CN");
                if (!addressCache.ContainsKey(ip)) addressCache.Add(ip, info);
                return info;
            }
        }

        /// <summary>
        /// 批量查询IP地址
        /// </summary>
        /// <param name="iplist"></param>
        public static Dictionary<string, string> GetAddress(string[] iplist)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string ip in iplist.Distinct())
            {
                data.Add(ip, GetAddress(ip));
            }
            return data;
        }

        /// <summary>
        /// 获取当前访问IP
        /// </summary>
        /// <returns></returns>
        public static string GetAddress()
        {
            return GetAddress(IP);
        }

        /// <summary>
        /// IP地址转化成为long型
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long IPToLong(this string ip)
        {
            if (!regex.IsMatch(ip) || ip == NO_IP) return 0;
            byte[] ip_bytes = new byte[8];
            string[] strArr = ip.Split('.');
            for (int i = 0; i < 4; i++)
            {
                ip_bytes[i] = byte.Parse(strArr[3 - i]);
            }
            return BitConverter.ToInt64(ip_bytes, 0);
        }

        /// <summary>
        /// long型转化成为IPv4
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string LongToIP(this long ip)
        {
            if (ip > 4294967295 || ip < 16777216) return NO_IP;
            return string.Join(".", BitConverter.GetBytes(ip).Take(4).Reverse());
        }

        /// <summary>
        /// IP转化成为int32格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static int IPToInt(this string ipv4)
        {
            string[] ipSlices = ipv4.Split('.');
            int result = 0;
            for (int i = 0; i < ipSlices.Length; i++)
            {
                // 将 ip 的每一段解析为 int，并根据位置左移 8 位
                int intSlice = int.Parse(ipSlices[i]) << 8 * i;
                // 求或
                result |= intSlice;
            }
            return result;
        }

        /// <summary>
        /// int32转化成为IPV4格式
        /// </summary>
        /// <param name="ipv4"></param>
        /// <returns></returns>
        public static string IntToIPv4(this int ipv4)
        {
            byte[] bs = BitConverter.GetBytes(ipv4);
            return $"{ bs[3] }.{ bs[2] }.{ bs[1] }.{ bs[0] }";
        }

        /// <summary>
        /// 判断IP是否在网段范围内
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="mask">IP网段 192.168.0.0 格式</param>
        /// <returns></returns>
        public static bool IsMask(this string ip, long mask)
        {
            if (!regex.IsMatch(ip) || ip == NO_IP) return false;
            byte[] mask_bytes = BitConverter.GetBytes(mask).Take(4).Reverse().ToArray();
            byte[] ip_bytes = ip.Split('.').Select(t => byte.Parse(t)).ToArray();

            for (int i = 0; i < ip_bytes.Length; i++)
            {
                if (mask_bytes[i] != 0 && mask_bytes[i] != ip_bytes[i])
                {
                    return false;
                }
            }
            return true;

        }
    }
}
