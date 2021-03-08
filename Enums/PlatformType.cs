using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SP.StudioCore.Enums
{
    /// <summary>
    /// 平台类型
    /// </summary>
    [Flags]
    public enum PlatformType : byte
    {
        [Description("网页端")]
        PC = 1,
        [Description("移动端")]
        Mobile = 2,
        [Description("微信端")]
        Wechat = 4,
        [Description("IOS")]
        IOS = 8,
        [Description("Andoird")]
        Android = 16,
        [Description("APP")]
        APP = 32,
        /// <summary>
        /// Windows设备
        /// </summary>
        [Description("Windows")]
        Windows = 64,
        /// <summary>
        /// 苹果电脑
        /// </summary>
        [Description("MAC设备")]
        MAC = 128
    }
}
