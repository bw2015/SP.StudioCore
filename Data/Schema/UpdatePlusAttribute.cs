using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Data.Schema
{
    /// <summary>
    /// 标记字段属性为自增类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UpdatePlusAttribute : Attribute
    {
    }
}
