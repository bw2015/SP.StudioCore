using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SP.StudioCore.Array;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using SP.StudioCore.Model;
using SP.StudioCore.Properties;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SP.StudioCore.Http
{
    public static class ContextExtensions
    {
        #region ===========  UserAgent的判断  =============

        /// <summary>
        /// 是否是移动端
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsMobile(this HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("User-Agent")) return false;
            string userAgent = context.Request.Headers["User-Agent"];
            if (string.IsNullOrEmpty(userAgent)) return false;
            return Regex.IsMatch(userAgent, "Mobile|iPad|iPhone|Android", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 是否在微信内访问
        /// </summary>
        public static bool IsWechat(this HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("User-Agent")) return false;
            string userAgent = context.Request.Headers["User-Agent"];
            return Regex.IsMatch(userAgent, "MicroMessenger", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 获取平台
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static PlatformType GetPlatform(this HttpContext context)
        {
            PlatformType platform = 0;
            if (context == null || !context.Request.Headers.ContainsKey("User-Agent")) return platform;
            string userAgent = context.Request.Headers["User-Agent"];
            if (string.IsNullOrEmpty(userAgent)) return platform;

            // 移动端
            if (Regex.IsMatch(userAgent, "Android|iPhone|iPad|Mobile", RegexOptions.IgnoreCase))
            {
                platform = PlatformType.Mobile;
                if (Regex.IsMatch(userAgent, "micromessenger", RegexOptions.IgnoreCase)) platform |= PlatformType.Wechat;
                if (Regex.IsMatch(userAgent, "Android", RegexOptions.IgnoreCase)) platform |= PlatformType.Android;
                if (Regex.IsMatch(userAgent, "iPhone|iPad", RegexOptions.IgnoreCase)) platform |= PlatformType.IOS;
                // Device/Huawei Model/HUAWEI,M17-TL00 Android/6.0 Version/1.0.0 IMEI/865 APP/com.betwin4.wkdj
                // Device/iPhone Model/iPhone1,1 IOS/7.1.1 Version/1.0.0 IMEI/7737D97A-35F6-4C0E-B0EC-DF1D711D99E1 APP/com.betwin4.wkdj
                if (Regex.IsMatch(userAgent, "APP")) platform |= PlatformType.APP;
            }
            else
            {
                platform = PlatformType.PC;
                if (Regex.IsMatch(userAgent, "Windows", RegexOptions.IgnoreCase)) platform |= PlatformType.Windows;
                if (Regex.IsMatch(userAgent, "Macintosh", RegexOptions.IgnoreCase)) platform |= PlatformType.MAC;
            }

            return platform;
        }

        #endregion

        #region ========== 获取数据 ==============

        public static string QF(this HttpContext context, string key)
        {
            if (context.Request.Method == "POST" && context.Request.HasFormContentType && context.Request.ContentLength != null && context.Request.Form.ContainsKey(key))
            {
                return context.Request.Form[key];
            }

            return null;
        }

        public static T QF<T>(this HttpContext context, string key, T t)
        {
            string value = context.QF(key);
            if (string.IsNullOrEmpty(value)) return t;
            return value.IsType<T>() ? value.GetValue<T>() : t;
        }

        public static string QS(this HttpContext context, string key)
        {
            return context.Request.Query[key];
        }

        public static T QS<T>(this HttpContext context, string key, T t)
        {
            string value = context.QS(key);
            if (string.IsNullOrEmpty(value)) return t;
            return value.IsType<T>() ? value.GetValue<T>() : t;
        }

        /// <summary>
        /// 从HTTP头获取内容
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Head(this HttpContext context, string key)
        {
            return context.Request.Headers[key];
        }

        /// <summary>
        /// 同头部获取指定类型的内容
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue Head<TValue>(this HttpContext context, string key)
        {
            string value = context.Head(key);
            if (string.IsNullOrEmpty(value)) return default;
            return value.GetValue<TValue>();
        }

        /// <summary>
        /// 按照Form、Query、Cookie、Head的顺序获取key值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetParam(this HttpContext context, string key)
        {
            string value = context.QF(key);
            if (value == null) value = context.QS(key);
            if (value == null) value = context.Request.Cookies[key];
            if (value == null) value = context.Head(key);
            return value;
        }

        public static T GetItem<T>(this HttpContext context)
        {
            if (context == null || !context.Items.ContainsKey(typeof(T))) return default;
            return (T)context.Items[typeof(T)];
        }

        public static void SetItem<T>(this HttpContext context, T value)
        {
            if (context == null) return;
            Type key = typeof(T);
            if (!context.Items.ContainsKey(key))
            {
                context.Items.Add(key, value);
            }
            else
            {
                context.Items[key] = value;
            }
        }

        /// <summary>
        /// 使用自定义的名字设定传递值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetItem<T>(this HttpContext context, string name, T value)
        {
            if (!context.Items.ContainsKey(name))
            {
                context.Items.Add(name, value);
            }
        }

        public static T GetItem<T>(this HttpContext context, string name)
        {
            if (!context.Items.ContainsKey(name)) return default;
            return (T)context.Items[name];
        }


        /// <summary>
        /// 获取日志数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetLog(this HttpContext context)
        {
            if (context == null) return null;
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("URL", context.Request.Host + context.Request.Path.ToString());
            data.Add("Headers", context.Request.Headers.Keys.ToDictionary(t => t, t => context.Request.Headers[t].ToString()));
            if (context.Request.HasFormContentType)
            {
                data.Add("Data", context.Request.Form.Keys.ToDictionary(t => t, t => context.Request.Form[t].ToString()));
            }
            else
            {
                data.Add("Data", context.GetString() ?? string.Empty);
            }

            return data.ToJson();
        }


        public static byte[]? GetData(this HttpContext context)
        {
            if (context.Request.Method != "POST" || context.Request.ContentLength == null || context.Request.ContentLength == 0) return null;
            byte[] data = context.GetItem<byte[]>();
            if (data != null) return data;
            try
            {
                using (MemoryStream ms = new MemoryStream((int)context.Request.ContentLength))
                {
                    context.Request.Body.CopyToAsync(ms).Wait();
                    ms.Position = 0;
                    data = ms.ToArray();
                }

                return data;
            }
            finally
            {
                context.SetItem(data);
            }
        }

        public static string? GetString(this HttpContext context, Encoding? encoding = null)
        {
            if (context == null) return string.Empty;
            if (encoding == null) encoding = Encoding.UTF8;
            byte[]? data = context.GetData();
            if (data == null) return string.Empty;
            if (data.Length == 0) return context.Request.ContentLength.ToString();
            if (data == null) return "null";
            return encoding.GetString(data);
        }

        /// <summary>
        /// 获取JSON并且序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T? GetJson<T>(this HttpContext context, Encoding? encoding = null)
        {
            string input = context.GetString(encoding);
            if (string.IsNullOrEmpty(input)) return default;
            return JsonConvert.DeserializeObject<T>(input);
        }

        /// <summary>
        /// 新建一个对象赋值（要求必须拥有无参构造）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="obj">具有初始值的对象</param>
        /// <param name="prefix">前缀</param>
        /// <returns></returns>
        public static T Fill<T>(this IFormCollection form, string? prefix = null) where T : class, new()
        {
            T obj = new T();
            return form.Fill(obj, prefix);
        }

        /// <summary>
        /// 对已有对象赋值
        /// </summary>
        ///  <param name="isHtmlEncode">是否过滤</param>
        public static T Fill<T>(this IFormCollection form, T obj, string? prefix = null, bool isHtmlEncode = true) where T : class
        {
            foreach (string key in form.Keys)
            {
                string name = key;
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!name.StartsWith(prefix)) continue;
                    name = name.Substring(prefix.Length);
                }

                PropertyInfo property = obj.GetType().GetProperty(name);
                if (property == null) continue;
                string value = form[key];
                object objValue;

                switch (property.PropertyType.Name)
                {
                    case "String":
                        objValue = (isHtmlEncode && property.HasAttribute<HtmlEncodeAttribute>()) ? HttpUtility.HtmlDecode(value) : value;
                        break;
                    default:
                        objValue = value.GetValue(property.PropertyType);
                        break;
                }

                property.SetValue(obj, objValue);
            }

            return obj;
        }

        public static object? Fill(this IFormCollection form, Type type, string? prefix = null, bool isHtmlEncode = true)
        {
            object? obj = Activator.CreateInstance(type);
            if (obj == null) return null;
            foreach (string key in form.Keys)
            {
                string name = key;
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!name.StartsWith(prefix)) continue;
                    name = name.Substring(prefix.Length);
                }

                PropertyInfo? property = type.GetProperty(name);
                if (property == null) continue;
                string value = form[key];
                object objValue;

                switch (property.PropertyType.Name)
                {
                    case "String":
                        objValue = (isHtmlEncode && property.HasAttribute<HtmlEncodeAttribute>()) ? HttpUtility.HtmlDecode(value) : value;
                        break;
                    default:
                        objValue = value.GetValue(property.PropertyType);
                        break;
                }

                property.SetValue(obj, objValue);
            }

            return obj;
        }

        /// <summary>
        /// 获取当前的域名（支持反代过来的域名）
        /// 顺序："X-Forwarded-Site", "Ali-Swift-LOG-Host", "Referer", "Host"
        /// </summary>
        /// <returns></returns>
        public static string? GetDomain(this HttpContext context)
        {
            if (context == null) return null;
            string domain = string.Empty;
            Regex url = new Regex(@"^(http|https)://(?<Domain>.+?)/", RegexOptions.IgnoreCase);
            try
            {
                foreach (string key in new[] { "X-Forwarded-Site", "Ali-Swift-LOG-Host", "Referer", "Host" })
                {
                    string value = context.Request.Headers[key];
                    if (string.IsNullOrEmpty(value)) continue;
                    if (url.IsMatch(value))
                    {
                        domain = url.Match(value).Groups["Domain"].Value;
                    }
                    else
                    {
                        domain = value;
                    }
                    break;
                }
            }
            catch
            {
                domain = context.Request.Host.ToString();
            }

            return domain;
        }

        /// <summary>
        /// 获取语种（默认为简体中文）
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Language GetLanguage(this HttpContext context, Language defaultLanguage = Language.CHN)
        {
            if (context.Request.Headers.ContainsKey(Const.LANGUAGE))
            {
                if (Enum.IsDefined(typeof(Language), context.Head(Const.LANGUAGE)))
                {
                    return context.Head(Const.LANGUAGE).ToEnum<Language>();
                }
                return defaultLanguage;
            }
            return defaultLanguage;
        }

        /// <summary>
        /// 获取 Auth 认证信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns>是否获取成功</returns>
        public static bool GetAuth(this HttpContext context, out string username, out string password)
        {
            username = password = null;
            string auth = context.Head("Authorization");
            if (string.IsNullOrEmpty(auth)) return false;
            Regex basic = new Regex(@"^Basic (?<Content>[a-zA-Z0-9/+]*={0,2})$");
            if (!basic.IsMatch(auth)) return false;
            string content = basic.Match(auth).Groups["Content"].Value;
            content = Encoding.UTF8.GetString(Convert.FromBase64String(content));
            basic = new Regex(@"^(?<uid>.+?):(?<pwd>.+)$");
            if (!basic.IsMatch(content))
            {
                username = password = content;
            }
            else
            {
                username = basic.Match(content).Groups["uid"].Value;
                password = basic.Match(content).Groups["pwd"].Value;
            }

            return true;
        }

        /// <summary>
        /// 转化成为BasicAuth认证的字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ToBasicAuth(this string content)
        {
            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(content))}";
        }

        #endregion

        #region =========== 错误页面提示  ===============

        public static Result ShowError(this HttpContext context, HttpStatusCode statusCode, string? error = null)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "text/html";
            return statusCode.ShowError(error);
        }

        /// <summary>
        ///  错误页面
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string ShowError(this HttpStatusCode statusCode, string? error = null)
        {
            string status = Regex.Replace(statusCode.ToString(), "[A-Z]", t => { return " " + t.Value; });
            status = statusCode.ToString();
            if (string.IsNullOrEmpty(error))
            {
                error = status;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<html><head><title>{0} {1}</title><meta name=\"description\" content=\"{2}\" /><body><h1><center>{0} {1}</center></h1>", (int)statusCode, status, error);
            sb.Append("<hr />");
            sb.AppendFormat("<center>{0}/{1}</center>", typeof(ContextExtensions).Assembly.GetName().Name, typeof(ContextExtensions).Assembly.GetName().Version);
            sb.Append("</body></html>");
            return sb.ToString();
        }

        public static Result ShowError(this HttpContext context, ErrorType type, string? error = null, Dictionary<string, object>? output = null)
        {
            if (string.IsNullOrEmpty(error)) error = type.GetDescription();
            if (output == null)
            {
                output = new Dictionary<string, object>();
            }

            output.Add("ErrorType", type);

            return new Result(false, error, output);
        }

        #endregion

        #region ========  对外输出  ========

        /// <summary>
        /// 对外输出 200页面
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Task Write(this HttpContext context, Result result)
        {
            return result.WriteAsync(context);
        }

        /// <summary>
        /// 输出未授权页面
        /// </summary>
        /// <param name="context"></param>
        /// <param name="realm"></param>
        /// <returns></returns>
        public static Task WriteUnauthorized(this HttpContext context, string realm)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{realm}\"");
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(Resources._401);
        }

        #endregion

        #region ========  文件上传  ========

        /// <summary>
        /// 上传的文件对象转换成为byte[]
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static byte[]? ToArray(this IFormFile file)
        {
            if (file == null) return null;
            using (MemoryStream ms = new())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string? ToString(this IFormFile file, Encoding? encoding = null)
        {
            byte[]? data = file.ToArray();
            if (data == null) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetString(data);
        }

        #endregion
    }
}