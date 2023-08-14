using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Tools
{
    /// <summary>
    /// 入口程序
    /// </summary>
    public interface IToolStart
    {
        public string? Execute();
    }
}
