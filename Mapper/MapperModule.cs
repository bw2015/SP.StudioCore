//using System.Reflection;

//namespace SP.StudioCore.Mapper
//{
//    /// <summary>
//    /// AutoMap初始化模块
//    /// </summary>
//    public class MapperModule
//    {
//        /// <summary>
//        /// 查找所有标注了AutoMap、AutoMapFrom以及AutoMapTo特性的类型，并完成他们之间的Map
//        /// </summary>
//        public void Init()
//        {
//            var types = new TypeFinder().Find(type =>
//                type.IsDefined(typeof(MapAttribute)) ||
//                type.IsDefined(typeof(MapFromAttribute)) ||
//                type.IsDefined(typeof(MapToAttribute))
//            );
            
//            MapperHelper.CreateMap(types);
//        }
//    }
//}