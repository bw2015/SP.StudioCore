using System;

namespace SP.StudioCore.Mapper
{
    public class MapFromAttribute : MapAttribute
    {
        public MapFromAttribute(params Type[] targetTypes) : base(targetTypes) { }

        internal override EumMapDirection Direction => EumMapDirection.From;
    }
}
