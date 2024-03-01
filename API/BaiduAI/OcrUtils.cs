using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient.Memcached;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Crmf;
using SP.StudioCore.Array;
using SP.StudioCore.Draw;
using SP.StudioCore.Gateway.Push.Telegrams;
using SP.StudioCore.Json;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SP.StudioCore.Net.Http;
using Ubiety.Dns.Core;
using SP.StudioCore.Data;

namespace SP.StudioCore.API.BaiduAI
{
    /// <summary>
    /// OCR 识别接口
    /// </summary>
    public static class OcrUtils
    {
        /// <summary>
        /// 通用文字识别（标准含位置版）
        /// 0.01/次  10QPS
        /// </summary>
        /// <returns></returns>
        public static List<OCRGeneralResult>? general(this BaiduSetting setting, Bitmap bitmap)
        {
            string? access_token = setting.GetAccessToken();
            string url = $"https://aip.baidubce.com/rest/2.0/ocr/v1/general?access_token={access_token}";
            string image = System.Web.HttpUtility.UrlEncode(bitmap.ToBase64());
            string result = NetAgent.UploadData(url, $"image={image}", Encoding.UTF8);
            JObject info = JObject.Parse(result);
            List<OCRGeneralResult> list = new();
            foreach (JObject item in info["words_result"]?.Value<JArray>() ?? new JArray())
            {
                list.Add(new OCRGeneralResult(item));
            }
            return list;
        }

        /// <summary>
        /// 识别远程图片
        /// 如您以前是百度智能云的老用户，其中API_KEY对应百度智能云的“Access Key ID”，SECRET_KEY对应百度智能云的“Access Key Secret”。
        /// </summary>
        public static IEnumerable<string> getContent(this BaiduSetting setting, string image)
        {
            string? token = setting.GetAccessToken();
            if (token == null) yield break;
            string url = $"https://aip.baidubce.com/rest/2.0/ocr/v1/general_basic?access_token={token}";
            using (HttpClient client = new HttpClient())
            {
                var response = client.Post(url, new Dictionary<string, string>()
                {
                    {"url",image },
                    {"detect_direction","false" },
                    {"detect_language","false" },
                    {"paragraph","false" },
                    {"probability","false" }
                }.ToQueryString(true), new Dictionary<string, string>()
                {
                    {"Content-Type","application/x-www-form-urlencoded" }
                });

                JObject info = JObject.Parse(response.Content);
                foreach (JToken? item in info["words_result"]?.Value<JArray>() ?? [])
                {
                    var words = item?["words"]?.Value<string>();
                    if (string.IsNullOrEmpty(words)) continue;
                    yield return words;
                }
            }
        }
    }

    /// <summary>
    /// 通用文字识别 识别的结果（标准含位置版）
    /// </summary>
    public struct OCRGeneralResult
    {
        public OCRGeneralResult(JObject item) : this()
        {
            this.Text = item["words"]?.Value<string>() ?? string.Empty;
            JObject? location = item["location"]?.Value<JObject>();
            if (location != null)
            {
                this.top = location.Get<int>("top");
                this.left = location.Get<int>("left");
                this.width = location.Get<int>("width");
                this.height = location.Get<int>("height");
            }
        }

        public string Text { get; set; }

        public int top { get; set; }

        public int left { get; set; }

        public int width { get; set; }

        public int height { get; set; }
    }
}
