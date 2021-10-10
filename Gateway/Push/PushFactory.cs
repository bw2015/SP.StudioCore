using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Gateway.Push
{
    public static class PushFactory
    {
        /// <summary>
        /// 创建信息推送对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static IPush? CreatePushObject(PushType type, string setting)
        {
            IPush? push = null;
            switch (type)
            {
                case PushType.GoEasy:
                    push = new GoEasy(setting);
                    break;
            }
            return push;
        }
    }
}
