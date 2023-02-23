using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API
{
    /// <summary>
    /// 翻译接口
    /// </summary>
    public interface ITranslateAPI
    {
        /// <summary>
        /// 执行翻译
        /// </summary>
        /// <param name="content">原文内容</param>
        /// <param name="source">原文语种</param>
        /// <param name="target">目标语种</param>
        /// <param name="result">翻译之后的内容</param>
        /// <returns></returns>
        public bool Execute(string content, Language source, Language target, out string result);
    }
}
