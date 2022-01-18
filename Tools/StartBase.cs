using Microsoft.AspNetCore.Http;
using SP.StudioCore.Http;
using SP.StudioCore.Json;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public abstract class StartBase
    {
        protected HttpContext context;

        protected readonly Stopwatch sw;

        public StartBase(HttpContext context)
        {
            this.context = context;
            sw = new Stopwatch();
            sw.Start();
        }

        /// <summary>
        /// 获取程序执行时长
        /// </summary>
        /// <returns></returns>
        protected string GetTime() => $"{sw.ElapsedMilliseconds}ms";

        protected int PageIndex => this.context.QF("PageIndex", 1);

        protected int PageSize => this.context.QF("PageSize", 20);

        /// <summary>
        /// 输出一个成功的JSON数据
        /// </summary>
        protected virtual Result GetResultContent(object data, string msg = "操作成功")
        {
            return new Result(true, msg, data);
        }

        protected virtual Result GetResultContent(bool success, object data)
        {
            return new Result(success, string.Empty, info: success ? data : null);
        }

        protected virtual Result GetResultList<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput>? converter = null, object? data = null, Action<IEnumerable<T>>? action = null) where TOutput : class
        {
            converter ??= t => t as TOutput;
            StringBuilder sb = new();
            string? json = null;
            IEnumerable<T> query;
            if (this.PageIndex == 1)
            {
                query = list.Take(PageSize).ToArray();
            }
            else
            {
                query = list.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToArray();
            }
            action?.Invoke(query);
            if (converter == null)
            {
                json = query.ToJson();
            }
            else
            {
                if (typeof(TOutput).Name == "String")
                {
                    json = string.Concat("[", string.Join(",", query.Select(converter)), "]");
                }
                else
                {
                    json = query.Select(converter).ToJson();
                }
            }
            return this.GetResultContent(string.Concat("{",
                $"\"RecordCount\":{ list.Count() },",
                $"\"PageIndex\":{this.PageIndex},",
                $"\"PageSize\":{this.PageSize},",
                $"\"data\":{ (data == null ? "null" : data.ToJson()) },",
                $"\"list\":{json}",
                "}"));
        }
    }
}
