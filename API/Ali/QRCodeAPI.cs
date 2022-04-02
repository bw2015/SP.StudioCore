using Newtonsoft.Json.Linq;
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
    /// 二维码扫描
    /// </summary>
    public class QRCodeAPI : ISetting
    {
        public QRCodeAPI(string queryString) : base(queryString)
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
        /// 识别二维码
        /// </summary>
        /// <param name="src">图片base64编码或者url（http或者https开头）</param>
        /// <returns>识别之后的内容</returns>
        public string Execute(string src)
        {
            if (this.Gateway == null) throw new NullReferenceException("the gateway is null");
            if (string.IsNullOrEmpty(src)) throw new FormatException();

            string json = NetAgent.UploadData(this.Gateway, $"src={src}&type=nor", Encoding.UTF8, headers: new()
            {
                { "Authorization", $"APPCODE {this.AppCode}" }
            });

            try
            {
                JObject info = JObject.Parse(json);
                if (!info.ContainsKey("status") || info["status"]?.Value<int>() != 200) throw new Exception(json);
                return info["msg"]?.Value<string>() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
