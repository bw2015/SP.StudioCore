using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Gateway.Push.Telegrams
{
    public class TelegramResponse<T>
    {
        public bool ok { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int error_code { get; set; }

        /// <summary>
        /// 错误备注
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// 返回的内容
        /// </summary>
        public T result { get; set; }

        public static implicit operator bool(TelegramResponse<T> response)
        {
            return response != null && response.ok;
        }

        public static implicit operator T(TelegramResponse<T> response)
        {
            return response.result;
        }
    }


}
