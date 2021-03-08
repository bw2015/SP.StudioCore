using SP.StudioCore.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.StudioCore.Utils
{
    /// <summary>
    /// 锁
    /// </summary>
    public static class LockHelper
    {
        static LockHelper()
        {
            string md5 = "0123456789ABCDEF";
            int[] index = new int[3];
            for (index[0] = 0; index[0] < md5.Length; index[0]++)
                for (index[1] = 0; index[1] < md5.Length; index[1]++)
                    for (index[2] = 0; index[2] < md5.Length; index[2]++)
                    {
                        string str = string.Join("", index.Select(t => md5[t]));
                        LOCKER.Add(str, new object());
                    }
        }

        private static readonly Dictionary<string, object> LOCKER = new Dictionary<string, object>();

        /// <summary>
        /// 获取锁定字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetLoker(string key)
        {
            string md5 = Encryption.toMD5(key).Substring(0, 3);
            return LOCKER[md5];
        }
    }
}
