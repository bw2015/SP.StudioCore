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
using System.Net.Sockets;
using SP.StudioCore.Web.ipv6wry;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// 获取IP
    /// </summary>
    public static class IPAgent
    {

        private static IPHeader? header = IocCollection.GetService<IPHeader>();

        /// <summary>
        /// 没有IP地址
        /// </summary>
        private const string NO_IP = "0.0.0.0";

        /// <summary>
        /// IP库的路径
        /// </summary>
        private const string IPDATA_PATH = "ipipfree.ipdb";

        /// <summary>
        /// IPv6数据库
        /// </summary>
        private const string IPV6_PATH = "ipv6wry.db";

        /// <summary>
        /// 获取当前访问的IP（支持IPv6）
        /// </summary>
        public static string IP
        {
            get
            {
                return Context.Current.GetIP();
            }
        }

        /// <summary>
        /// 用Guid表示的IP地址（支持IPv6）
        /// </summary>
        public static Guid IPv6
        {
            get
            {
                return IP.ToGuid();
            }
        }


        /// <summary>
        /// 获取IP（支持IPV6）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
                string values = context.Request.Headers[key];
                if (string.IsNullOrEmpty(values)) continue;
                foreach (string value in values.Split(','))
                {
                    if (IPAddress.TryParse(value, out IPAddress? address))
                    {
                        ip = address.ToString();
                    }
                }
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? NO_IP;
            }
            return ip;
        }

        /// <summary>
        /// 本地缓存库
        /// </summary>
        private static readonly Dictionary<string, CityInfo> addressCache = new Dictionary<string, CityInfo>();

        public static CityInfo GetAddress(string ip)
        {
            if (!IPAddress.TryParse(ip, out IPAddress? address) || ip == NO_IP) return ip ?? string.Empty;
            ip = address.ToString();
            if (addressCache.ContainsKey(ip)) return addressCache[ip];
            lock (addressCache)
            {
                CityInfo info = new CityInfo();
                // IPv6 查询
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (!File.Exists(IPV6_PATH)) return info;
                    AccessFile af = new(IPV6_PATH, "20210510");
                    info = af.query(ip);
                }
                else
                {
                    if (!File.Exists(IPDATA_PATH)) return info;
                    City db = new(IPDATA_PATH);
                    info = db.findInfo(ip, "CN");
                }
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
        /// IP地址转化成为long型（仅支持IPV4）
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long IPToLong(this string ip)
        {
            if (ip == NO_IP) return 0;
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
        /// IPv4转化成为int32格式
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
            if (ip == NO_IP) return false;
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

        /// <summary>
        /// 判断是否是中国大陆地区IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsChina(string ip)
        {
            string info = GetAddress(ip);
            if (!info.Contains("中国")) return false;
            if (info.Contains("香港") || info.Contains("澳门") || info.Contains("台湾")) return false;
            return true;
        }

        /// <summary>
        /// IP转换成为 Guid 格式（兼容ipv4 + ipv6)
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string ip)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(ip);
                byte[] data = ipAddress.GetAddressBytes();
                if (data.Length == 4)
                {
                    data = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, data[0], data[1], data[2], data[3] };
                }
                return new Guid(data);
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Guid转化成为IP（兼容 ipv4+ipv6）
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string ToIP(this Guid guid)
        {
            byte[] data = guid.ToByteArray();
            IPAddress ipAddress = new IPAddress(data);

            if (!data.Take(12).Any(t => t != 0))
            {
                return ipAddress.ToString();
            }
            else
            {
                return ipAddress.MapToIPv4().ToString();
            }
        }
    }
}
