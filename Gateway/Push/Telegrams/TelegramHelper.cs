using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SP.StudioCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams
{
    /// <summary>
    /// Telegram的帮助类
    /// </summary>
    public static class TelegramHelper
    {
        /// <summary>
        /// 获取webhook的推送的信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static getUpdatesResponse? GetWebHookResponse(this HttpContext context)
        {
            string content = context.GetString() ?? string.Empty;
            if (string.IsNullOrEmpty(content)) return default;

            try
            {
                return JsonConvert.DeserializeObject<getUpdatesResponse>(content);
            }
            catch
            {
                return null;
            }
        }
    }
}
