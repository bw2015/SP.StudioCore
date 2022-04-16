using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using SP.StudioCore.Net;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec;

namespace SP.StudioCore.Draw
{
    /// <summary>
    /// 二维码的处理
    /// </summary>
    public static class QRAgent
    {
        /// <summary>
        /// 解析二维码
        /// </summary>
        /// <param name="url">远程路径</param>
        /// <returns></returns>
        public static string Decode(Uri url, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            byte[]? bytes = NetAgent.DownloadFile(url.ToString());
            if (bytes == null) throw new TimeoutException($"download image file timeout => {url}");

            using (Image image = bytes.ToImage())
            {
                Bitmap bitmap = new Bitmap(image);
                QRCodeBitmapImage qRCode = new QRCodeBitmapImage(bitmap);
                return new QRCodeDecoder().decode(qRCode, encoding);
            }
        }
    }
}

