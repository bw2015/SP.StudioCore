using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Net.Models
{
    /// <summary>
    /// HTTP接口的返回
    /// </summary>
    public struct HttpResult
    {
        public HttpResult(Exception ex, string url)
        {
            this.StatusCode = 0;
            this.Content = new
            {
                _url = url,
                _exception = ex.Message
            }.ToJson();
        }

        /// <summary>
        /// 发生网络错误
        /// </summary>
        public HttpResult(HttpRequestException ex, string url)
        {
            this.StatusCode = HttpStatusCode.RequestTimeout;
            this.Content = new
            {
                _url = url,
                _exception = ex.Message
            }.ToJson();
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 返回的Http头信息
        /// </summary>
        public HttpResponseHeaders Headers { get; set; }

        /// <summary>
        /// 返回的内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 二进制内容
        /// </summary>
        public byte[] Data { get; set; }

        public static implicit operator string(HttpResult result)
        {
            return result.Content;
        }
    }
}
