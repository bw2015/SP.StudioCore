using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Gateway.AliMNS.Interface
{
    /// <summary>
    /// MNS 接口
    /// </summary>
    public interface IMessageMns
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        string Send(string body);
    }
}
