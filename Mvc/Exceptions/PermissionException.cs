using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Mvc.Exceptions
{
    /// <summary>
    /// 没有权限的异常抛出
    /// </summary>
    public class PermissionException : Exception
    {
        /// <summary>
        /// 错误内容
        /// </summary>
        public override string Message { get; }

        /// <summary>
        /// 权限错误提醒
        /// </summary>
        /// <param name="message"></param>
        public PermissionException(string message)
        {
            this.Message = message;
        }
    }
}
