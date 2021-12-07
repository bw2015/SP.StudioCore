using Newtonsoft.Json.Linq;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.BaiduAI
{
    /// <summary>
    /// 百度AI的工具类
    /// </summary>
    internal static class BaiduUtils
    {
        /// <summary>
        /// 获取Access Token
        /// </summary>
        /// <returns></returns>
        public static string? GetAccessToken(this BaiduSetting setting)
        {
            string url = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={setting.client_id}&client_secret={setting.client_secret}";
            string result = NetAgent.UploadData(url, string.Empty, Encoding.UTF8);
            JObject info = JObject.Parse(result);
            return info["access_token"]?.Value<string>();
        }
    }
}
