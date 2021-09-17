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
    /// 内部API接口的对接
    /// 须注册服务 UploadConfig
    /// </summary>
    public static class UploadAgent
    {
        private static UploadConfig Config => IocCollection.GetService<UploadConfig>();

        /// <summary>
        /// 图片上传路径
        /// </summary>
        private static string UploadUrl => Config.UploadUrl;

        /// <summary>
        /// 图片服务器
        /// </summary>
        public static string ImgServer => Config.ImgServer;

        /// <summary>
        /// 上传文件流
        /// </summary>
        /// <param name="type">自定义的文件扩展名</param>
        /// <returns>返回上传结果</returns>
        public static Result Upload(this IFormFile file, string type = null)
        {
            if (string.IsNullOrEmpty(UploadUrl)) throw new ResultException("未配置上传路径");
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byte[] data = ms.ToArray();
                Dictionary<string, string> header = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(type)) header.Add("x-type", type);
                string result = NetAgent.UploadData(UploadUrl, data, Encoding.UTF8, null, header);
                return new Result(ContentType.Result, result);
            }
        }

        /// <summary>
        /// 上传文件流
        /// </summary>
        /// <param name="url">远程文件URL</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Result Upload(string url, string type = null)
        {
            if (string.IsNullOrEmpty(UploadUrl)) throw new ResultException("未配置上传路径");
            if (!System.Uri.IsWellFormedUriString(url, UriKind.Absolute)) throw new ResultException("资源路径非法");
            byte[] data = NetAgent.DownloadFile(url);
            if (data == null) throw new ResultException("下载文件失败");

            Dictionary<string, string> header = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(type)) header.Add("x-type", type);
            string result = NetAgent.UploadData(UploadUrl, data, Encoding.UTF8, null, header);
            return new Result(ContentType.Result, result);
        }

        /// <summary>
        /// 返回layui的上传接口格式
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task LayUpload(this HttpContext context)
        {
            if (context.Request.Form.Files.Count == 0)
            {
                return new Result(ContentType.JSON, new
                {
                    code = 0,
                    msg = "none"
                }).WriteAsync(context);
            }
            Result result = context.Request.Form.Files[0].Upload();
            if (result.Success != 1)
            {
                return new Result(ContentType.JSON, new
                {
                    code = 0,
                    msg = result.Message
                }).WriteAsync(context);
            }
            string fileName = ((JObject)result.Info)["fileName"].Value<string>();

            return new Result(ContentType.JSON, new
            {
                code = 0,
                msg = "success",
                data = new
                {
                    value = fileName,
                    src = fileName.GetImage()
                }
            }).WriteAsync(context);
        }

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
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(defaultPath)) return defaultPath;
                return $"{ImgServer}{defaultPath}";
            }
            if (path.StartsWith("http")) return path;
            return $"{ImgServer}{path}";
        }
    }

    /// <summary>
    /// 上传文件的配置（需在服务中注册）
    /// </summary>
    public class UploadConfig : ISetting
    {
        public UploadConfig(string setting) : base(setting) { }

        public UploadConfig(string uploadUrl, string imgServer)
        {
            this.UploadUrl = uploadUrl;
            this.ImgServer = imgServer;
        }

        public string UploadUrl { get; set; }

        public string ImgServer { get; set; }

        public static implicit operator UploadConfig(string config)
        {
            return new UploadConfig(config);
        }
    }
}
