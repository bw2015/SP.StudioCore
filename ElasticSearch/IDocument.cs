using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.ElasticSearch
{
    public interface IDocument : IDocument<int>
    {

    }
    public interface IDocument<TKey> where TKey : struct
    {
        public TKey ID { get; set; }
    }
}
