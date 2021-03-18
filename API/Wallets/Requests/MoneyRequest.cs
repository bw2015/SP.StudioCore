using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Wallets.Requests
{
    /// <summary>
    /// 请求加减款接口
    /// </summary>
    public class MoneyRequest : WalletRequestBase
    {
        public MoneyRequest(string secretKey, string url) : base(secretKey, url)
        {
        }

        public override string Action => "Money";

        /// <summary>
        /// 要操作的用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 资金类型（业务层定义的枚举）
        /// </summary>
        public Enum Type { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 备注（注意，备注信息勿包含中文或特殊字符，因不同平台/语言对于中文的MD5化存在差异）
        /// </summary>
        public string Description { get; set; }
    }
}