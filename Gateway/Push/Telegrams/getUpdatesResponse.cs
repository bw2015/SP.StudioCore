using SP.StudioCore.Gateway.Push.Telegrams.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams
{
    /// <summary>
    /// getUpdates 接口返回的信息
    /// </summary>
    public class getUpdatesResponse : IResultResponse
    {
        public long update_id { get; set; }

        public messageResponse message { get; set; }
    }
}
