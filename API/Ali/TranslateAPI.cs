using Newtonsoft.Json.Linq;
using SP.StudioCore.Enums;
using SP.StudioCore.Model;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.Ali
{
    /// <summary>
    /// 翻译接口设定
    /// </summary>
    public class TranslateAPI : AliAPIBase<TranslateAPI>, ITranslateAPI
    {
        public TranslateAPI(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 执行翻译
        /// </summary>
        /// <param name="content">要翻译的内容</param>
        /// <param name="source">来源语种</param>
        /// <param name="target">目标语种</param>
        /// <param name="result">翻译结果</param>
        /// <returns>是否执行成功</returns>
        public bool Execute(string content, Language source, Language target, out string result)
        {
            if (Gateway == null) throw new NullReferenceException("the gateway is null");
            if (source == target) { result = content; return true; }
            string? s = source.GetAttribute<ISO6391Attribute>()?.Code;
            string? d = target.GetAttribute<ISO6391Attribute>()?.Code;
            if (s == null || d == null || string.IsNullOrEmpty(content)) throw new FormatException();

            string json = NetAgent.UploadData(Gateway, $"s={s}&d={d}&q={content}", Encoding.UTF8, headers: new()
            {
                { "Authorization", $"APPCODE {AppCode}" }
            });

            try
            {
                JObject info = JObject.Parse(json);
                if (!info.ContainsKey("status") || info["status"]?.Value<int>() != 200) throw new Exception(json);
                result = info["msg"]?.Value<string>() ?? string.Empty;
                return true;
            }
            catch
            {
                result = json;
                return false;
            }
        }
    }
}
