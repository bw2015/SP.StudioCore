using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SP.StudioCore.Net
{
    /// <summary>
    /// HTTP请求方式
    /// </summary>
    public interface IHttpRequestConfig
    {
        public string GetUrl(string url);
    }

    /// <summary>
    /// 默认的实现
    /// </summary>
    public class HttpRequestConfig : IHttpRequestConfig
    {
        public string GetUrl(string url)
        {
            Regex regex = new Regex(@"^link", RegexOptions.IgnoreCase);
            if (regex.IsMatch(url))
            {
                url = regex.Replace(url, "http");
            }
            return url;
        }
    }
}
