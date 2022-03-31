using Microsoft.AspNetCore.Http;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.Services
{
    /// <summary>
    /// 检查权限和判断是否登录
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        /// 检查权限
        /// </summary>
        public bool CheckPermission(HttpContext context, PermissionAttribute? permission);
    }
}
