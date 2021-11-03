using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc.Exceptions
{
    /// <summary>
    /// 指定错误代码
    /// </summary>
    public class ErrorCodeAttribute : Attribute
    {
        public string Code { get; private set; }

        public ErrorCodeAttribute(string code)
        {
            this.Code = code;
        }

        public ErrorCodeAttribute(ErrorType type)
        {
            this.Code = type.ToString();
        }
    }
}
