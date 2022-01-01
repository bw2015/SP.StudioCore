using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams.Models
{
    /// <summary>
    /// 聊天对象
    /// </summary>
    public class chatTargetResponse
    {
        public long id { get; set; }

        public bool is_bot { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string username { get; set; }

        public string language_code { get; set; }

        public string type { get; set; }
    }
}
