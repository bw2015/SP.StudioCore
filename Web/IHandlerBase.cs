using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SP.StudioCore.Array;
using SP.StudioCore.Http;
using SP.StudioCore.Json;
using SP.StudioCore.Linq;
using SP.StudioCore.Model;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SP.StudioCore.Web
{
    /// <summary>
    /// API输出的基类（只有方法，未继承 IHttpHandler)
    /// </summary>
    public abstract class HandlerBase : IDisposable
    {
        protected HttpContext context;

        private Stopwatch sw;

        public HandlerBase()
        {
            sw = this.context.GetItem<Stopwatch>();
        }

        private Dictionary<string, DbContext> _dbContext;

        protected virtual T DbContext<T>() where T : DbContext, new()
        {
            if (_dbContext == null) _dbContext = new Dictionary<string, DbContext>();
            if (_dbContext.ContainsKey(typeof(T).FullName)) return (T)_dbContext[typeof(T).FullName];
            T t = new T();
            _dbContext.Add(typeof(T).FullName, t);
            return t;
        }

        /// <summary>
        /// 自定义链接字符串的数据库操作上下文对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        protected virtual T DbContext<T>(string connection) where T : DbContext, ISplitDbContext, new()
        {
            if (_dbContext == null) _dbContext = new Dictionary<string, DbContext>();
            if (_dbContext.ContainsKey(connection)) return (T)_dbContext[connection];
            T t = new T();
            t.SetDataConnection(connection);
            _dbContext.Add(typeof(T).FullName, t);
            return t;
        }



        /// <summary>
        /// 本次任务的执行时间
        /// </summary>
        /// <returns></returns>
        protected virtual string StopwatchMessage()
        {
            return string.Concat(sw.ElapsedMilliseconds, "ms");
        }
        protected virtual string QF(string name)
        {
            return this.context.QF(name);
        }

        protected virtual T QF<T>(string name, T defaultValue)
        {
            return this.context.QF<T>(name, defaultValue);
        }

        protected virtual int PageIndex
        {
            get
            {
                if (!this.context.Request.HasFormContentType)
                {
                    Dictionary<string, string> data = this.context.GetJson<Dictionary<string, string>>();
                    if (data == null || !data.ContainsKey("PageIndex")) return 1;
                    return data.Get("PageIndex", "1").GetValue<int>();
                }
                return this.QF("PageIndex", 1);
            }
        }

        protected virtual int PageSize
        {
            get
            {
                if (!this.context.Request.HasFormContentType)
                {
                    Dictionary<string, string> data = this.context.GetJson<Dictionary<string, string>>();
                    if (data == null || !data.ContainsKey("PageSize")) return 20;
                    return data.Get("PageSize", "20").GetValue<int>();
                }
                return this.QF("PageSize", 20);
            }
        }


        /// <summary>
        /// 返回对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Result GetResult(object data)
        {
            return new Result(true, string.Concat(sw.ElapsedMilliseconds, "ms"), data);
        }

        /// <summary>
        /// 返回bool值
        /// </summary>
        /// <param name="success"></param>
        /// <param name="successMessage"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual Result GetResult(bool success, string successMessage = "处理成功", object info = null)
        {
            if (success) return new Result(success, successMessage, info);
            string message = this.context.RequestServices.GetService<MessageResult>();
            if (string.IsNullOrEmpty(message)) message = "发生不可描述的错误";
            return new Result(false, message);
        }

        /// <summary>
        /// 返回数组
        /// </summary>
        protected virtual Result GetResult<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            string resultData = this.ShowResult(list, converter, data);
            return this.GetResult(resultData);
        }

        /// <summary>
        /// 返回排序数组（自带分页）
        /// </summary>
        protected virtual Result GetResult<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            string resultData = this.ShowResult(list, converter, data);
            return this.GetResult(resultData);
        }

        protected virtual string ShowResult<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;

            if (typeof(TOutput) == typeof(string))
            {
                result = string.Concat("[", string.Join(",", list.Select(t => converter(t))), "]");
            }
            else
            {
                result = list.ToList().ConvertAll(converter).ToJson();
            }
            sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                .AppendFormat("\"data\":{0},", data == null ? "null" : data.ToJson())
                .AppendFormat("\"list\":{0}", result)
                .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 分页输出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="queryList"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual string ShowResult<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput>? converter = null, object? data = null) where TOutput : class
        {
            if (converter == null)
            {
                converter = input =>
                {
                    TOutput? output = input as TOutput;
                    if (output == null) throw new NullReferenceException();
                    return output;
                };
            }
            StringBuilder sb = new StringBuilder();
            string? json = null;
            IEnumerable<T> query;
            int pageIndex = this.PageIndex;
            int pageSize = this.PageSize;

            int recordCount = list.Count();
            if (pageIndex == 1)
            {
                query = list.Take(pageSize);
            }
            else
            {
                query = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            if (converter == null)
            {
                json = query.AsEnumerable().ToJson();
            }
            else
            {
                json = query.AsEnumerable().Select(converter).ToJson();
            }
            sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", recordCount)
                .AppendFormat("\"PageIndex\":{0},", pageIndex)
                .AppendFormat("\"PageSize\":{0},", pageSize)
                .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
                .AppendFormat(",\"list\":{0}", json)
                .Append("}");

            return sb.ToString();
        }

        public void Dispose()
        {
            if (_dbContext == null) return;
            foreach (DbContext item in _dbContext.Values)
            {
                item.Dispose();

            }
        }
    }
}
