using Microsoft.AspNetCore.Http;
using SP.StudioCore.Enums;
using SP.StudioCore.Http;
using SP.StudioCore.Model;
using SP.StudioCore.Mvc.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.MiddleWare
{
    /// <summary>
    /// 处理异常的中间件
    /// </summary>
    public abstract class ExceptionMiddlewareBase
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddlewareBase(RequestDelegate next)
        {
            _next = next;
        }

        public async virtual Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        public async virtual Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (ex is ResultException)
            {  
                context.Response.StatusCode = 500;
                await ((ResultException)ex).WriteAsync(context).ConfigureAwait(false);
            }
            else
            {
                await HandleException(context, ex).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 系統錯誤的處理方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected async virtual Task HandleException(HttpContext context, Exception ex)
        {
            Guid requestId = Guid.Empty;
            ErrorType type = ErrorType.Exception;
            if (ex is LoginException)
            {
                type = ErrorType.Authorization;
            }
            else if (ex is PermissionException)
            {
                type = ErrorType.Permission;
            }
            else
            {
                requestId = this.ThrowRequestID(context, ex).RequestID;
            }
            context.Response.StatusCode = 500;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (requestId != Guid.Empty) dic.Add("RequestID", requestId);
            await context.ShowError(type, ex.Message, dic).WriteAsync(context).ConfigureAwait(true);
        }

        /// <summary>
        /// 抛出错误编号(用于子类处理）
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected virtual ErrorLogModel ThrowRequestID(HttpContext context, Exception ex)
        {
            ErrorLogModel model = new ErrorLogModel(ex, 0, 0, context);
            return model;
        }
    }
}
