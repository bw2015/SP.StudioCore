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

        /// <summary>
        /// 必须参数，应用的API Key
        /// </summary>
        public string client_id { get; set; } = "SDCAiT76wrGC35Tl1LiMf7o6";

        /// <summary>
        /// 必须参数，应用的Secret Key
        /// </summary>
        public string client_secret { get; set; } = "Re9riydSjzBW9dg0SmiRzLojqwoNgSDr";


    }
}
