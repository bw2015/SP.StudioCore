using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Json
{
    /// <summary>
    /// 标记一个字符串是JSON格式的字符串
    /// </summary>
    public struct JsonString : IEquatable<JsonString>
    {
        private readonly string _jsonString;

        public JsonString(string jsonString)
        {
            this._jsonString = jsonString;
        }

        public JsonString(params object[] args)
        {
            this._jsonString = string.Concat(args);
        }

        public override string ToString()
        {
            return this._jsonString;
        }

        public static implicit operator JsonString(string json)
        {
            if (json == null) return new JsonString("null");
            if (string.IsNullOrEmpty(json)) return new JsonString("\"\"");
            return new JsonString(json);
        }

        public static implicit operator string(JsonString json)
        {
            return json._jsonString;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return !string.IsNullOrEmpty(this._jsonString);
            return false;
        }

        public override int GetHashCode()
        {
            if (this == null) return 0;
            return this._jsonString.Length;
        }

        public static bool operator ==(JsonString left, JsonString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JsonString left, JsonString right)
        {
            return !(left == right);
        }

        public bool Equals(JsonString other)
        {
            throw new NotImplementedException();
        }
    }
}
