using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SP.StudioCore.Http;
using SP.StudioCore.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SP.StudioCore.Utils
{
    /// <summary>
    /// 錯誤處理
    /// </summary>
    public static class ErrorHelper
    {
        /// <summary>
        /// 獲取詳細的錯誤信息（字典格式）
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetException(Exception ex, HttpContext? context = null)
        {
            Dictionary<string, object> data = new()
            {
                { "Message", ex.Message },
                { "Time", DateTime.Now }
            };

            if (context != null)
            {
                HttpRequest? request = context.Request;
                IPAddress? iPAddress = context.Connection.RemoteIpAddress;
                Dictionary<string, object> RequestData = new Dictionary<string, object>
                {
                    { "RawUrl", request.Host + request.Path },
                    { "IP", iPAddress == null ? string.Empty : iPAddress.ToString() },
                    { "Method", request.Method },
                    { "Headers", request.Headers.Keys.ToDictionary(t => t, t => request.Headers[t].ToString()) }
                };
                if (request.Method == "POST")
                {
                    try
                    {
                        if (context.Request.HasFormContentType)
                        {
                            RequestData.Add("PostData", context.Request.Form.Keys.ToDictionary(t => t, t => context.Request.Form[t].ToString()));
                        }
                        else
                        {
                            RequestData.Add("PostData", context.GetString());
                        }
                    }
                    catch (Exception dataException)
                    {
                        RequestData.Add("PostData", dataException.Message);
                    }
                }
                data.Add("HttpContext", RequestData);
            }
            if (ex != null)
            {
                var Exception = new Dictionary<string, object>()
                {
                    {"Type",ex.GetType().FullName ?? string.Empty },
                    {"Source",ex.Source??string.Empty },
                    {"StackTrace",ex.StackTrace?.Split('\n') ?? System.Array.Empty<string>() }
                };
                if (ex.TargetSite != null)
                {
                    Exception.Add("TargetSite", new
                    {
                        Method = ex.TargetSite.Name,
                        Class = ex.TargetSite.DeclaringType?.FullName
                    });
                }
                Dictionary<object, object> exData = new Dictionary<object, object>();
                foreach (DictionaryEntry item in ex.Data)
                {
                    exData.Add(item.Key, item.Value ?? string.Empty);
                }
                Exception.Add("Data", exData);
                data.Add("Exception", Exception);
            }

            return data;
        }
        
        /// <summary>
        /// 獲取詳細的錯誤信息（JSON格式）
        /// </summary>
        public static string GetExceptionContent(Exception ex, HttpContext? context = null)
        {
            return JsonConvert.SerializeObject(GetException(ex, context));
        }
    }
}
