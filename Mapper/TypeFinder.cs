using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace SP.StudioCore.Mapper
{
    /// <summary>
    /// 类型查找器
    /// </summary>
    public class TypeFinder
    {
        private readonly object _syncObj = new object();
        private Type[] _types;


        /// <summary>
        /// 查找类型
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public Type[] Find(Func<Type, bool> predicate)
        {
            return GetAllTypes().Where(predicate).ToArray();
        }

        /// <summary>
        /// 查找所有的类型
        /// </summary>
        /// <returns></returns>
        public Type[] FindAll() => GetAllTypes().ToArray();

        /// <summary>
        /// 获取所有的类型
        /// </summary>
        /// <returns></returns>
        private Type[] GetAllTypes()
        {
            if (_types == null)
            {
                lock (_syncObj)
                {
                    if (_types == null)
                    {
                        _types = CreateTypeList().ToArray();
                    }
                }
            }

            return _types;
        }

        /// <summary>
        /// 创建类型列表
        /// </summary>
        /// <returns></returns>
        private List<Type> CreateTypeList()
        {
            var allTypes = new List<Type>();

            var assemblies = GetAssembliesFromFolder().Distinct();

            foreach (var assembly in assemblies)
            {
                try
                {
                    Type[] typesInThisAssembly;

                    try
                    {
                        typesInThisAssembly = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        typesInThisAssembly = ex.Types;
                    }

                    if (typesInThisAssembly == null || typesInThisAssembly.Length == 0)
                    {
                        continue;
                    }

                    allTypes.AddRange(typesInThisAssembly.Where(type => type != null));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return allTypes;
        }


        /// <summary>
        /// 在当前运行目录下，查找所有dll
        /// </summary>
        /// <returns></returns>
        public List<Assembly> GetAssembliesFromFolder()
        {
            var assemblies = new List<Assembly>();
            var files = Directory.GetFiles($"{AppContext.BaseDirectory}", "*.dll");
            foreach (var file in files)
            {
                try
                {
                    assemblies.Add(Assembly.Load(AssemblyLoadContext.GetAssemblyName(file)));
                }
                catch (Exception)
                {
                    // ignored 失败的原因是非托管程序
                }
            }

            return assemblies;
        }
    }
}