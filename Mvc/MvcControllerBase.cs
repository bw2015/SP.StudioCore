using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.Http;
using System.Diagnostics;
using SP.StudioCore.Enums;
using System.Threading.Tasks;
using SP.StudioCore.Model;
using System.Linq;
using SP.StudioCore.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using SP.StudioCore.Ioc;
using SP.StudioCore.Data.Repository;
using Nest;
using Result = SP.StudioCore.Model.Result;
using Language = SP.StudioCore.Enums.Language;
using System.Linq.Expressions;
using Simple.Elasticsearch;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// MVC 控制层基类
    /// </summary>
    public abstract class MvcControllerBase : ControllerBase
    {
        /// <summary>
        /// 只读数据库
        /// </summary>
        protected virtual IReadRepository ReadDB => IocCollection.GetService<IReadRepository>();

        /// <summary>
        /// 可读/可写数据库（不建议在Controller进行可写操作）
        /// </summary>
        protected virtual IWriteRepository WriteDB => IocCollection.GetService<IWriteRepository>();

        /// <summary>
        /// ES连接对象
        /// </summary>
        protected virtual IElasticClient ESDB => IocCollection.GetService<IElasticClient>();

        private Stopwatch Stopwatch { get; }

        public MvcControllerBase()
        {
            this.Stopwatch = this.context.GetItem<Stopwatch>();
            if (Stopwatch == null)
            {
                this.Stopwatch = new Stopwatch();
                this.Stopwatch.Start();
            }
        }

        /// <summary>
        /// 本次任务的执行时间
        /// </summary>
        /// <returns></returns> 
        protected virtual string StopwatchMessage()
        {
            Stopwatch.Stop();
            return string.Concat(Stopwatch.ElapsedMilliseconds, "ms");
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
                return this.context.QF("PageIndex", 1);
            }
        }

        protected virtual int PageSize
        {
            get
            {
                return this.context.QF("PageSize", 20);
            }
        }

        /// <summary>
        /// 当前HTTP请求的上下文对象
        /// </summary>
        protected virtual HttpContext context
        {
            get
            {
                return this.HttpContext;
            }
        }

        /// <summary>
        /// 获取语种（仅支持获取简体中文、繁体中文、英文：其他语种默认英文）
        /// </summary>
        protected virtual Language Language
        {
            get
            {
                Language lanuage = this.context.GetLanguage();
                if (lanuage == Language.CHN || lanuage == Language.THN)
                {
                    return lanuage;
                }
                else
                {
                    return Language.ENG;
                }
            }
        }

        /// <summary>
        /// 获取语种（仅支持获取简体中文、繁体中文、英文：其他语种默认英文）
        /// </summary>
        protected virtual PlatformType Platform => this.context.GetPlatform();

        #region ========  公共Result输出 （Task输出）  ========

        /// <summary>
        /// 返回对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Obsolete("被 GetContent 取代")]
        protected virtual Task GetResult(object data)
        {
            return new Result(true, this.StopwatchMessage(), data).WriteAsync(this.context);
        }

        [Obsolete("被 GetContent 取代")]
        protected virtual Task GetResult(ContentType type, object data)
        {
            return new Result(type, data).WriteAsync(this.context);
        }

        /// <summary>
        /// 返回bool值
        /// </summary>
        /// <param name="success"></param>
        /// <param name="successMessage"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        [Obsolete("被 GetContent 取代")]
        protected virtual Task GetResult(bool success, string successMessage = "处理成功", object info = null)
        {
            if (success) return new Result(success, successMessage, info).WriteAsync(HttpContext);
            string message = this.context.RequestServices.GetService<MessageResult>();
            if (string.IsNullOrEmpty(message)) message = "发生不可描述的错误";
            return new Result(false, message).WriteAsync(HttpContext);
        }

        /// <summary>
        /// 返回数组
        /// </summary>
        [Obsolete("被 GetContent 取代")]
        protected virtual Task GetResult<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null) where TOutput : class
        {
            string resultData = this.ShowResult(list, converter, data);
            return this.GetResult(resultData);
        }

        [Obsolete("被 ShowContent 取代")]
        protected virtual string ShowResult<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null)
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;

            if (typeof(TOutput) == typeof(string))
            {
                if (converter == null)
                {
                    result = string.Concat("[", string.Join(",", list.Select(t => t)), "]");
                }
                else
                {
                    result = string.Concat("[", string.Join(",", list.Select(t => converter(t))), "]");
                }
            }
            else
            {
                if (converter == null)
                {
                    result = list.ToJson();
                }
                else
                {
                    result = list.ToList().ConvertAll(converter).ToJson();
                }
            }
            _ = sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                  .AppendFormat("\"data\":{0},", data == null ? "null" : data.ToJson())
                  .AppendFormat("\"list\":{0}", result)
                .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 返回排序数组（自带分页）
        /// </summary>
        [Obsolete("被 GetContent 取代")]
        protected virtual Task GetResult<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput> converter = null, object data = null, Action<IEnumerable<T>> action = null) where TOutput : class
        {
            string resultData = this.ShowResult(list, converter, data, action);
            return this.GetResult(resultData);
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
        /// <param name="action">分页数据的前置处理</param>
        /// <returns></returns>
        [Obsolete("被 ShowContent 取代")]
        protected virtual string ShowResult<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput> converter = null, object data = null, Action<IEnumerable<T>> action = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string json = null;
            IEnumerable<T> query;
            if (this.PageIndex == 1)
            {
                query = list.Take(this.PageSize).ToArray();
            }
            else
            {
                query = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToArray();
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
            _ = sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                .AppendFormat("\"PageIndex\":{0},", this.PageIndex)
                .AppendFormat("\"PageSize\":{0},", this.PageSize)
                .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
                .AppendFormat(",\"list\":{0}", json)
                .Append("}");

            return sb.ToString();
        }

        /// <summary>
        /// 输出一条错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Obsolete("被 ShowErrorContent 取代")]
        protected virtual Task ShowError(string message)
        {
            return new Result(false, message).WriteAsync(HttpContext);
        }

        [Obsolete("被 ShowErrorContent 取代")]
        protected virtual Task ShowError(string message, object info)
        {
            return new Result(false, message, info).WriteAsync(HttpContext);
        }

        #endregion

        #region ========  使用Result输出（使用Content命名）  ========

        /// <summary>
        /// 输出一个成功的JSON数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Result GetResultContent(object data)
        {
            return new Result(true, this.StopwatchMessage(), data);
        }

        /// <summary>
        /// 返回一个自定义类型的内容
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Result GetResultContent(ContentType type, object data)
        {
            return new Result(type, data);
        }

        /// <summary>
        /// 自动判断成功状态的输出
        /// </summary>
        /// <param name="success"></param>
        /// <param name="successMessage"></param>
        /// <param name="info">如果状态为成功需要输出的对象</param>
        /// <returns></returns>
        protected virtual Result GetResultContent(bool success, string successMessage = "处理成功", object info = null)
        {
            if (success) return new Result(success, successMessage, info);
            string? message = this.context.RequestServices.GetService<MessageResult>();
            if (string.IsNullOrEmpty(message)) message = "发生不可描述的错误";
            return new Result(false, message);
        }

        /// <summary>
        /// 输出一个无分页的列表内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput">转换方法</typeparam>
        /// <param name="list"></param>
        /// <param name="converter"></param>
        /// <param name="data">附带输出内容</param>
        /// <returns></returns>
        protected virtual string GetResultContent<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null)
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;

            if (typeof(TOutput) == typeof(string))
            {
                if (converter == null)
                {
                    result = string.Concat("[", string.Join(",", list.Select(t => t)), "]");
                }
                else
                {
                    result = string.Concat("[", string.Join(",", list.Select(t => converter(t))), "]");
                }
            }
            else
            {
                if (converter == null)
                {
                    result = list.ToJson();
                }
                else
                {
                    result = list.ToList().ConvertAll(converter).ToJson();
                }
            }
            _ = sb.Append("{")
                .AppendFormat("\"RecordCount\":{0},", list.Count())
                  .AppendFormat("\"data\":{0},", data == null ? "null" : data.ToJson())
                  .AppendFormat("\"list\":{0}", result)
                .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 返回一个无分页的输出对象
        /// 包含 RecordCount / list / data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual Result GetResultList<T, TOutput>(IEnumerable<T> list, Converter<T, TOutput> converter = null, Object data = null)
        {
            string resultData = this.GetResultContent(list, converter, data);
            return this.GetResultContent(resultData);
        }
        /// <summary>
        /// ES对象分页返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="response"></param>
        /// <param name="convert"></param>
        /// <param name="data"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected virtual Result GetResultList<T, TOutput>(Func<SearchDescriptor<T>, ISearchRequest> search, Func<T, TOutput> convert = null, object data = null) where T : class, IDocument where TOutput : class
        {
            if (convert == null) convert = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string? json = null;
            Func<SearchDescriptor<T>, ISearchRequest> action = (s) =>
            {
                return search.Invoke(s.Paged(this.PageIndex, this.PageSize));
            };

            ISearchResponse<T> response = ESDB.Search(action);
            if (!response.IsValid)
            {
                throw new Exception(response.ServerError.ToString());
            }
            if (convert == null)
            {
                json = response.Documents.ToJson();
            }
            else
            {
                json = response.Documents.Select(convert).ToJson();
            }
            sb.Append("{")
            .AppendFormat("\"RecordCount\":{0},", response.Total == -1 ? 0 : response.Total)
            .AppendFormat("\"PageIndex\":{0},", this.PageIndex)
            .AppendFormat("\"PageSize\":{0},", this.PageSize)
            .AppendFormat("\"data\":{0}", data == null ? "null" : data.ToJson())
            .AppendFormat(",\"list\":{0}", json)
            .Append("}");
            return GetResultContent(sb.ToString());
        }
        /// <summary>
        /// 分页输出（指定分页参数）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="pageindex"></param>
        /// <param name="pagesize"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected virtual Result GetResultList<T, TOutput>(IOrderedQueryable<T> list, int pageindex, int pagesize, Func<T, TOutput> converter = null, object data = null, Action<IEnumerable<T>> action = null) where TOutput : class
        {
            if (converter == null) converter = t => t as TOutput;
            StringBuilder sb = new StringBuilder();
            string? json = null;
            IEnumerable<T> query;
            if (pageindex == 1)
            {
                query = list.Take(pagesize).ToArray();
            }
            else
            {
                query = list.Skip((pageindex - 1) * pagesize).Take(pagesize).ToArray();
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

        /// <summary>
        /// 生成分页输出
        /// 包含 RecordCount \ PageIndex \ PageSize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <param name="action">提前对分页内内容的处理方法</param>
        /// <returns></returns>
        protected virtual string GetResultContent<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput>? converter = null, object? data = null, Action<IEnumerable<T>>? action = null) where TOutput : class
        {
            if (converter == null)
            {
                converter = t =>  t as TOutput;
            }
            string json = string.Empty;
            IEnumerable<T> query;
            int recordCount = list.Count();
            if (this.PageIndex == 1)
            {
                query = list.Take(this.PageSize).ToArray();
            }
            else
            {
                query = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).ToArray();
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
            return string.Concat("{",
                $"\"RecordCount\":{ recordCount },",
                $"\"PageIndex\":{this.PageIndex},",
                $"\"PageSize\":{this.PageSize},",
                $"\"data\":{ (data == null ? "null" : data.ToJson()) },",
                $"\"list\":{json}",
                "}");
        }

        /// <summary>
        /// 返回排序数组（自带分页）
        /// 包含 RecordCount \ PageIndex \ PageSize
        /// </summary>
        /// <param name="action">提前对分页内内容的处理方法</param>
        /// <returns>返回内容JSON</returns>
        protected virtual Result GetResultList<T, TOutput>(IOrderedQueryable<T> list, Func<T, TOutput>? converter = null, object? data = null, Action<IEnumerable<T>>? action = null) where TOutput : class
        {
            string resultData = this.GetResultContent(list, converter, data, action);
            return this.GetResultContent(resultData);
        }


        /// <summary>
        /// 数据库内找出主键，然后去缓存内找出实体类的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="list"></param>
        /// <param name="field"></param>
        /// <param name="convert">批量查找的缓存方法</param>
        /// <returns></returns>
        protected virtual Result GetResultList<T, TKey, TOutput>(IOrderedQueryable<T> list, Expression<Func<T, TKey>> selector, Func<IEnumerable<TKey>, IEnumerable<TOutput>> convert)
        {
            int recordCount = list.Count();
            IQueryable<TKey> query;
            if (this.PageIndex == 1)
            {
                query = list.Take(this.PageSize).Select(selector);
            }
            else
            {
                query = list.Skip((this.PageIndex - 1) * this.PageSize).Take(this.PageSize).Select(selector);
            }

            string json = convert(query).ToJson();

            return GetResultContent(string.Concat("{",
               $"\"RecordCount\":{ recordCount },",
               $"\"PageIndex\":{this.PageIndex},",
               $"\"PageSize\":{this.PageSize},",
               $"\"list\":{json}",
               "}"));
        }

        /// <summary>
        /// 输出一条错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual Result GetResultError(string message)
        {
            return new Result(false, message);
        }

        /// <summary>
        /// 输出一个附带内容的错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual Result GetResultError(string message, object info)
        {
            return new Result(false, message, info);
        }

        #endregion
    }
}
