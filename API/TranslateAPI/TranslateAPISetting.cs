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

namespace SP.StudioCore.API.TranslateAPI
{
    /// <summary>
    /// 翻译接口设定
    /// </summary>
    public class TranslateAPISetting : ISetting
    {
        public TranslateAPISetting(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// 网关
        /// </summary>
        [Description("网关")]
        public string? Gateway { get; set; }

        [Description("密钥")]
        public string? AppCode { get; set; }

        /// <summary>
        /// 执行翻译
        /// </summary>
        public bool Execute(string content, Language source, Language target, out string result)
        {
            if (this.Gateway == null) throw new NullReferenceException("the gateway is null");
            if (source == target) { result = content; return true; }
            string? s = source.GetAttribute<ISO6391Attribute>()?.Code;
            string? d = target.GetAttribute<ISO6391Attribute>()?.Code;
            if (s == null || d == null || string.IsNullOrEmpty(content)) throw new FormatException();

            string json = NetAgent.UploadData(this.Gateway, $"s={s}&d={d}&q={content}", Encoding.UTF8, headers: new()
            {
                { "Authorization", $"APPCODE {this.AppCode}" }
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
