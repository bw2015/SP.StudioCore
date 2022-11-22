using Newtonsoft.Json.Linq;
using SP.StudioCore.Array;
using SP.StudioCore.Data;
using SP.StudioCore.Enums;
using SP.StudioCore.Net;
using SP.StudioCore.Security;
using SP.StudioCore.Utils;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.BaiduAI
{
    /// <summary>
    /// 百度翻译
    /// </summary>
    public class BaiduTranslate : BaiduSetting
    {
        /// <summary>
        /// 通用翻译API HTTPS 地址
        /// </summary>
        public string Gateway { get; set; } = "https://fanyi-api.baidu.com/api/trans/vip/translate";


        public BaiduTranslate(string setting) : base(setting) { }

        /// <summary>
        /// 调用翻译
        /// </summary>
        public bool Execute(string content, Language to, Language from, out string result)
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                {"q",content },
                {"from", from.GetAttribute<ISO6391Attribute>()?.BaiduCode ?? "zh" },
                {"to", to.GetAttribute<ISO6391Attribute>()?.BaiduCode ?? "en" },
                {"appid",this.client_id },
                {"salt",WebAgent.GetTimestamps() }
            };
            string sign = $"{data["appid"]}{data["q"]}{data["salt"]}{this.client_secret}";
            data.Add("sign", sign.toMD5().ToLower());

            result = NetAgent.PostAsync(this.Gateway, data.ToQueryString()).Result;

            try
            {
                result = JObject.Parse(result)?["trans_result"]?[0]?["dst"]?.Value<string>();
                return true;
            }
            catch
            {
                ConsoleHelper.Error(result);
                return false;
            }
        }
    }
}
