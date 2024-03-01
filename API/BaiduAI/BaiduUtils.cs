using Newtonsoft.Json.Linq;
using SP.StudioCore.Array;
using SP.StudioCore.Data;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SP.StudioCore.Net.Http;

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
            string url = $"https://aip.baidubce.com/oauth/2.0/token?client_id={setting.client_id}&client_secret={setting.client_secret}&grant_type=client_credentials";
            using (HttpClient client = new HttpClient())
            {
                HttpClientResponse response = client.Post(url, string.Empty, new Dictionary<string, string>()
                {
                    {"Content-Type","application/json" }
                });
                JObject info = JObject.Parse(response.Content);
                return info["access_token"]?.Value<string>();
            }
        }
    }
}
