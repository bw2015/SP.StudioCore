using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Types
{
    /// <summary>
    /// Guid的扩展
    /// </summary>
    public static class GuidExtensions
    {
        public static long ToNumber(this Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

    }
}
