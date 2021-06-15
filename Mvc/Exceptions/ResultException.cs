using Microsoft.AspNetCore.Http;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.Exceptions
{
    /// <summary>
    /// 抛出APIResult的错误异常
    /// </summary>
    public class ResultException : Exception
    {
        private Result result;

        public ResultException(Result result)
        {
            this.result = result;
        }

        public override string Message => this.result.ToString();

        public Task WriteAsync(HttpContext context)
        {
            return result.WriteAsync(context);
        }

        public ResultException()
        {
        }

        public ResultException(string message) : base(message)
        {
            result = new Result(false, message);
        }

        public ResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
