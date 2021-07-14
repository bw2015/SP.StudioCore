using System;

namespace SP.StudioCore.Log
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public interface IGlobalException
    {
        /// <summary>
        /// 处理异常
        /// </summary>
        bool Handle(Exception exp);
    }
}