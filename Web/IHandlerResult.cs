using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SP.StudioCore.Http;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace SP.StudioCore.Web
{
    public abstract class IHandlerResult : IDisposable
    {
        protected HttpContext context;

        public IHandlerResult(HttpContext context)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            context.SetItem(sw);

            this.context = context;
        }

        #region =========== 反射用的缓存对象 ==============

        /// <summary>
        /// 资源文件
        /// </summary>
        private static Dictionary<string, Assembly> assembly = new Dictionary<string, Assembly>();

        /// <summary>
        /// 处理类库缓存
        /// </summary>
        private static Dictionary<string, Type> handler = new Dictionary<string, Type>();

        /// <summary>
        /// 要执行的动作
        /// </summary>
        private static Dictionary<string, MethodInfo> handlerMethod = new Dictionary<string, MethodInfo>();

        #endregion

        /// <summary>
        /// 资源文件前缀
        /// </summary>
        protected abstract string AssemblyName { get; }

        /// <summary>
        /// 获取资源文件
        /// </summary>
        /// <returns></returns>
        protected virtual Assembly GetAssembly(IEnumerable<string> path)
        {
            string assName = $"{this.AssemblyName}.{path.FirstOrDefault()}.dll";
            Assembly ass = null;
            if (assembly.ContainsKey(assName))
            {
                ass = assembly[assName];
            }
            else
            {
                string fileName = Directory.GetFiles(AppContext.BaseDirectory, "*.dll").FirstOrDefault(t => t.EndsWith(assName, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new Exception(assName);
                }
                string assFile = $"{fileName}";
                ass = Assembly.LoadFrom(assFile);
                if (ass == null) throw new Exception(assFile);
                assembly.Add(assName, ass);
            }
            return ass;
        }

        protected virtual Type GetHandler(Assembly ass, IEnumerable<string> path)
        {
            Type handlerType = null;
            string handlerName = string.Concat(ass.GetName().Name, ".", string.Join(".", path.Skip(1).Take(path.Count() - 2)));
            if (handler.ContainsKey(handlerName))
            {
                handlerType = handler[handlerName];
            }
            else
            {
                handlerType = ass.GetType(handlerName);
                if (handlerType == null) return null;
                handler.Add(handlerName, handlerType);
            }
            return handlerType;
        }

        protected virtual MethodInfo GetMethodInfo(Type handler, string methodName)
        {
            MethodInfo method = null;
            string handlerName = handler.FullName;
            if (handlerMethod.ContainsKey($"{handlerName}.{methodName}"))
            {
                method = handlerMethod[$"{handlerName}.{methodName}"];
            }
            else
            {
                method = handler.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null) return null;
                handlerMethod.Add($"{handlerName}.{methodName}", method);
            }
            return method;
        }

        /// <summary>
        /// 执行逻辑处理方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected virtual Result Invoke(Type type, MethodInfo method)
        {
            return new Result();
        }

        public virtual Result GetResult()
        {
            IEnumerable<string> path = context.Request.Path.ToString().Split('/').Where(t => !string.IsNullOrEmpty(t));
            if (path.Count() < 3) return context.ShowError(HttpStatusCode.NotFound, context.Request.Path.ToString());

            Assembly ass = this.GetAssembly(path);
            if (ass == null) return context.ShowError(HttpStatusCode.NotFound, $"Assembly is null {string.Join(",", path)}");

            Type handlerType = this.GetHandler(ass, path);
            if (handlerType == null) return context.ShowError(HttpStatusCode.NotFound, $"Type Is Null {ass.FullName} {path}");

            MethodInfo method = this.GetMethodInfo(handlerType, path.Last());
            if (method == null) return context.ShowError(HttpStatusCode.MethodNotAllowed, $"Method is Null {handlerType.FullName} {path.Last()}");

            Result result = this.Invoke(handlerType, method);
            if (!result) return result;

            HandlerBase handlerObj = null;
            try
            {
                handlerObj = (HandlerBase)Activator.CreateInstance(handlerType);
                return (Result)method.Invoke(handlerObj, null);
            }
            catch (Exception ex)
            {
                throw ex;
                // 错误日志记录
                // return context.ShowError(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (handlerObj != null) handlerObj.Dispose();
            }
        }

        public void Dispose()
        {
            Stopwatch sw = this.context.GetItem<Stopwatch>();
            if (sw != null) sw.Stop();
        }
    }
}
