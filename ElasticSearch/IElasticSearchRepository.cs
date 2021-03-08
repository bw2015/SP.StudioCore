using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SP.StudioCore.ElasticSearch
{
    public interface IElasticSearchRepository
    {
        bool Insert<TEntity>(TEntity entity) where TEntity : class, IDocument;
        bool Delete<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class;
    }
}
