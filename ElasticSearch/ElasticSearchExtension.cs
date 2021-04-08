using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Nest;
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
            
            return desc.Script(s=>s.Source(string.Join(';', lstScript)).Params(dicValue));
        }
        
        /// <summary>
        /// 新增
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Insert<TDocument>(this IElasticClient client, TDocument entity) where TDocument : class, IDocument<int>
        {
            return Insert<TDocument, int>(client, entity);
        }
        public static bool Insert<TDocument, TKey>(this IElasticClient client, TDocument entity) where TDocument : class, IDocument<TKey> where TKey : struct
        {
            string indexname = typeof(TDocument).GetIndexName();
            Id id = new Id(entity.ID);
            return client.Index(entity, c => c.Index(indexname).Id(id).Refresh(Refresh.False)).IsValid;
        }
        
        /// <summary>
        /// 根据ID修改所有字段
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool Update<TDocument>(this IElasticClient client, TDocument entity) where TDocument : class, IDocument<int>
        {
            return client.Update<TDocument, int>(entity);
        }
        public static bool Update<TDocument, TKey>(this IElasticClient client, TDocument entity) where TDocument : class, IDocument<TKey> where TKey : struct
        {
            string indexname = typeof(TDocument).GetIndexName();
            string key = string.Empty;//主键
            Dictionary<string, object> param = new Dictionary<string, object>();
            List<string> source = new List<string>();
            foreach (PropertyInfo property in typeof(TDocument).GetProperties())
            {
                string field = property.GetFieldName();
                if (property.Name == "ID")
                {
                    key = field;
                    continue;
                }
                source.Add($"ctx._source.{field}=params.{field}");
                param.Add(field, property.GetValue(entity));
            }
            return client.UpdateByQuery<TDocument>(c => c.Index(indexname).Query(q => q.Term(t => t.Field(key).Value(entity.ID)))
                                                         .Script(s => s.Source(string.Join(";", source)).Params(param))).IsValid;
        }

        /// <summary>
        /// 通过主键ID删除
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Delete<TDocument>(this IElasticClient client, int value) where TDocument : class, IDocument<int>
        {
            return client.Delete<TDocument, int>(value);
        }
        public static bool Delete<TDocument, TKey>(this IElasticClient client, TKey value) where TDocument : class, IDocument<TKey> where TKey : struct
        {
            string indexname = typeof(TDocument).GetIndexName();
            string fieldname = typeof(TDocument).GetProperty("ID").GetFieldName();
            return client.DeleteByQuery<TDocument>(c => c.Index(indexname).Query(q => q.Term(t => t.Field(fieldname).Value(value)))).IsValid;
        }

        public static bool Delete<TDocument>(this IElasticClient client, Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> selector = null) where TDocument : class
        {
            string indexname = typeof(TDocument).GetIndexName();
            Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> action = null;
            if (selector == null)
            {
                action = (s) =>
                {
                    return s.Index(indexname);
                };
            }
            else
            {
                action = (s) =>
                {
                    return selector.Invoke(s.Index(indexname));
                };
            }
            return client.DeleteByQuery(action).IsValid;
        }


        public static long Count<TDocument>(this IElasticClient client) where TDocument : class
        {
            string indexname = typeof(TDocument).GetIndexName();
            return client.Count<TDocument>(c => c.Index(indexname)).Count;
        }

        /// <summary>
        /// 通过主键ID查询是否存在
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool Any<TDocument>(this IElasticClient client, int value) where TDocument : class, IDocument<int>
        {
            return client.Any<TDocument, int>(value);
        }
        public static bool Any<TDocument, TKey>(this IElasticClient client, TKey value) where TDocument : class, IDocument<TKey> where TKey : struct
        {
            string indexname = typeof(TDocument).GetIndexName();
            string fieldname = typeof(TDocument).GetProperty("ID").GetFieldName();
            if (value is Guid)
            {
                fieldname = fieldname + ".keyword";
            }
            return client.Search<TDocument>(q => q.Index(indexname).Query(q => q.Term(t => t.Field(fieldname).Value(value))).Select(c => c.ID).Size(1)).Documents.Count == 1;
        }
        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="client"></param>
        /// <param name="value"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TDocument FirstOrDefault<TDocument, TValue>(this IElasticClient client, TValue value, Expression<Func<TDocument, TValue>> expression) where TDocument : class
        {
            return client.Query<TDocument>(c => c.Where(value, expression).Size(1)).Documents.FirstOrDefault();
        }

        public static ISearchResponse<TDocument> Query<TDocument>(this IElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> selector = null) where TDocument : class
        {
            string indexname = typeof(TDocument).GetIndexName();
            Func<SearchDescriptor<TDocument>, ISearchRequest> action = null;
            if (selector == null)
            {
                action = (s) =>
                {
                    return s.Index(indexname).TrackTotalHits(true);
                };
            }
            else
            {
                action = (s) =>
                {
                    return selector.Invoke(s.Index(indexname).TrackTotalHits(true));
                };

            }
            return client.Search(action);
        }

        /// <summary>
        /// 拼接where 条件
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="client"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Where<TDocument>(this IElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> search) where TDocument : class
        {
            return search;
        }
        public static SearchDescriptor<TDocument> Where<TDocument, TValue>(this SearchDescriptor<TDocument> query, object value, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            if (value == null) return query;
            if (query == null) throw new NullReferenceException();
            switch (value.GetType().Name)
            {
                case "Guid":
                case "String":
                    query = query.Query(q => q.Term(c => c.Field(field.GetFieldName() + ".keyword").Value(value)));
                    break;
                case "Int32":
                    query = query.Where((int?)value, field);
                    break;
                case "Int64":
                    query = query.Where((long?)value, field);
                    break;
                case "Byte":
                    query = query.Where((byte?)value, field);
                    break;
            }
            return query;
        }
        public static SearchDescriptor<TDocument> Where<TDocument, TValue>(this SearchDescriptor<TDocument> query, TValue? value, Expression<Func<TDocument, TValue>> field) where TDocument : class where TValue : struct
        {
            if (value == null) return query;
            if (value is Guid)
            {
                return query.Query(q => q.Term(c => c.Field(field.GetFieldName() + ".keyword").Value(value)));
            }
            else
            {
                return query.Query(q => q.Term(t => t.Field(field).Value(value)));
            }
        }
        public static SearchDescriptor<TDocument> Where<TDocument, TValue>(this SearchDescriptor<TDocument> query, object value, Expression<Func<TDocument, TValue>> field, ExpressionType type) where TDocument : class
        {
            if (value == null) return query;
            switch (value.GetType().Name)
            {
                case "String":
                    break;
                case "Int32":
                    query = query.Where((int)value, field, type);
                    break;
                case "Int64":
                    query = query.Where((long)value, field, type);
                    break;
            }
            return query;
        }
        public static SearchDescriptor<TDocument> Where<TDocument, TValue>(this SearchDescriptor<TDocument> query, TValue? value, Expression<Func<TDocument, TValue>> field, ExpressionType type) where TDocument : class where TValue : struct
        {
            if (value == null) return query;
            object v = value.Value;
            switch (type)
            {
                case ExpressionType.GreaterThan:
                    if (value.Value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.GreaterThan((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.GreaterThan((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    if (value.Value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.GreaterThanOrEquals((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.GreaterThanOrEquals((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.LessThan:
                    if (value.Value is DateTime)
                    {
                        query = query.Query(q => q.DateRange(dr => dr.LessThan((DateTime)v).Field(field)));
                    }
                    else
                    {
                        query = query.Query(q => q.Range(r => r.LessThan((double)v).Field(field)));
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    if (value.Value is DateTime)
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
        /// 分页
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static SearchDescriptor<TDocument> Paged<TDocument, TValue>(this SearchDescriptor<TDocument> query, Expression<Func<TDocument, TValue>> field, int page, int limit) where TDocument : class
        {
            if (page == 1)
            {
                return query.Sort(c => c.Descending(field)).Size(limit);
            }
            else
            {
                return query.Sort(c => c.Descending(field)).From((page - 1) * limit).Size(limit);
            }
        }
        public static SearchDescriptor<TDocument> Paged<TDocument, TValue>(this SearchDescriptor<TDocument> query, Expression<Func<TDocument, TValue>> field) where TDocument : class
        {
            HttpContext context = Web.Context.Current;
            int page = 1;
            int limit = 20;
            if (context != null)
            {
                page = context.QF("PageIndex", 1);
                limit = context.QF("PageSize", 20);
            }
            return query.Paged(field, page, limit);
        }
        public static SearchDescriptor<TDocument> Paged<TDocument>(this SearchDescriptor<TDocument> query) where TDocument : class
        {
            HttpContext context = Web.Context.Current;
            int page = 1;
            int limit = 20;
            if (context != null)
            {
                page = context.QF("PageIndex", 1);
                limit = context.QF("PageSize", 20);
            }
            if (page == 1)
            {
                return query.Size(limit);
            }
            else
            {
                return query.From((page - 1) * limit).Size(limit);
            }
        }
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Paged<TDocument>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search) where TDocument : class
        {
            return (s) =>
              {
                  return search.Invoke(s.Paged());
              };
        }
        /// <summary>
        /// 聚合
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="search"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Func<SearchDescriptor<TDocument>, ISearchRequest> Aggregate<TDocument, TKey>(this Func<SearchDescriptor<TDocument>, ISearchRequest> search, Expression<Func<TDocument, TKey>> field) where TDocument : class
        {
            string fieldname = field.GetFieldName();
            PropertyInfo property = field.ToPropertyInfo();
            AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
            return (s) =>
            {
                s = s.Size(0);
                if (aggregate == null)
                {
                    return search.Invoke(s);
                }
                else if (aggregate.Type == AggregateType.Sum)
                {
                    return search.Invoke(s.Aggregations(aggs => aggs.Sum(aggregate.Name ?? fieldname, c => c.Field(field))));
                }
                else if (aggregate.Type == AggregateType.Average)
                {
                    return search.Invoke(s.Aggregations(aggs => aggs.Average(aggregate.Name ?? fieldname, c => c.Field(field))));
                }
                else if (aggregate.Type == AggregateType.Count)
                {
                    return search.Invoke(s.Aggregations(aggs => aggs.ValueCount(aggregate.Name ?? fieldname, c => c.Field(field))));
                }
                else if (aggregate.Type == AggregateType.Max)
                {
                    return search.Invoke(s.Aggregations(aggs => aggs.Max(aggregate.Name ?? fieldname, c => c.Field(field))));
                }
                else if (aggregate.Type == AggregateType.Min)
                {
                    return search.Invoke(s.Aggregations(aggs => aggs.Min(aggregate.Name ?? fieldname, c => c.Field(field))));
                }
                else
                {
                    return search.Invoke(s.Size(20));
                }
            };
        }
        public static SearchDescriptor<TDocument> Aggregate<TDocument, TKey>(this SearchDescriptor<TDocument> search, Expression<Func<TDocument, TKey>> field) where TDocument : class
        {
            string fieldname = field.GetFieldName();
            PropertyInfo property = field.ToPropertyInfo();
            AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
            if (aggregate == null)
            {
                return search;
            }
            else if (aggregate.Type == AggregateType.Sum)
            {
                return search.Aggregations(aggs => aggs.Sum(aggregate.Name ?? fieldname, c => c.Field(field)));
            }
            else if (aggregate.Type == AggregateType.Average)
            {
                return search.Aggregations(aggs => aggs.Average(aggregate.Name ?? fieldname, c => c.Field(field)));
            }
            else if (aggregate.Type == AggregateType.Count)
            {
                return search.Aggregations(aggs => aggs.ValueCount(aggregate.Name ?? fieldname, c => c.Field(field)));
            }
            else if (aggregate.Type == AggregateType.Max)
            {
                return search.Aggregations(aggs => aggs.Max(aggregate.Name ?? fieldname, c => c.Field(field)));
            }
            else if (aggregate.Type == AggregateType.Min)
            {
                return search.Aggregations(aggs => aggs.Min(aggregate.Name ?? fieldname, c => c.Field(field)));
            }
            else
            {
                return search;
            }
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
            return query.Source(sc => sc.Includes(ic => ic.Fields(fields)));
        }
        /// <summary>
        /// 转换聚合值
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static TDocument ToAggregate<TDocument>(this ISearchResponse<TDocument> response, params Expression<Func<TDocument, object>>[] fields) where TDocument : class
        {
            TDocument document = Activator.CreateInstance<TDocument>();
            PropertyInfo[] properties = fields.Select(c => c.ToPropertyInfo()).ToArray();
            foreach (PropertyInfo property in properties)
            {
                AggregateAttribute aggregate = property.GetAttribute<AggregateAttribute>();
                if (aggregate == null) continue;
                string fieldname = aggregate.Name ?? property.GetFieldName();
                object value = null;
                if (aggregate.Type == AggregateType.Sum)
                {
                    value = response.Aggregations.Sum(fieldname).Value;
                }
                else if (aggregate.Type == AggregateType.Average)
                {
                    value = response.Aggregations.Average(fieldname).Value;
                }
                else if (aggregate.Type == AggregateType.Count)
                {
                    value = response.Aggregations.ValueCount(fieldname).Value;
                }
                else if (aggregate.Type == AggregateType.Max)
                {
                    value = response.Aggregations.Max(fieldname).Value;
                }
                else if (aggregate.Type == AggregateType.Min)
                {
                    value = response.Aggregations.Min(fieldname).Value;
                }
                property.SetValue(document, Convert.ChangeType(value, property.PropertyType));
            }
            return document;
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
