using SP.StudioCore.Ioc;
using SP.StudioCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Net
{
    public static class NetAgent
    {
        /// <summary>
        /// 默认的用户代理字符串
        /// </summary>
        private static string USER_AGENT
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
                if (url.Contains('?')) url = url.Substring(0, url.IndexOf('?'));
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

        public static async Task PostAsync(string url, string data)
        {
            StringContent content = new StringContent(data, Encoding.UTF8);
            content.Headers.Add("User-Agent", USER_AGENT);
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            await new HttpClient().PostAsync(new Uri(url), content).ConfigureAwait(false);
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
        public static string UploadData(string url, byte[] data, Encoding encoding = null, WebClient wc = null, Dictionary<string, string> headers = null)
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
                    byte[] dataResult = wc.UploadData(RequestConfig.GetUrl(url), "POST", data);
                    strResult = encoding.GetString(dataResult);
                    wc.Headers.Remove("Content-Type");
                }
                catch (WebException ex)
                {
                    strResult = string.Format("Error:{0},Url:{1}", ex.Message, url);
                    if (ex.Response != null)
                    {
                        StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                        if (reader != null)
                        {
                            strResult = reader.ReadToEnd();
                        }
                    }
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
                byte[] data = wc.DownloadData(RequestConfig.GetUrl(url));
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

        private static HttpClient? httpClient;

        /// <summary>
        /// 异步返回内容
        /// </summary>
        public static async Task<string> GetString(string url, Encoding? encoding = null)
        {
            if (httpClient == null) httpClient = new HttpClient();
            encoding ??= Encoding.UTF8;
            byte[] data = await httpClient.GetByteArrayAsync(RequestConfig.GetUrl(url)).ConfigureAwait(false);
            return encoding.GetString(data);
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
    }
}
