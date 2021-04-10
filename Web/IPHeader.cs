using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// 自定义获取IP的头部信息
    /// </summary>
    public sealed class IPHeader
    {
        internal readonly string[] Headers;

        public IPHeader(string[] headers)
        {
            this.Headers = headers;
        }
    }
}
