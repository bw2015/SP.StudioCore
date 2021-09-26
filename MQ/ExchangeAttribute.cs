using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.MQ
{
    /// <summary>
    /// 在消息实体类上定义交换机名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExchangeAttribute : Attribute
    {
        public ExchangeAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; }
    }
}
