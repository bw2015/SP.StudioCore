using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.BaiduAI
{
    /// <summary>
    /// 参数设置
    /// </summary>
    public class BaiduSetting : ISetting
    {
        private const string grant_type = "client_credentials";

        public BaiduSetting(string queryString) : base(queryString)
        {
        }

        public BaiduSetting(string client_id, string client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
        }

        /// <summary>
        /// 必须参数，应用的API Key
        /// </summary>
        public string client_id { get; set; } = "";

        /// <summary>
        /// 必须参数，应用的Secret Key
        /// </summary>
        public string client_secret { get; set; } = "";


    }
}
