using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    public abstract class StartBase
    {
        protected HttpContext context;

        protected readonly Stopwatch sw;

        public StartBase(HttpContext context)
        {
            this.context = context;
            sw = new Stopwatch();
            sw.Start();
        }

        /// <summary>
        /// 获取程序执行时长
        /// </summary>
        /// <returns></returns>
        protected string GetTime() => $"{sw.ElapsedMilliseconds}ms";
    }
}
