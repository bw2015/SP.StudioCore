using SP.StudioCore.Array;
using SP.StudioCore.Ioc;
using SP.StudioCore.Json;
using SP.StudioCore.Net.Models;
using SP.StudioCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Net
{
    public static class NetAgent
    {
        /// <summary>
        /// 忽略证书错误
        /// </summary>
        static NetAgent()
        {
            ServicePointManager.ServerCertificateValidationCallback =
               new RemoteCertificateValidationCallback(
                    delegate
                    { return true; }
                );
        }

        /// <summary>
        /// 默认的用户代理字符串
        /// </summary>
        internal static string USER_AGENT
        {
            get
            {
                Assembly assembly = typeof(NetAgent).Assembly;
                AssemblyName assemblyName = assembly.GetName();
                return string.Concat(assemblyName.Name, "/", assemblyName.Version);
            }
        }

        /// <summary>
        /// 表单提交
        /// </summary>
        /// <param name="uRL"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string UploadValues(string url, NameValueCollection data, Encoding? encoding = null, WebClient? wc = null, Dictionary<string, string>? headers = null)
        {
            wc ??= CreateWebClient();
            headers ??= new Dictionary<string, string>();
            encoding ??= Encoding.UTF8;
            foreach (KeyValuePair<string, string> header in headers)
            {
                wc.Headers[header.Key] = header.Value;
            }
            byte[] resultData = wc.UploadValues(url, data);
            return encoding.GetString(resultData);
        }

        private static IHttpRequestConfig RequestConfig => IocCollection.GetService<IHttpRequestConfig>() ?? new HttpRequestConfig();


        private static WebClient CreateWebClient(string? url = null, Encoding? encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            WebClient wc = new WebClient
            {
                Encoding = encoding
            };
            wc.Headers.Add("Accept", "*/*");
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains('?')) url = url[..url.IndexOf('?')];
                wc.Headers.Add("Referer", url);
            }
            wc.Headers.Add("Cookie", "");
            wc.Headers.Add("User-Agent", USER_AGENT);
            return wc;
        }

        /// <summary>
        /// 进行gzip的解压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] UnGZip(byte[] data)
        {
            using (MemoryStream dms = new MemoryStream())
            {
                using (MemoryStream cms = new MemoryStream(data))
                {
                    using (System.IO.Compression.GZipStream gzip = new(cms, System.IO.Compression.CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while ((len = gzip.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            dms.Write(bytes, 0, len);
                        }
                    }
                }
                return dms.ToArray();
            }
        }

        /// <summary>
        /// 发送POST表单
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="wc"></param>
        /// <param name="header">要写入头部的信息</param>
        /// <returns></returns>
        public static string UploadData(string url, string data, Encoding? encoding = null, WebClient? wc = null, Dictionary<string, string>? headers = null)
        {
            if (headers == null) headers = new Dictionary<string, string>();
            if (!headers.ContainsKey("Content-Type"))
            {
                headers.Add("Content-Type", "application/x-www-form-urlencoded");
            }
            if (!headers.ContainsKey("User-Agent"))
            {
                headers.Add("User-Agent", USER_AGENT);
            }
            if (encoding == null) encoding = Encoding.UTF8;
            return UploadData(url, encoding.GetBytes(data), encoding, wc, headers);
        }

        public static Task<HttpResponseMessage> UploadDataAsync(string url, string data, Encoding encoding = null, WebClient wc = null, Dictionary<string, string> headers = null)
        {
            if (headers == null) headers = new Dictionary<string, string>();
            if (!headers.ContainsKey("Content-Type"))
            {
                headers.Add("Content-Type", "application/x-www-form-urlencoded");
            }
            if (!headers.ContainsKey("User-Agent"))
            {
                headers.Add("User-Agent", USER_AGENT);
            }
            if (encoding == null) encoding = Encoding.UTF8;

            StringContent content = new StringContent(data, encoding, headers["Content-Type"]);
            Task<HttpResponseMessage> response = new HttpClient().PostAsync(url, content);
            return response;
        }


        /// <summary>
        /// 上传二进制流
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <param name="wc"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string UploadData(string url, byte[] data, Encoding? encoding = null, WebClient? wc = null, Dictionary<string, string>? headers = null)
        {
            encoding ??= Encoding.UTF8;
            using (wc ??= CreateWebClient(url))
            {
                string? strResult;
                try
                {
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                        {
                            wc.Headers[header.Key] = header.Value;
                        }
                    }
                    if (!wc.Headers.AllKeys.Contains("Content-Type"))
                    {
                        wc.Headers.Add(HttpRequestHeader.ContentType, "text/plain;charset=UTF-8");
                    }
                    url = RequestConfig.GetUrl(url);
                    // Console.WriteLine($"URL => {url}");
                    byte[] dataResult = wc.UploadData(url, "POST", data);
                    // 如果返回内容使用了gzip编码
                    if (wc.ResponseHeaders?[HttpRequestHeader.ContentEncoding] == "gzip")
                    {
                        dataResult = UnGZip(dataResult);
                    }
                    strResult = encoding.GetString(dataResult);
                    wc.Headers.Remove("Content-Type");
                }
                catch (WebException ex)
                {
                    string? response = null;
                    if (ex.Response != null)
                    {
                        StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                        if (reader != null)
                        {
                            response = reader.ReadToEnd();
                        }
                    }
                    strResult = new
                    {
                        _url = url,
                        _message = ex.Message,
                        _response = response
                    }.ToJson();
                }
                return strResult;
            }
        }

        /// <summary>
        /// 异步提交（不需要接受返回值）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="wc"></param>
        /// <param name="headers"></param>
        /// <param name="exception">异常处理</param>
        public static void UploadDataAsync(string url, byte[] data, WebClient wc = null, Dictionary<string, string> headers = null, Action<WebException> exception = null)
        {
            using (wc ??= CreateWebClient(url))
            {
                try
                {
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, string> header in headers)
                        {
                            wc.Headers[header.Key] = header.Value;
                        }
                    }
                    if (!wc.Headers.AllKeys.Contains("Content-Type")) wc.Headers.Add(HttpRequestHeader.ContentType, "text/plain;charset=UTF-8");
                    wc.UploadDataAsync(new Uri(RequestConfig.GetUrl(url)), data);
                }
                catch (WebException ex)
                {
                    if (exception != null) exception(ex);
                }
            }
        }

        /// <summary>
        /// 下载数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="header"></param>
        /// <param name="wc"></param>
        /// <returns></returns>
        public static string DownloadData(string url, Encoding encoding, Dictionary<string, string>? header = null, WebClient? wc = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url, encoding);
                isNew = true;
            }
            if (header != null)
            {
                foreach (KeyValuePair<string, string> item in header)
                {
                    wc.Headers[item.Key] = item.Value;
                }
            }
            string? strResult = null;
            try
            {
                url = RequestConfig.GetUrl(url);
                byte[] data = wc.DownloadData(url);
                if (wc.ResponseHeaders?[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    data = UnGZip(data);
                }
                strResult = encoding.GetString(data);
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    strResult = string.Format("Error:{0}", ex.Message);
                }
                else
                {
                    StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), encoding);
                    strResult = reader.ReadToEnd();
                }
            }
            finally
            {
                if (isNew) wc.Dispose();
            }
            return strResult;
        }

        /// <summary>
        /// 使用Get方式下载数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string DownloadData(string url)
        {
            return DownloadData(url, Encoding.UTF8, null);
        }

        /// <summary>
        /// 下载小文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static byte[]? DownloadFile(string url, Dictionary<string, string>? header = null, WebClient? wc = null)
        {
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url);
                isNew = true;
            }
            if (header != null)
            {
                foreach (KeyValuePair<string, string> item in header)
                {
                    wc.Headers[item.Key] = item.Value;
                }
            }
            byte[]? data = null;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                data = null;
            }
            finally
            {
                if (isNew) wc.Dispose();
            }
            return data;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        public static string UploadFile(string url, NameValueCollection nvc, MultipartModel file, Encoding? encoding = null, Dictionary<string, string>? header = null, WebClient? wc = null)
        {
            wc ??= CreateWebClient(url, encoding);
            encoding ??= Encoding.UTF8;
            var multipart = new MultipartFormBuilder();
            foreach (string key in nvc.Keys)
            {
                multipart.AddField(key, nvc[key] ?? string.Empty);
            }
            multipart.AddFile(file);

            wc.Headers.Add(HttpRequestHeader.ContentType, multipart.ContentType);
            using (var stream = multipart.GetStream())
            {
                byte[] data = wc.UploadData(url, stream.ToArray());
                return encoding.GetString(data);
            }
        }

        #region ========  异步方法  ========


        private static HttpClient? _httpClient;

        /// <summary>
        /// 创建一个HttpClient对象
        /// </summary>
        /// <returns></returns>
        private static HttpClient CreateHttpClient()
        {
            _httpClient ??= new HttpClient();
            return _httpClient;
        }

        /// <summary>
        /// 添加默认的Header头
        /// </summary>
        /// <param name="headers"></param>
        private static void AddDefaultHeader(this HttpHeaders headers, Dictionary<string, string>? header = null)
        {
            header ??= new Dictionary<string, string>();
            if (!header.ContainsStringKey("User-Agent")) header.Add("User-Agent", USER_AGENT);
            foreach (KeyValuePair<string, string> item in header)
            {
                if (new[] { "Content-Type" }.Contains(item.Key)) continue;
                headers.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 异步返回内容
        /// </summary>
        public static async Task<HttpResult> GetAsync(string url, Encoding? encoding = null, HttpClientOption? options = null)
        {
            return await SendAsync(url, null, HttpMethod.Get, encoding, options);
        }

        public static async Task<HttpResult> PostAsync(string url, string data, Encoding? encoding = null, HttpClientOption? headers = null)
        {
            encoding ??= Encoding.UTF8;
            headers ??= new Dictionary<string, string>();
            //if (!header.ContainsStringKey("Content-Type")) header.Add("Content-Type", "application/x-www-form-urlencoded");
            return await SendAsync(url, encoding.GetBytes(data), HttpMethod.Post, encoding, headers);
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        public static async Task<HttpResult> SendAsync(string url, byte[]? data, HttpMethod method, Encoding? encoding, HttpClientOption? options = null)
        {
            HttpClient httpClient = CreateHttpClient();
            if (options?.Proxy != null)
            {
                HttpClient.DefaultProxy = options?.Proxy;
            }

            encoding ??= Encoding.UTF8;
            options ??= new HttpClientOption();
            if (!options.Headers.ContainsStringKey("Referer")) options.Referrer = url;
            url = RequestConfig.GetUrl(url);
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, url)
                {
                    Version = new Version(2, 0)
                })
                {
                    if (data != null)
                    {
                        StringContent content = new StringContent(encoding.GetString(data), encoding, options.ContentType);
                        request.Content = content;
                    }
                    request.Headers.AddDefaultHeader(options.Headers);
                    //request.Headers.AddDefaultHeader(header);


                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    byte[] resultData = await response.Content.ReadAsByteArrayAsync();
                    // 如果启用了gzip压缩
                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        resultData = UnGZip(resultData);
                    }
                    return new HttpResult
                    {
                        StatusCode = response.StatusCode,
                        Headers = response.Headers,
                        Data = resultData,
                        Content = encoding.GetString(resultData)
                    };
                }
            }
            // 发生网络错误
            catch (HttpRequestException ex)
            {
                return new HttpResult(ex, url);
            }
            catch (Exception ex)
            {
                return new HttpResult(ex, url);
            }
        }

        #endregion

    }
}
