using Aliyun.OSS;
using SP.StudioCore.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SP.StudioCore.API
{
    /// <summary>
    /// 阿里云OSS存储
    /// </summary>
    public static class OSSAgent
    {
        /// <summary>
        /// 上传本地文件
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="objectName">远程文件名（包含路径），不能以/开头</param>
        /// <param name="localFilename">本地路径</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Upload(this OSSSetting setting, string objectName, string localFilename, out string message)
        {
            message = null;
            try
            {
                OssClient client = new OssClient(setting.endpoint, setting.accessKeyId, setting.accessKeySecret);
                PutObjectResult result = client.PutObject(setting.bucketName, objectName, localFilename);
                return true;
            }
            catch (Exception ex)
            {
                message = "OSS错误:" + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 上传二进制内容
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="objectName">远程文件名（包含路径），不能以/开头</param>
        /// <param name="binaryData">二进制内容</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Upload(this OSSSetting setting, string objectName, byte[] binaryData, ObjectMetadata metadata, out string message)
        {
            message = null;
            try
            {
                using (MemoryStream requestContent = new MemoryStream(binaryData))
                {
                    OssClient client = new OssClient(setting.endpoint, setting.accessKeyId, setting.accessKeySecret);
                    PutObjectResult result = client.PutObject(setting.bucketName, objectName, requestContent, metadata);
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "OSS错误:" + ex.Message;
                return false;
            }
        }
    }

    /// <summary>
    /// OSS 的参数设定
    /// </summary>
    public class OSSSetting : ISetting
    {
        public OSSSetting(string queryString) : base(queryString)
        {
        }

        /// <summary>
        /// EndPoint（地域节点）
        /// </summary>
        public string endpoint { get; set; }

        /// <summary>
        /// 授权账户（RAM管理内）
        /// </summary>
        public string accessKeyId { get; set; }

        /// <summary>
        /// 授权密钥（RAM管理内）
        /// </summary>
        public string accessKeySecret { get; set; }

        /// <summary>
        /// 存储对象名字（backet的名字）
        /// </summary>
        public string bucketName { get; set; }
    }
}
