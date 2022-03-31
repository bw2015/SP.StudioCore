using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// 标记当前方法可被游客访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class GuestAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记当前方法只能管理员访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AdminAttribute : Attribute
    {

    }

    /// <summary>
    /// 标记当前方法只有登录用户才能访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class UserAttribute : Attribute
    {

    }

    /// <summary>
    /// 权限设定
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class PermissionAttribute : System.Attribute
    {
        public PermissionAttribute()
        {
            this.Value = string.Empty;
        }

        public PermissionAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(PermissionAttribute permission)
        {
            return permission.Value;
        }
    }

    /// <summary>
    /// 忽略在线统计标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class IgnoreOnlineAttribute : Attribute
    {

    }
}
