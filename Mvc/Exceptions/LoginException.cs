using SP.StudioCore.Enums;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Text;


namespace SP.StudioCore.Mvc.Exceptions
{
    /// <summary>
    /// 未登录引发的异常
    /// </summary>
    public sealed class LoginException : Exception
    {
        public override string Message { get; }

        public LoginException(string message)
        {
            Message = message;
        }

        public LoginException()
        {
            this.Message = ErrorType.Login.GetDescription();
        }

        /// <summary>
        /// 转换成为Result的输出类型
        /// </summary>
        /// <param name="ex"></param>
        public static implicit operator Result(LoginException ex)
        {
            return new Result(false, ex.Message, new
            {
                ErrorType = "Login"
            });
        }
    }
}
