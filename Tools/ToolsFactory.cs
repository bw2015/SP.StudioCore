using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SP.StudioCore.Http;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public static class ToolsFactory
    {
        internal static Dictionary<string, WebSocketHandlerBase> WebSocketHandlerCache = new();

        /// <summary>
        /// 执行工具包内方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static Result Invote(HttpContext context)
        {
            string path = context.Request.Path.ToString();
            if (Regex.IsMatch(path, @"^/\w+$", RegexOptions.IgnoreCase))
            {
                path += "/Index";
            }
            Regex regex = new(@"^/(?<Controller>\w+)/(?<Method>[\w\.]+)$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(path))
            {
                return context.ShowError(HttpStatusCode.MethodNotAllowed, path);
            }
            string controller = regex.Match(path).Groups["Controller"].Value;
            string methodName = regex.Match(path).Groups["Method"].Value;

            Assembly assembly = Assembly.Load($"Tools.{controller}");
            if (assembly == null) return context.ShowError(HttpStatusCode.BadRequest, controller);

            Result? staticFile = GetStaticFile(assembly, path);
            if (staticFile.HasValue)
            {
                return staticFile.Value;
            }

            Type start = assembly.GetTypes().FirstOrDefault(t => t.IsBaseType<StartBase>());
            if (start == null) return context.ShowError(HttpStatusCode.MethodNotAllowed, $"Tools.{controller}.Start");

            MethodInfo methodInfo = start.GetMethod(methodName);
            if (methodInfo == null) return context.ShowError(HttpStatusCode.NotFound, methodName);

            // 得到动作的参数
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object obj = Activator.CreateInstance(start, new object[] { context });

            if (context.Request.Method == "GET")
            {
                return (Result)methodInfo.Invoke(obj, parameters.Select(t => context.QS(t.Name).GetValue(t.ParameterType)).ToArray());
            }
            else if (context.Request.Method == "POST")
            {
                if (parameters.Length == 0)
                {
                    return (Result)methodInfo.Invoke(obj, null);
                }
                else if (parameters.Length == 1 && parameters[0].HasAttribute<FromBodyAttribute>())
                {
                    Type parameterType = parameters[0].ParameterType;
                    string value = context.GetString();
                    object parameterValue = JsonConvert.DeserializeObject(value, parameterType);
                    return (Result)methodInfo.Invoke(obj, new[] { parameterValue });
                }
                else
                {
                    return (Result)methodInfo.Invoke(obj, parameters.Select(t => context.QF(t.Name).GetValue(t.ParameterType)).ToArray());
                }
            }

            return context.ShowError(HttpStatusCode.BadRequest, $"{path},{  string.Join(",", parameters.Select(t => t.ParameterType.Name)) }");
        }

        /// <summary>
        /// 从资源文件中读取静态文件内容
        /// </summary>
        /// <returns></returns>
        internal static Result? GetStaticFile(Assembly assembly, string path)
        {
            Regex staticRegex = new Regex(@"\.html$|\.htm$|\.css$|\.js$", RegexOptions.IgnoreCase);
            if (!staticRegex.IsMatch(path)) return null;

            string resourceName = path.Substring(path.LastIndexOf('/') + 1);
            resourceName = resourceName.Substring(0, resourceName.IndexOf('.'));

            Type? resourceType = assembly.GetType($"{assembly.GetName().Name}.Properties.Resources");
            if (resourceType == null) return null;
            ResourceManager rm = new ResourceManager(resourceType);
            if (rm == null) return null;
            object? resource = rm.GetObject(resourceName);
            if (resource == null) return null;

            string extend = staticRegex.Match(path).Value;
            ContentType contentType = extend switch
            {
                ".html" => ContentType.HTML,
                ".htm" => ContentType.HTML,
                "css" => ContentType.CSS,
                "js" => ContentType.JS,
                _ => ContentType.TEXT
            };
            return new Result(contentType, resource);
        }


        internal static WebSocketHandlerBase GetWebSocket(HttpContext context)
        {
            string path = context.Request.Path.ToString();
            Regex regex = new(@"^/(?<Controller>\w+)", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(path))
            {
                return null;
            }
            string controller = regex.Match(path).Groups["Controller"].Value;
            string assemblyName = $"Tools.{controller}";
            if (WebSocketHandlerCache.ContainsKey(assemblyName)) return WebSocketHandlerCache[assemblyName];

            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly == null) return null;
            Type handlerType = assembly.GetType($"{assemblyName}.WebSocketHandler");
            if (handlerType == null) return null;

            WebSocketHandlerBase ws = (WebSocketHandlerBase)Activator.CreateInstance(handlerType);
            WebSocketHandlerCache.Add(assemblyName, ws);
            return ws;
        }
    }
}
