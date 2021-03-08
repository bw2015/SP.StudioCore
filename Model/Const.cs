using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    public static class Const
    {
        /// <summary>
        /// 要传递的信息
        /// </summary>
        public const string MESSAGE = "MESSAGE";

        /// <summary>
        /// 计时器
        /// </summary>
        public const string STOPWATCH = "STOPWATCH";

        /// <summary>
        /// 标记是否启用日志（如果启用，则会把Result输出放入context.Item["RESULT"]中）
        /// </summary>
        public const string LOG = "LOG";

        /// <summary>
        /// 是否记录返回值
        /// </summary>
        public const string RESULT = "RESULT";

        /// <summary>
        /// 在http头里面用于标识语言的Key
        /// </summary>
        public const string LANGUAGE = "Content-Language";

        /// <summary>
        /// 放置于Http头内部，用于内部传输用户IP的字段
        /// </summary>
        public const string USERIP = "X-User-IP";

        /// <summary>
        /// http头需要携带得Token内容
        /// </summary>
        public const string AUTH = "Authorization";

        /// <summary>
        /// 出错的金额
        /// </summary>
        public const decimal ERRORMONEY = decimal.MinValue;
    }
}
