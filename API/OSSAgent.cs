using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using SP.StudioCore.Array;
using SP.StudioCore.Http;
using SP.StudioCore.Model;
using SP.StudioCore.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static Dictionary<string, string> uploadTokenId = new Dictionary<string, string>();

        private static Dictionary<string, List<PartETag>> uploadETags = new Dictionary<string, List<PartETag>>();

        /// <summary>
        /// 分片断点续传
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="objectName"></param>
        /// <param name="binaryData"></param>
        /// <param name="index">第多少个分片（从1开始）</param>
        /// <param name="total">总分片数量</param>
        /// <returns></returns>
        public static bool Upload(this OSSSetting setting, string objectName, byte[] binaryData, int index, int total, string uploadToken)
        {
            OssClient client = new OssClient(setting.endpoint, setting.accessKeyId, setting.accessKeySecret);

            string uploadId = uploadTokenId.Get(uploadToken);
            List<PartETag> partETags = uploadETags.Get(uploadToken);
            if (index == 1)
            {
                var request = new InitiateMultipartUploadRequest(setting.bucketName, objectName, new ObjectMetadata
                {
                    ExpirationTime = DateTime.Now.AddDays(1)
                });
                var result = client.InitiateMultipartUpload(request);
                uploadId = result.UploadId;
                partETags = new List<PartETag>();

                if (uploadTokenId.ContainsKey(uploadToken))
                {
                    uploadTokenId[uploadToken] = uploadId;
                }
                else
                {
                    uploadTokenId.Add(uploadToken, uploadId);
                }
                if (uploadETags.ContainsKey(uploadToken))
                {
                    uploadETags[uploadToken] = partETags;
                }
                else
                {
                    uploadETags.Add(uploadToken, partETags);
                }
            }

            using (MemoryStream requestContent = new MemoryStream(binaryData))
            {
                var result = client.UploadPart(new UploadPartRequest(setting.bucketName, objectName, uploadId)
                {
                    InputStream = requestContent,
                    PartSize = binaryData.Length,
                    PartNumber = index
                });
                partETags.Add(result.PartETag);
            }

            if (index == total)
            {
                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(setting.bucketName, objectName, uploadId);
                foreach (var partETag in partETags)
                {
                    completeMultipartUploadRequest.PartETags.Add(partETag);
                }
                client.CompleteMultipartUpload(completeMultipartUploadRequest);

                if (uploadTokenId.ContainsKey(uploadToken)) uploadTokenId.Remove(uploadToken);
                if (uploadETags.ContainsKey(uploadToken)) uploadETags.Remove(uploadToken);
            }

            return true;
        }

        /// <summary>
        /// 设定文件的过期时间
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="objectName"></param>
        /// <param name="expireTime">为null是永不过期</param>
        /// <returns></returns>
        public static bool SetExpirationTime(this OSSSetting setting, string objectName, DateTime? expireTime = null)
        {
            expireTime ??= DateTime.MaxValue;
            OssClient client = new OssClient(setting.endpoint, setting.accessKeyId, setting.accessKeySecret);
            client.ModifyObjectMeta(setting.bucketName, objectName, new ObjectMetadata
            {
                ExpirationTime = expireTime.Value
            });
            return true;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public static bool Delete(this OSSSetting setting, string objectName)
        {
            OssClient client = new OssClient(setting.endpoint, setting.accessKeyId, setting.accessKeySecret);
            DeleteObjectResult result = client.DeleteObject(setting.bucketName, objectName);
            return true;
        }

        /// <summary>
        /// 封装的上传图片到OSS的方法
        /// 固定上传到 /upload/{year}{month}/{md5}.{type}
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string UploadImage(this OSSSetting setting, IFormFile file)
        {
            if (file == null)
            {
                throw new NullReferenceException();
            }
            string fix = file.FileName[file.FileName.LastIndexOf('.')..][1..];
            byte[] data = file.ToArray();
            string md5 = Encryption.toMD5Short(Encryption.toMD5(data));
            string path = $"upload/{DateTime.Now.ToString("yyyyMM")}/{md5}.{fix}";

            if (setting.Upload(path, data, new ObjectMetadata(), out string message))
            {
                return $"/{path}";
            }
            throw new Exception(message);
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
        [Description("EndPoint")]
        public string endpoint { get; set; }

        /// <summary>
        /// 授权账户（RAM管理内）
        /// </summary>
        [Description("授权账户")]
        public string accessKeyId { get; set; }

        /// <summary>
        /// 授权密钥（RAM管理内）
        /// </summary>
        [Description("授权密钥")]
        public string accessKeySecret { get; set; }

        /// <summary>
        /// 存储对象名字（backet的名字）
        /// </summary>
        [Description("backetName")]
        public string bucketName { get; set; }
    }
}
