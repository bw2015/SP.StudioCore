using Google.Protobuf;
using SP.StudioCore.Cache.Memory;
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


        /// <summary>
        /// 设定对象锁定
        /// </summary>
        /// <param name="lockTime">默认的锁定时间（1分钟）</param>

        public static void SetLock<T>(TimeSpan? lockTime = null) where T : IMessage
        {
            lockTime ??= TimeSpan.FromMinutes(1);
            MemoryUtils.Set(typeof(T).Name, DateTime.Now, lockTime.Value);
        }

        /// <summary>
        /// 解除锁定，并且得到锁定的时间点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static DateTime? UnLock<T>() where T : IMessage
        {
            string key = typeof(T).Name;
            if (MemoryUtils.Contains(key))
            {
                DateTime value = (DateTime)MemoryUtils.Get(key);
                MemoryUtils.Remove(key);
                return value;
            }
            return null;
        }

        /// <summary>
        /// 锁定对象名
        /// </summary>
        public static bool IsLock<T>() where T : IMessage
        {
            return MemoryUtils.Contains(typeof(T).Name);
        }
    }
}
