using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Web.IPRule
{
    /// <summary>
    /// IP规则
    /// </summary>
    public interface IIPRule
    {
        /// <summary>
        /// 获取IP
        /// </summary>
        public string GetIP(HttpContext context);
    }
}
