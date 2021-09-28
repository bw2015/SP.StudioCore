using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Nest;
using SP.StudioCore.Enums;
using SP.StudioCore.Http;
using SP.StudioCore.Types;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.ElasticSearch
{
    public static class ElasticSearchExtension
    {
        /// <summary>
        /// 索引缓存
        /// </summary>
        private static readonly Dictionary<string, bool> IndexCache = new();
        /// <summary>
        /// 生成更新脚本
        /// </summary>
        /// <param name="desc">ES对象</param>
        /// <param name="entity">要更新的对象</param>
        /// <param name="firstCharToLower">是否首字母小写（默认小写）</param>
        public static UpdateByQueryDescriptor<TEntity> Script<TEntity>(this UpdateByQueryDescriptor<TEntity> desc, object entity, bool firstCharToLower = true) where TEntity : class
        {
            var lstScript = new List<string>();
            var dicValue = new Dictionary<string, object>();

            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                string field = property.GetFieldName();
                // 首字母小写
                if (firstCharToLower)
                {
                    var firstChar = field.Substring(0, 1).ToLower();
                    if (field.Length > 1) field = firstChar + field.Substring(1);
                    else field = firstChar;
                }

                lstScript.Add($"ctx._source.{field}=params.{field}");
                dicValue.Add(field, property.GetValue(entity));
            }

            return desc.Script(s => s.Source(string.Join(';', lstScript)).Params(dicValue));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool Insert<TDocument>(this IElasticClient client, TDocument document, DateTime? indexDateTime = null) where TDocument : class
        {
            ElasticSearchIndexAttribute elasticsearch = typeof(TDocument).GetAttribute<ElasticSearchIndexAttribute>();
            if (elasticsearch == null) throw new ArgumentNullException("缺失ElasticSearchIndex特性");
            //检查是否已经创建索引
            if (indexDateTime.HasValue)
            {
                elasticsearch.SetIndexTime(indexDateTime.Value);
            }
            client.WhenNotExistsAddIndex<TDocument>(elasticsearch);
            return client.Index(new IndexRequest<TDocument>(document, elasticsearch.IndexName)).IsValid;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="documents"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool Insert<TDocument>(this IElasticClient client, IEnumerable<TDocument> documents, DateTime? indexDateTime = null) where TDocument : class
        {
            ElasticSearchIndexAttribute elasticsearch = typeof(TDocument).GetAttribute<ElasticSearchIndexAttribute>();
            if (elasticsearch == null) throw new ArgumentNullException("缺失ElasticSearchIndex特性");
            if (indexDateTime.HasValue)
            {
                elasticsearch.SetIndexTime(indexDateTime.Value);
            }
            //检查是否已经创建索引
            client.WhenNotExistsAddIndex<TDocument>(elasticsearch);
            return client.IndexMany(documents, elasticsearch.IndexName).IsValid;
        }


        //public static bool Update<TDocument>(this IElasticClient client, TDocument entity) where TDocument : class
        //{
        //    string indexname = typeof(TDocument).GetIndexName();
        //    string key = string.Empty;//主键
        //    Dictionary<string, object> param = new Dictionary<string, object>();
        //    List<string> source = new List<string>();
        //    foreach (PropertyInfo property in typeof(TDocument).GetProperties())
        //    {
        //        string field = property.GetFieldName();
        //        if (property.Name == "ID")
        //        {
        //            key = field;
        //            continue;
        //        }
        //        source.Add($"ctx._source.{field}=params.{field}");
        //        param.Add(field, property.GetValue(entity));
        //    }
        //    return client.UpdateByQuery<TDocument>(c => c.Index(indexname).Query(q => q.Term(t => t.Field(key).Value(entity.ID)))
        //                                                 .Script(s => s.Source(string.Join(";", source)).Params(param))).IsValid;
        //}

        //public static bool Delete<TDocument, TKey>(this IElasticClient client, TKey value) where TDocument : class
        //{
        //    string indexname = typeof(TDocument).GetIndexName();
        //    string fieldname = typeof(TDocument).GetProperty("ID").GetFieldName();
        //    return client.DeleteByQuery<TDocument>(c => c.Index(indexname).Query(q => q.Term(t => t.Field(fieldname).Value(value)))).IsValid;
        //}

        //public static bool Delete<TDocument>(this IElasticClient client, Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> selector = null) where TDocument : class
        //{
        //    string indexname = typeof(TDocument).GetIndexName();
        //    Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> action = null;
        //    if (selector == null)
        //    {
        //        action = (s) =>
        //        {
        //            return s.Index(indexname);
        //        };
        //    }
        //    else
        //    {
        //        action = (s) =>
        //        {
        //            return selector.Invoke(s.Index(indexname));
        //        };
        //    }
        //    return client.DeleteByQuery(action).IsValid;
        //}

        /// <summary>
        /// 获取表总记录数
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int Count<TDocument>(this IElasticClient client) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            return (int)client.Count<TDocument>(c => c.Index(indexname)).Count;
        }
        /// <summary>
        /// 根据条件获取表总记录数
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static int Count<TDocument, TValue>(this IElasticClient client, TValue value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            if (value == null) throw new NullReferenceException();
            if (field == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            return (int)client.Count<TDocument>(c => c.Index(indexname).Query(q => q.Term(field, value))).Count;
        }
        /// <summary>
        /// 多条件获取表记录数
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static int Count<TDocument>(this IElasticClient client, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            return (int)client.Count<TDocument>(c => c.Index(indexname).Query(q => q.Bool(b => b.Must(queries)))).Count;
        }
        /// <summary>
        /// 查询表记录数（指定查询条件）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static int Count<TDocument>(this IElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> search) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            if (search == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            Func<QueryContainerDescriptor<TDocument>, QueryContainer>[]? query = null;
            foreach (Delegate del in search.GetInvocationList())
            {
                if (del.Target == null) continue;
                dynamic target = del.Target;
                query = target.queries;
            }
            ICountRequest count(CountDescriptor<TDocument> q)
            {
                if (query == null)
                {
                    return q.Index(indexname);
                }
                else
                {
                    return q.Index(indexname).Query(q => q.Bool(b => b.Must(query)));
                }
            };
            return (int)client.Count<TDocument>(count).Count;
        }
        /// <summary>
        /// 查询表是否存在
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static bool Any<TDocument>(this IElasticClient client, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            return client.Count(queries) > 0;
        }
        /// <summary>
        /// 查询表是否存在（指定查询条件）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static bool Any<TDocument>(this IElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> search) where TDocument : class
        {
            return client.Count(search) > 0;
        }

        /// <summary>
        /// 条件查询表是否存在
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Any<TDocument, TValue>(this IElasticClient client, TValue value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            return client.Count(value, field) > 0;
        }
        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns>没有则为null</returns>
        public static TDocument? FirstOrDefault<TDocument, TValue>(this IElasticClient client, TValue value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            if (value == null) throw new NullReferenceException();
            if (field == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            return client.Search<TDocument>(c => c.Index(indexname).Query(q => q.Term(field, value)).Size(1)).Documents?.FirstOrDefault();
        }
        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns>没有则为null</returns>
        public static TDocument? FirstOrDefault<TDocument, TValue>(this IElasticClient client, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            return client.Search<TDocument>(s => s.Index(indexname).Query(q => q.Bool(b => b.Must(queries))).Size(1)).Documents?.FirstOrDefault();
        }
        /// <summary>
        /// 查询条件（仅拼接查询条件，非真实查询）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Query<TDocument>(this IElasticClient client, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            string indexname = typeof(TDocument).GetIndexName();
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).TrackTotalHits(true).Query(q => q.Bool(b => b.Must(queries)));
            }
            return query;
        }
        /// <summary>
        /// 查询条件与过滤条件
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="musts">查询条件</param>
        /// <param name="must_nots">过滤条件</param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Query<TDocument>(this IElasticClient client, IEnumerable<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> musts, IEnumerable<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> mustnots) where TDocument : class
        {
            string indexname = typeof(TDocument).GetIndexName();
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).TrackTotalHits(true).Query(q => q.Bool(b => b.Must(musts).MustNot(mustnots)));
            }
            return query;
        }
        /// <summary>
        /// 指定索引或别名
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexname"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Query<TDocument>(this IElasticClient client, string indexname, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).TrackTotalHits(true).Query(q => q.Bool(b => b.Must(queries)));
            }
            return query;
        }
        /// <summary>
        /// 查询条件与过滤条件
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexname"></param>
        /// <param name="musts"></param>
        /// <param name="mustnots"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Query<TDocument>(this IElasticClient client, string indexname, IEnumerable<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> musts, IEnumerable<Func<QueryContainerDescriptor<TDocument>, QueryContainer>> mustnots) where TDocument : class
        {
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).TrackTotalHits(true).Query(q => q.Bool(b => b.Must(musts).MustNot(mustnots)));
            }
            return query;
        }

        /// <summary>
        /// 查询（真实查询）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="selector">检索条件</param>
        /// <returns></returns>
        public static List<TDocument> Search<TDocument>(this IElasticClient client, Func<QueryContainerDescriptor<TDocument>, QueryContainer> selector, params Expression<Func<TDocument, object>>[] fields) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).Query(q => q.Bool(b => b.Must(selector))).Select(fields);
            }
            return client.Search((Func<SearchDescriptor<TDocument>, ISearchRequest>)query).Documents.ToList();
        }
        /// <summary>
        /// 查询（真实查询）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="queries">查询条件</param>
        /// <returns></returns>
        public static List<TDocument> Search<TDocument>(this IElasticClient client, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] queries) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).Query(q => q.Bool(b => b.Must(queries)));
            }
            return client.Search((Func<SearchDescriptor<TDocument>, ISearchRequest>)query).Documents.ToList();
        }
        /// <summary>
        /// 查询（真实查询）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="fields">过滤字段</param>
        /// <returns></returns>
        public static List<TDocument> Search<TDocument>(this IElasticClient client, params Expression<Func<TDocument, object>>[] fields) where TDocument : class
        {
            if (client == null) throw new NullReferenceException();
            string indexname = typeof(TDocument).GetIndexName();
            ISearchRequest query(SearchDescriptor<TDocument> q)
            {
                return q.Index(indexname).Select(fields);
            }
            return client.Search((Func<SearchDescriptor<TDocument>, ISearchRequest>)query).Documents.ToList();
        }

        /// <summary>
        /// 匹配一个或者多个值，同OR
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue? value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            query.Term(field, value);
            return query;
        }

        /// <summary>
        /// 匹配一个或者多个值，同OR
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="value"></param>
        /// <param name="script">脚本（查询数组中的字段）</param>
        /// <returns></returns>
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue? value, string script) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            query.Term(script, value);
            return query;
        }

        /// <summary>
        /// 匹配一个或者多个值，同OR
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue[] value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            if (field == null) return query;
            if (value.Length == 0)
            {
                switch (typeof(TValue).Name)
                {
                    case "Int16":
                        value = WebAgent.GetArray<TValue>(short.MinValue.ToString());
                        break;
                    case "Int32":
                        value = WebAgent.GetArray<TValue>(int.MinValue.ToString());
                        break;
                    case "Int64":
                        value = WebAgent.GetArray<TValue>(long.MinValue.ToString());
                        break;
                    case "Byte":
                        value = WebAgent.GetArray<TValue>(byte.MinValue.ToString());
                        break;
                    default:
                        break;
                }
            }
            query.Terms(t => t.Field(field).Terms(value));
            return query;
        }
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue[] value, string script) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            if (script == null) return query;
            if (value.Length == 0)
            {
                switch (typeof(TValue).Name)
                {
                    case "Int16":
                        value = WebAgent.GetArray<TValue>(short.MinValue.ToString());
                        break;
                    case "Int32":
                        value = WebAgent.GetArray<TValue>(int.MinValue.ToString());
                        break;
                    case "Int64":
                        value = WebAgent.GetArray<TValue>(long.MinValue.ToString());
                        break;
                    case "Byte":
                        value = WebAgent.GetArray<TValue>(byte.MinValue.ToString());
                        break;
                    default:
                        break;
                }
            }
            query.Terms(t => t.Field(script).Terms(value));
            return query;
        }
        /// <summary>
        /// 范围查询
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue? value, Expression<Func<TDocument, TValue>> field, ExpressionType type) where TDocument : class where TValue : struct
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            object v = value.Value;
            switch (type)
            {
                case ExpressionType.GreaterThan:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.GreaterThan((DateTime)v).Field(field));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.GreaterThan((long)v).Field(field));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.GreaterThan(Convert.ToDouble(v)).Field(field));
                    }
                    else
                    {
                        query.Range(r => r.GreaterThan((double)v).Field(field));
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.GreaterThanOrEquals((DateTime)v).Field(field));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.GreaterThanOrEquals((long)v).Field(field));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.GreaterThanOrEquals(Convert.ToDouble(v)).Field(field));
                    }
                    else
                    {
                        query.Range(r => r.GreaterThanOrEquals((double)v).Field(field));
                    }
                    break;
                case ExpressionType.LessThan:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.LessThan((DateTime)v).Field(field));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.LessThan((long)v).Field(field));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.LessThan(Convert.ToDouble(v)).Field(field));
                    }
                    else
                    {
                        query.Range(r => r.LessThan((double)v).Field(field));
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.LessThanOrEquals((DateTime)v).Field(field));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.LessThanOrEquals((long)v).Field(field));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.LessThanOrEquals(Convert.ToDouble(v)).Field(field));
                    }
                    else
                    {
                        query.Range(r => r.LessThanOrEquals((double)v).Field(field));
                    }
                    break;
            }
            return query;
        }
        public static QueryContainerDescriptor<TDocument> Where<TDocument, TValue>(this QueryContainerDescriptor<TDocument> query, TValue? value, string script, ExpressionType type) where TDocument : class where TValue : struct
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            if (script == null) return query;
            object v = value.Value;
            switch (type)
            {
                case ExpressionType.GreaterThan:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.GreaterThan((DateTime)v).Field(script));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.GreaterThan((long)v).Field(script));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.GreaterThan(Convert.ToDouble(v)).Field(script));
                    }
                    else
                    {
                        query.Range(r => r.GreaterThan((double)v).Field(script));
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.GreaterThanOrEquals((DateTime)v).Field(script));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.GreaterThanOrEquals((long)v).Field(script));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.GreaterThanOrEquals(Convert.ToDouble(v)).Field(script));
                    }
                    else
                    {
                        query.Range(r => r.GreaterThanOrEquals((double)v).Field(script));
                    }
                    break;
                case ExpressionType.LessThan:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.LessThan((DateTime)v).Field(script));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.LessThan((long)v).Field(script));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.LessThan(Convert.ToDouble(v)).Field(script));
                    }
                    else
                    {
                        query.Range(r => r.LessThan((double)v).Field(script));
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (value.Value is DateTime)
                    {
                        query.DateRange(dr => dr.LessThanOrEquals((DateTime)v).Field(script));
                    }
                    else if (value.Value is Int64)
                    {
                        query.LongRange(r => r.LessThanOrEquals((long)v).Field(script));
                    }
                    else if (value.Value is Decimal)
                    {
                        query.Range(r => r.LessThanOrEquals(Convert.ToDouble(v)).Field(script));
                    }
                    else
                    {
                        query.Range(r => r.LessThanOrEquals((double)v).Field(script));
                    }
                    break;
            }
            return query;
        }
        public static DeleteByQueryDescriptor<TDocument> Where<TDocument, TValue>(this DeleteByQueryDescriptor<TDocument> query, object value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            switch (value.GetType().Name)
            {
                case "Guid":
                case "String":
                    query = query.Query(q => q.Term(c => c.Field(field.GetFieldName() + ".keyword").Value(value)));
                    break;
                default:
                    query = query.Query(q => q.Term(t => t.Field(field.GetFieldName()).Value(value)));
                    break;
            }
            return query;
        }
        public static DeleteByQueryDescriptor<TDocument> Where<TDocument, TValue>(this DeleteByQueryDescriptor<TDocument> query, TValue value, Expression<Func<TDocument, TValue>> field, ExpressionType type) where TDocument : class where TValue : struct
        {
            if (query == null) throw new NullReferenceException();
            object v = value;
            switch (type)
            {
                case ExpressionType.GreaterThan:
                    if (value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.GreaterThan((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.GreaterThan((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.GreaterThanOrEquals((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.GreaterThanOrEquals((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.LessThan:
                    if (value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.LessThan((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.LessThan((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.LessThanOrEquals((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.LessThanOrEquals((double)v).Field(field)));
                    }
                    break;

            }
            return query;
        }
        /// <summary>
        /// 降序
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static SearchDescriptor<TDocument> OrderByDescending<TDocument, TValue>(this SearchDescriptor<TDocument> query, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (query == null) throw new NullReferenceException();
            return query.Sort(c => c.Descending(field));
        }
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> OrderByDescending<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            return (s) =>
            {
                return search.Invoke(s.OrderByDescending(field));
            };
        }
        /// <summary>
        /// 升序
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static SearchDescriptor<TDocument> OrderBy<TDocument, TValue>(this SearchDescriptor<TDocument> query, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (query == null) throw new NullReferenceException();
            return query.Sort(c => c.Ascending(field));
        }
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> OrderBy<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            return (s) =>
            {
                return search.Invoke(s.OrderBy(field));
            };
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static SearchDescriptor<TDocument> Paged<TDocument>(this SearchDescriptor<TDocument> query, int page, int limit) where TDocument : class
        {
            if (query == null) throw new NullReferenceException();
            if (page == 1)
            {
                return query.Size(limit);
            }
            else
            {
                return query.From((page - 1) * limit).Size(limit);
            }
        }
        /// <summary>
        /// 分页，传入分页参数
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Paged<TDocument>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, int page, int limit) where TDocument : class
        {
            return (s) =>
            {
                return search.Invoke(s.Paged(page, limit));
            };
        }
        /// <summary>
        /// 根据单个字段分组
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="search"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> GroupBy<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            return (s) =>
            {
                s.Size(0).Aggregations(GroupBy(field));
                search.Invoke(s);
                return s;
            };
        }
        /// <summary>
        /// 根据字符串字段进行分组
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="search"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> GroupBy<TDocument>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, string field) where TDocument : class
        {
            return (s) =>
            {
                s.Size(0).Aggregations(aggs => aggs.Terms("group_by_script", t => t.Field(field)));
                search.Invoke(s);
                return s;
            };
        }
        /// <summary>
        /// 聚合
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="search"></param>
        /// <param name="script"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> GroupBy<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, string script, params Expression<Func<TDocument, TValue>>[] fields) where TDocument : class
        {
            IAggregationContainer group(AggregationContainerDescriptor<TDocument> aggs)
            {
                string[] group_field = WebAgent.GetArray<string>(script);
                string _script = string.Empty;
                for (int i = 0; i < group_field.Length; i++)
                {
                    _script += "doc['" + group_field[i] + "'].value";
                    if (i + 1 < group_field.Length)
                    {
                        _script += "+'-'+";
                    }
                }
                return aggs.Terms("group_by_script", t => t.Script(_script).Aggregations(GroupBy(fields)));
            };
            return (s) =>
            {
                s.Size(0).Aggregations(group);
                search.Invoke(s);
                return s;
            };
        }

        /// <summary>
        /// 聚合（聚合指定特性Aggregate）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> GroupBy<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, params Expression<Func<TDocument, TValue>>[] fields) where TDocument : class
        {
            return (s) =>
            {
                s.Size(0).Aggregations(GroupBy(fields));
                search.Invoke(s);
                return s;
            };
        }
        /// <summary>
        /// 内部组装聚合条件
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static Func<AggregationContainerDescriptor<TDocument>, IAggregationContainer> GroupBy<TDocument, TValue>(params Expression<Func<TDocument, TValue>>[] fields) where TDocument : class
        {
            return (s) =>
            {
                foreach (var field in fields)
                {
                    PropertyInfo property = field.ToPropertyInfo();
                    AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
                    string fieldname = property.GetFieldName();
                    if (aggregate == null) { continue; }
                    else if (aggregate.Type == AggregateType.Sum)
                    {
                        s.Sum(aggregate.Name ?? fieldname, c => c.Field(field));
                    }
                    else if (aggregate.Type == AggregateType.Average)
                    {
                        s.Average(aggregate.Name ?? fieldname, c => c.Field(field));
                    }
                    else if (aggregate.Type == AggregateType.Count)
                    {
                        s.ValueCount(aggregate.Name ?? fieldname, c => c.Field(field));
                    }
                    else if (aggregate.Type == AggregateType.Max)
                    {
                        s.Max(aggregate.Name ?? fieldname, c => c.Field(field));
                    }
                    else if (aggregate.Type == AggregateType.Min)
                    {
                        s.Min(aggregate.Name ?? fieldname, c => c.Field(field));
                    }
                }
                return s;
            };
        }
        /// <summary>
        /// 时间聚合（需指定聚合类型）
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="search">查询条件</param>
        /// <param name="interval">聚合格式</param>
        /// <param name="condition">聚合条件</param>
        /// <param name="script">聚合条件字段，多字段逗号分隔</param>
        /// <param name="fields">聚合内容字段</param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> GroupByDate<TDocument, TValue>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, DateInterval interval, Expression<Func<TDocument, DateTime>> condition, string script, params Expression<Func<TDocument, TValue>>[] fields) where TDocument : class
        {
            string fieldName = condition.GetFieldName();
            if (string.IsNullOrWhiteSpace(fieldName)) throw new NullReferenceException("Group By FieldName IS NULL");
            IAggregationContainer group(AggregationContainerDescriptor<TDocument> aggs)
            {
                string[] group_field = WebAgent.GetArray<string>(script);
                string _script = string.Empty;
                for (int i = 0; i < group_field.Length; i++)
                {
                    _script += "doc['" + group_field[i] + "'].value";
                    if (i + 1 < group_field.Length)
                    {
                        _script += "+'-'+";
                    }
                }
                return aggs.DateHistogram(fieldName, d => d.Field(condition).CalendarInterval(interval).Format("yyyy-MM-dd").Aggregations(aggs => aggs.Terms("group_by_script", t => t.Script(_script).Aggregations(GroupBy(fields)))));
            }
            return (s) =>
            {
                s.Size(0).Aggregations(group);
                search.Invoke(s);
                return s;
            };
        }
        /// <summary>
        /// 查询指定字段
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static SearchDescriptor<TDocument> Select<TDocument>(this SearchDescriptor<TDocument> query, params Expression<Func<TDocument, object>>[] fields) where TDocument : class
        {
            if (query == null) throw new NullReferenceException();
            return query.Source(sc => sc.Includes(ic => ic.Fields(fields)));
        }
        /// <summary>
        /// 查询指定字段
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Select<TDocument>(this Func<SearchDescriptor<TDocument>, ISearchRequest> query, params Expression<Func<TDocument, object>>[] fields) where TDocument : class
        {
            if (query == null) throw new NullReferenceException();
            return (s) =>
            {
                return s.Select(fields);
            };
        }
        /// <summary>
        /// 转换聚合值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static TDocument ToAggregate<TDocument>(this ISearchResponse<TDocument> response) where TDocument : class
        {
            TDocument document = Activator.CreateInstance<TDocument>();
            IEnumerable<PropertyInfo> properties = typeof(TDocument).GetProperties().Where(c => c.HasAttribute<AggregateAttribute>());
            foreach (PropertyInfo property in properties)
            {
                AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
                if (aggregate == null) continue;
                string fieldname = aggregate.Name ?? property.GetFieldName();
                object? value = null;
                if (aggregate.Type == AggregateType.Sum)
                {
                    value = response.Aggregations.Sum(fieldname)?.Value;
                }
                else if (aggregate.Type == AggregateType.Average)
                {
                    value = response.Aggregations.Average(fieldname)?.Value;
                }
                else if (aggregate.Type == AggregateType.Count)
                {
                    value = response.Aggregations.ValueCount(fieldname)?.Value;
                }
                else if (aggregate.Type == AggregateType.Max)
                {
                    value = response.Aggregations.Max(fieldname)?.Value;
                }
                else if (aggregate.Type == AggregateType.Min)
                {
                    value = response.Aggregations.Min(fieldname)?.Value;
                }
                if (value == null) continue;
                property.SetValue(document, Convert.ChangeType(value, property.PropertyType));
            }
            return document;
        }
        /// <summary>
        /// 转换聚合值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="response"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public static IEnumerable<TDocument> ToAggregate<TDocument>(this ISearchResponse<TDocument> response, string script) where TDocument : class
        {
            if (response == null) throw new NullReferenceException();
            IEnumerable<PropertyInfo> properties = typeof(TDocument).GetProperties();
            string[] scripts = WebAgent.GetArray<string>(script.ToLower());
            foreach (var item in response.Aggregations.Terms("group_by_script").Buckets)
            {
                TDocument document = Activator.CreateInstance<TDocument>();
                string[] key_value = WebAgent.GetArray<string>(item.Key, '-');
                foreach (PropertyInfo property in properties)
                {
                    object? value = null;
                    string name = property.GetFieldName().ToLower();
                    if (property.HasAttribute<CountAttribute>())
                    {
                        value = item.DocCount;
                    }
                    else if (scripts.Contains(name))
                    {
                        int index = System.Array.IndexOf(scripts, name);
                        if (property.PropertyType.IsEnum)
                        {
                            value = key_value[index].ToEnum(property.PropertyType);
                        }
                        else
                        {
                            value = key_value[index];
                        }
                    }
                    else
                    {
                        AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
                        if (aggregate == null) continue;
                        string fieldname = aggregate.Name ?? property.GetFieldName();
                        if (aggregate.Type == AggregateType.Sum)
                        {
                            value = item.Sum(fieldname)?.Value;
                        }
                        else if (aggregate.Type == AggregateType.Average)
                        {
                            value = item.Average(fieldname)?.Value;
                        }
                        else if (aggregate.Type == AggregateType.Count)
                        {
                            value = item.ValueCount(fieldname)?.Value;
                        }
                        else if (aggregate.Type == AggregateType.Max)
                        {
                            value = item.Max(fieldname)?.Value;
                        }
                        else if (aggregate.Type == AggregateType.Min)
                        {
                            value = item.Min(fieldname)?.Value;
                        }
                    }
                    if (value == null) continue;
                    property.SetValue(document, Convert.ChangeType(value, property.PropertyType));
                }
                yield return document;
            }
        }
        /// <summary>
        /// 时间聚合转换
        /// </summary>
        /// <param name="response"></param>
        /// <param name="field">日期聚合的条件</param>
        /// <param name="script">聚合条件脚本，多字段逗号分隔，注意（顺序跟Group中的script一致）</param>
        /// <returns></returns>
        public static IEnumerable<TDocument> ToDateAggregate<TDocument>(this ISearchResponse<TDocument> response, Expression<Func<TDocument, DateTime>> field, string script) where TDocument : class
        {
            if (response == null) throw new NullReferenceException();
            string condition = field.GetFieldName();
            PropertyInfo propertyCondition = field.ToPropertyInfo();
            IEnumerable<PropertyInfo> properties = typeof(TDocument).GetProperties();
            string[] scripts = WebAgent.GetArray<string>(script.ToLower());
            foreach (DateHistogramBucket bucket in response.Aggregations.DateHistogram(condition).Buckets)
            {
                TDocument document = Activator.CreateInstance<TDocument>();
                DateTime key_date = Convert.ToDateTime(bucket.KeyAsString);
                foreach (var item in bucket.Terms("group_by_script").Buckets)
                {
                    string[] key_value = WebAgent.GetArray<string>(item.Key, '-');
                    foreach (PropertyInfo property in properties)
                    {
                        object? value = null;
                        string name = property.GetFieldName().ToLower();
                        if (property.HasAttribute<CountAttribute>())
                        {
                            value = item.DocCount;
                        }
                        else if (scripts.Contains(name))
                        {
                            int index = System.Array.IndexOf(scripts, name);
                            if (property.PropertyType.IsEnum)
                            {
                                value = key_value[index].ToEnum(property.PropertyType);
                            }
                            else
                            {
                                value = key_value[index];
                            }
                        }
                        else if (property.Name.ToLower() == propertyCondition.Name.ToLower())
                        {
                            value = key_date;
                        }
                        else
                        {
                            AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
                            if (aggregate == null) continue;
                            string fieldname = aggregate.Name ?? property.GetFieldName();
                            if (aggregate.Type == AggregateType.Sum)
                            {
                                value = item.Sum(fieldname)?.Value;
                            }
                            else if (aggregate.Type == AggregateType.Average)
                            {
                                value = item.Average(fieldname)?.Value;
                            }
                            else if (aggregate.Type == AggregateType.Count)
                            {
                                value = item.ValueCount(fieldname)?.Value;
                            }
                            else if (aggregate.Type == AggregateType.Max)
                            {
                                value = item.Max(fieldname)?.Value;
                            }
                            else if (aggregate.Type == AggregateType.Min)
                            {
                                value = item.Min(fieldname)?.Value;
                            }
                        }
                        if (value == null) continue;
                        property.SetValue(document, Convert.ChangeType(value, property.PropertyType));
                    }
                    yield return document;
                }
            }
        }
        /// <summary>
        /// 单个分组ToList
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this AggregateDictionary response)
        {
            if (response == null) throw new NullReferenceException();
            List<T> list = new List<T>();
            foreach (var item in response.Terms("group_by_script").Buckets)
            {
                list.Add(item.Key.GetValue<T>());
            }
            return list;
        }
        public static List<TDocument> ToList<TDocument>(this ISearchResponse<TDocument> response) where TDocument : class
        {
            return response.Documents.ToList();
        }

        /// <summary>
        /// 获取索引名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetIndexName(this Type type)
        {
            ElasticSearchIndexAttribute elasticsearch = type.GetAttribute<ElasticSearchIndexAttribute>();
            if (elasticsearch == null) throw new Exception("not index name");
            return elasticsearch.IndexName;
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string GetFieldName<TDocument, TValue>(this Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            return field.ToPropertyInfo().GetFieldName();
        }
        /// <summary>
        /// 索引不存在时，创建索引
        /// </summary>
        private static void WhenNotExistsAddIndex<TDocument>(this IElasticClient client, ElasticSearchIndexAttribute elasticsearch) where TDocument : class
        {
            if (!IndexCache.ContainsKey(elasticsearch.IndexName) || !IndexCache[elasticsearch.IndexName])
            {
                if (!client.Indices.Exists(elasticsearch.IndexName).Exists)
                {
                    IndexCache[elasticsearch.IndexName] = client.CreateIndex<TDocument>(elasticsearch);
                }
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        private static bool CreateIndex<TDocument>(this IElasticClient client, ElasticSearchIndexAttribute elasticsearch) where TDocument : class
        {
            var rsp = client.Indices.Create(elasticsearch.IndexName, c => c
                .Map<TDocument>(m => m.AutoMap())
                .Aliases(des =>
                {
                    foreach (var aliasName in elasticsearch.AliasNames)
                    {
                        des.Alias(aliasName);
                    }

                    return des;
                }).Settings(s => s.NumberOfReplicas(elasticsearch.ReplicasCount).NumberOfShards(elasticsearch.ShardsCount))
            );
            return rsp.IsValid;
        }
        /// <summary>
        /// 获取ES实体字段名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetFieldName(this PropertyInfo property)
        {
            NumberAttribute number = property.GetAttribute<NumberAttribute>();
            if (number != null)
            {
                if (!string.IsNullOrWhiteSpace(number.Name))
                {
                    return number.Name;
                }
            }
            KeywordAttribute keyword = property.GetAttribute<KeywordAttribute>();
            if (keyword != null)
            {
                if (!string.IsNullOrWhiteSpace(keyword.Name))
                {
                    return keyword.Name;
                }
            }
            DateAttribute date = property.GetAttribute<DateAttribute>();
            if (date != null)
            {
                if (!string.IsNullOrWhiteSpace(date.Name))
                {
                    return date.Name;
                }
            }
            TextAttribute text = property.GetAttribute<TextAttribute>();
            if (text != null)
            {
                if (!string.IsNullOrWhiteSpace(text.Name))
                {
                    return text.Name;
                }
            }
            return property.Name;
        }
    }
}
