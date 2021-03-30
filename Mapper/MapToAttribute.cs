using System;

namespace SP.StudioCore.Mapper
{
    public class MapToAttribute : MapAttribute
    {
        public MapToAttribute(params Type[] targetTypes) : base(targetTypes) { }

        internal override EumMapDirection Direction => EumMapDirection.To;
    }
}
