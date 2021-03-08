using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.MiddleWare
{
    public abstract class RepositoryMiddleware
    {
        private readonly RequestDelegate _next;
        public RepositoryMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public virtual Task Invoke(HttpContext context)
        {

            return _next.Invoke(context);
        }
    }
}
