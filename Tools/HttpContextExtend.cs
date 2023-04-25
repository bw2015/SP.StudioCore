using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    /// <summary>
    /// 可覆盖httpcontext的信息
    /// </summary>
    public class HttpContextExtend
    {
        /// <summary>
        /// 当前请求的路径（不包括QueryString查询字符串）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Get提交的数据
        /// </summary>
        public Dictionary<string, string> Query { get; set; }

        /// <summary>
        /// Form提交的数据
        /// </summary>
        public Dictionary<string, string> Form { get; set; }

        public string QS(string name)
        {
            if (this.Query == null) return string.Empty;
            return this.Query.ContainsKey(name) ? this.Query[name] : string.Empty;
        }

        public string QF(string name)
        {
            if (this.Form == null) return string.Empty;
            return this.Form.ContainsKey(name) ? this.Form[name] : string.Empty;
        }
    }
}
