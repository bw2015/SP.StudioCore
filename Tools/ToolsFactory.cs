using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SP.StudioCore.Http;
using SP.StudioCore.Ioc;
using SP.StudioCore.Model;
using SP.StudioCore.Mvc;
using SP.StudioCore.Mvc.Exceptions;
using SP.StudioCore.Mvc.Services;
using SP.StudioCore.Protobufs;
using SP.StudioCore.Types;
using SP.StudioCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// 资源缓存
        /// </summary>
        private static ConcurrentDictionary<string, Assembly?> _assembly = new();
        private static ConcurrentDictionary<string, Type?> _start = new();
        private static ConcurrentDictionary<string, MethodInfo?> _method = new();
        private static ConcurrentDictionary<string, ParameterInfo[]?> _parameter = new();

        private static IPermission? permission => IocCollection.GetService<IPermission>();

        /// <summary>
        /// 执行工具包内方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Result Invote(HttpContext context)
        {
            //logs.Add(context.GetString(), ConsoleColor.Blue);
            //foreach (var item in context.Request.Headers)
            //{
            //    logs.Add($"   {item.Key} => {item.Value}", ConsoleColor.DarkBlue);
            //}

            //return new Result();

            Stopwatch sw = Stopwatch.StartNew();
            List<string> logs = new List<string>();
            try
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
                string controller = regex.Match(path).Groups["Controller"].Value,
                    methodName = regex.Match(path).Groups["Method"].Value,
                    assemblyName = $"Tools.{controller}".ToLower();

                if (!_assembly.TryGetValue(assemblyName, out Assembly? assembly))
                {
                    _assembly.TryAdd(assemblyName, assembly = Assembly.Load(assemblyName));
                }
                logs.Add($"资源加载完毕，耗时：{sw.ElapsedMilliseconds}ms");

                if (assembly == null) return context.ShowError(HttpStatusCode.BadRequest, controller);

                Result? staticFile = GetStaticFile(assembly, path);
                if (staticFile.HasValue)
                {
                    return staticFile.Value;
                }

                if (!_start.TryGetValue($"{assembly.FullName}:Startup", out Type? start))
                {
                    _start.TryAdd($"{assembly.FullName}:Startup", start = assembly.GetTypes().FirstOrDefault(t => t.IsBaseType<StartBase>()));
                }
                logs.Add($"查找启动类，耗时：{sw.ElapsedMilliseconds}ms"); ;

                if (start == null) return context.ShowError(HttpStatusCode.MethodNotAllowed, $"Tools.{controller}.Start");

                if (!_method.TryGetValue($"{assembly.FullName}:{methodName}", out MethodInfo? methodInfo))
                {
                    _method.TryAdd($"{assembly.FullName}:{methodName}", methodInfo = start.GetMethod(methodName));
                }
                if (methodInfo == null) return context.ShowError(HttpStatusCode.NotFound, $"{start.FullName}.{methodName}");

                logs.Add($"查找动作，耗时：{sw.ElapsedMilliseconds}ms");

                // 检查是否需要管理员登录&权限检查
                if (methodInfo.HasAttribute<PermissionAttribute>())
                {
                    if (permission == null) throw new ResultException("未实现IPermission");
                    if (!permission.CheckPermission(context, methodInfo.GetAttribute<PermissionAttribute>()))
                    {
                        throw new ResultException("没有权限");
                    }
                }

                // 得到动作的参数
                if (!_parameter.TryGetValue($"{assembly.FullName}:{methodName}", out ParameterInfo[]? parameters))
                {
                    _parameter.TryAdd($"{assembly.FullName}:{methodName}", parameters = methodInfo.GetParameters());
                }
                if (parameters == null) parameters = System.Array.Empty<ParameterInfo>();

                logs.Add($"获取参数，耗时：{sw.ElapsedMilliseconds}ms"); ;

                // 创建实例
                object? obj = Activator.CreateInstance(start, new object[] { context });
                if (obj == null) return context.ShowError(HttpStatusCode.NotFound, start.FullName);

                logs.Add($"创建实例，耗时：{sw.ElapsedMilliseconds}ms"); ;

                bool isGetMethod = methodInfo.HasAttribute<HttpGetAttribute>();
                bool isPostMethod = methodInfo.HasAttribute<HttpPostAttribute>();

                Result? result = (Result?)methodInfo.Invoke(obj, context.GetParameterValue(parameters));
                logs.Add($"执行完毕，耗时：{sw.ElapsedMilliseconds}ms"); ;

                if (result.HasValue) return result.Value;

                return context.ShowError(HttpStatusCode.InternalServerError, $"{path},{  string.Join(",", parameters.Select(t => t.ParameterType.Name)) }");
            }
            finally
            {
                //ConsoleHelper.WriteLine(string.Join("\n\r", logs), ConsoleColor.Green);
            }
        }

        private static object? GetParameterValue(this HttpContext context, ParameterInfo parameter)
        {
            string? name = parameter.Name;
            if (name == null) return null;
            object? value;
            if (parameter.HasAttribute<FromQueryAttribute>())
            {
                value = context.QS(name).GetValue(parameter.ParameterType);
            }
            else if (parameter.HasAttribute<FromFormAttribute>())
            {
                value = context.QF(name).GetValue(parameter.ParameterType);
            }
            else if (parameter.HasAttribute<FromBodyAttribute>())
            {
                value = JsonConvert.DeserializeObject(context.GetString() ?? string.Empty, parameter.ParameterType);
            }
            else if (parameter.HasAttribute<FromJsonAttribute>())
            {
                value = JsonConvert.DeserializeObject(context.QF(name) ?? String.Empty, parameter.ParameterType);
            }
            else if (parameter.HasAttribute<FromProtobufAttribute>())
            {
                value = context.GetData()?.ToObject(parameter.ParameterType);
            }
            else
            {
                value = context.GetParam(name).GetValue(parameter.ParameterType);
            }
            return value;
        }

        /// <summary>
        /// 获取参数返回值
        /// </summary>
        private static object?[]? GetParameterValue(this HttpContext context, ParameterInfo[] parameters)
        {
            if (parameters.Length == 0) return null;
            List<object?> list = new List<object?>();
            foreach (ParameterInfo parameter in parameters)
            {
                list.Add(context.GetParameterValue(parameter));
            }
            return list.ToArray();
        }

        private static ConcurrentDictionary<string, Result?> _staticFiles = new();
        /// <summary>
        /// 从资源文件中读取静态文件内容
        /// </summary>
        /// <returns></returns>
        private static Result? GetStaticFile(Assembly assembly, string path)
        {
            Regex staticRegex = new Regex(@"\.html$|\.htm$|\.css$|\.js$", RegexOptions.IgnoreCase);
            if (!staticRegex.IsMatch(path)) return null;

            string key = $"{assembly.FullName}:{path}";
            if (_staticFiles.ContainsKey(key)) return _staticFiles[key];

            Result? result = null;
            try
            {
                string resourceName = path[(path.LastIndexOf('/') + 1)..];
                resourceName = resourceName[..resourceName.IndexOf('.')];

                Type? resourceType = assembly.GetType($"{assembly.GetName().Name}.Properties.Resources");
                if (resourceType == null) return null;
                ResourceManager rm = new ResourceManager(resourceType);
                if (rm == null) return result = null;
                object? resource = rm.GetObject(resourceName);
                if (resource == null) return result = null;

                string extend = staticRegex.Match(path).Value;
                ContentType contentType = extend switch
                {
                    ".html" => ContentType.HTML,
                    ".htm" => ContentType.HTML,
                    "css" => ContentType.CSS,
                    "js" => ContentType.JS,
                    _ => ContentType.TEXT
                };
                return result = new Result(contentType, resource);
            }
            finally
            {
                _staticFiles.TryAdd(key, result);
            }
        }


        internal static WebSocketHandlerBase? GetWebSocket(HttpContext context)
        {
            string path = context.Request.Path.ToString();
            Regex regex = new(@"^/(?<Controller>\w+)", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(path))
            {
                return null;
            }
            string controller = regex.Match(path).Groups["Controller"].Value,
                assemblyName = $"Tools.{controller}".ToLower();
            if(WebSocketHandlerCache.ContainsKey(assemblyName)) return WebSocketHandlerCache[assemblyName];

            if (!_assembly.TryGetValue(assemblyName, out Assembly? assembly))
            {
                _assembly.TryAdd(assemblyName, assembly = Assembly.Load(assemblyName));
            }
            if (assembly == null) return null;
            Type? handlerType = assembly.GetType($"{assemblyName}.WebSocketHandler", false, true);
            if (handlerType == null) return null;

            WebSocketHandlerBase? ws = (WebSocketHandlerBase?)Activator.CreateInstance(handlerType);
            if (ws == null) return null;
            WebSocketHandlerCache.Add(assemblyName, ws);
            return ws;
        }
    }
}
