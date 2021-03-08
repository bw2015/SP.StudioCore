using Newtonsoft.Json;
using SP.StudioCore.Json;
using SP.StudioCore.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Gateway.Push
{
    /// <summary>
    /// 要发送的信息基类
    /// </summary>
    public abstract class IMessage
    {
        /// <summary>
        /// 动作名
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 频道名
        /// </summary>
        [JsonIgnore]
        public virtual string Channel
        {
            get
            {
                return Encryption.toMD5(this.Name).Substring(8, 8);
            }
        }

        /// <summary>
        /// 输出需要推送的JSON对象
        /// </summary>
        /// <returns></returns>
        public virtual object ToObject()
        {
            return this;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this.ToObject());
        }
    }
}
