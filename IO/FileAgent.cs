using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace SP.StudioCore.IO
{
    public static class FileAgent
    {
        /// <summary>
        /// 根据二进制流获取文件的Mime类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetContentType(byte[] data)
        {
            if (data.Length < 2) return null;
            string mime = null;
            switch (string.Concat(data[0], ",", data[1]))
            {
                case "255,216":
                    mime = "image/jpeg";
                    break;
                case "71,73":
                    mime = "image/gif";
                    break;
                case "137,80":
                    mime = "image/png";
                    break;
                case "66,77":
                    mime = "image/bmp";
                    break;
                case "77,90":
                    mime = "application/exe";
                    break;
                case "77,84":
                    mime = "media/midi";
                    break;
                case "82,97":
                    mime = "media/rar";
                    break;
                case "73,68":
                case "255,243":
                case "255,251":
                    mime = "audio/mp3";
                    break;
            }
            return mime;
        }

        /// <summary>
        /// 获取本地文件的二进制内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetFileData(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    return br.ReadBytes((int)fs.Length);
                }
            }
        }

        /// <summary>
        /// 获取文件的MD5值（大写）
        /// 注意：本方法获取MD5的时候需要独占打开文件，如果被别的线程在使用的话则会抛出异常
        /// </summary>
        /// <param name="path">绝对路径</param>
        /// <returns></returns>
        public static string GetMD5(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                return string.Join("", retVal.Select(t => t.ToString("x2")));
            }
            catch
            {
                return null;
            }
        }

    }
}
