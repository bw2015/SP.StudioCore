using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Enums
{
    public class EnumConfigAttribute : Attribute
    {
        /// <summary>
        /// 使用Code或者使用值
        /// </summary>
        public bool UseCode { get; set; }

        /// <summary>
        /// 忽略查找
        /// </summary>
        public bool Ignore { get; set; }

        public EnumConfigAttribute(bool useCode = true, bool ignore = false)
        {
            this.UseCode = useCode;
            this.Ignore = ignore;
        }
    }
}
