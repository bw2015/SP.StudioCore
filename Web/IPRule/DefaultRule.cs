using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.IPRule
{
    /// <summary>
    /// 默认规则
    /// </summary>
    public class DefaultRule : IIPRule
    {
        public string GetIP(HttpContext context)
        {
            if (context == null) return IPAgent.NO_IP;
            string[] fields = new[] { "Ali-CDN-Real-IP", "X-Real-IP", "X-Forwarded-IP", "X-Forwarded-For" };
            string? ip = null;
            foreach (string key in fields)
            {
                if (key == null || !context.Request.Headers.ContainsKey(key)) continue;
                string values = context.Request.Headers[key];
                if (string.IsNullOrEmpty(values)) continue;
                foreach (string value in values.Split(','))
                {
                    if (IPAddress.TryParse(value.Trim(), out IPAddress? address))
                    {
                        ip = address.ToString();
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(ip)) break;
            }
            return ip ?? IPAgent.NO_IP;
        }
    }
}
