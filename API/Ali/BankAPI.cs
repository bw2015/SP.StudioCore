using Newtonsoft.Json;
using SP.StudioCore.Enums;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API.Ali
{
    /// <summary>
    /// 获取银行卡号的信息
    /// </summary>
    public class BankAPI : AliAPIBase<BankAPI>
    {
        public BankAPI(string queryString) : base(queryString)
        {
        }

        public override string? Gateway { get; set; } = "https://ccdcapi.alipay.com/validateAndCacheCardInfo.json";

        /// <summary>
        /// 根据卡号得到银行类型
        /// </summary>
        public BankType Execute(string cardNo)
        {
            string content = NetAgent.DownloadData($"{this.Gateway}?_input_charset=utf-8&cardNo={cardNo}&cardBinCheck=true");
            response res = JsonConvert.DeserializeObject<response>(content);
            if (!res.validated) return 0;
            return res.bank.ToEnum<BankType>();
        }

        struct response
        {
            public bool validated { get; set; }

            public string bank { get; set; }

            public string cardType { get; set; }
        }
    }
}
