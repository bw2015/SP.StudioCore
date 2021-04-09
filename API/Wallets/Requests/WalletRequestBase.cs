using SP.StudioCore.Array;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using SP.StudioCore.Model;
using SP.StudioCore.Security;
using SP.StudioCore.Types;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SP.StudioCore.API.Wallets.Requests
{
    /// <summary>
    /// 收到查询请求的实现类
    /// </summary>
    public abstract class WalletRequestBase
    {
        protected WalletRequestBase(string secretKey, string url)
        {
            this.SecretKey = secretKey;
            Url            = url;
        }

        /// <summary>
        /// 请求地址
        /// </summary>
        [Ignore]public string Url { get; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Ignore]public string SecretKey { get; }

        /// <summary>
        /// 动作名称
        /// </summary>
        public abstract string Action { get; }

        /// <summary>
        /// 请求参数
        /// </summary>
        [Ignore]public string PostData => this.ToString();

        /// <summary>
        /// 时间戳（毫秒)
        /// </summary>
        public long Timestamp => WebAgent.GetTimestamps();

        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime RequestAt { get; set; }
        
        /// <summary>
        /// 当前通信所使用的语种
        /// </summary>
        [Ignore] public Language Language { get; set; }

        /// <summary>
        /// 扩展数据
        /// 目的1：记录日志的时候可以标记用户ID、商户ID等本实体类没有的信息
        /// </summary>
        [Ignore] public object ExtendData { get; set; }

        /// <summary>
        /// 查询提交的JSON内容
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var dic = new SortedDictionary<string, string>
            {
                {"Action", this.Action},
                {"Timestamp", this.Timestamp.ToString()}
            };
            foreach (PropertyInfo property in this.GetType().GetProperties().Where(t => !t.HasAttribute<IgnoreAttribute>()))
            {
                if (!dic.ContainsKey(property.Name))
                {
                    dic.Add(property.Name, property.GetValue(this).ToString());
                }
            }

            string signStr = dic.ToQueryString() + this.SecretKey;
            dic.Add("Sign", Encryption.toMD5(signStr));
            return dic.ToJson();
        }
    }
}