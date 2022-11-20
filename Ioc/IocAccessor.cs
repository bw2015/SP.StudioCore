using SP.StudioCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Ioc
{
    /// <summary>
    /// Studio中用到的所有存取器
    /// </summary>
    internal static class IocAccessor
    {
        /// <summary>
        /// 枚举的扩展
        /// </summary>
        internal static IEnumExtensionHandler EnumExtensionHandler => IocCollection.GetService<IEnumExtensionHandler>() 
            ?? DefaultEnumExtensionHandler.GetInstance();


    }
}
