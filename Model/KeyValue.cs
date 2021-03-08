using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    public struct KeyValue<TKey, TValue>
    {
        public KeyValue(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public TKey Key;

        public TValue Value;

        public override string ToString()
        {
            return $"{Key}={Value}";
        }
    }
}
