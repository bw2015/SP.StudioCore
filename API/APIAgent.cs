using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Ioc;
using SP.StudioCore.Model;
using SP.StudioCore.Mvc.Exceptions;
using SP.StudioCore.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.API
{
    /// <summary>
    /// 内部API接口的路径设置
    /// 须注册服务 UploadConfig
    /// </summary>
    public static class APIPathAgent
    {
        private static APIPathConfig? Config => IocCollection.GetService<APIPathConfig>();


        public static string GetImage(this string path)
        {
            return path.GetImage("/images/space.png");
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="defaultPath">如果不存在，默认的图片</param>
        /// <returns></returns>
        public static string GetImage(this string path, string defaultPath)
        {
            if (Config == null) return defaultPath;
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(defaultPath)) return defaultPath;
                return $"{Config.ImgServer}{defaultPath}";
            }
            if (path.StartsWith("http")) return path;
            return $"{Config.ImgServer}{path}";
        }
    }

    /// <summary>
    /// 路径设置（需在服务中注册）
    /// </summary>
    public class APIPathConfig : ISetting
    {
        public APIPathConfig() { }

        public APIPathConfig(string setting) : base(setting) { }

        public string? ImgServer { get; set; }

        public static implicit operator APIPathConfig(string config)
        {
            return new APIPathConfig(config);
        }
    }
}
