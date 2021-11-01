using SP.StudioCore.Enums;
using System;

namespace SP.StudioCore.Mapper
{
    /// <summary>
    /// 实体匹配方向枚举
    /// </summary>
    [Flags,EnumConfig(ignore: true)]
    public enum EumMapDirection
    {
        From,
        To
    }
}
