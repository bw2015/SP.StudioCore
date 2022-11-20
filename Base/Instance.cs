using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Base
{
    /// <summary>
    /// 单例对象的基类实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Instance<T> where T : class, new()
    {
        private static T _instance;

        public static T GetInstance()
        {
            if (_instance == null) { _instance = new T(); }
            return _instance;
        }
    }
}
