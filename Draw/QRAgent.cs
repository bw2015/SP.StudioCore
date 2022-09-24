using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using SP.StudioCore.Net;
using ThoughtWorks.QRCode.Codec.Data;
using ThoughtWorks.QRCode.Codec;
using SkiaSharp.QrCode;
using SkiaSharp;
using System.Xml.Linq;

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

        /// <summary>
        /// 创建一个二维码
        /// </summary>
        public static byte[] Create(string content, int width = 256, int height = 256)
        {
            using (var generator = new QRCodeGenerator())
            {
                var qr = generator.CreateQrCode(content, ECCLevel.H);
                var info = new SKImageInfo(width, height);
                using (var surface = SKSurface.Create(info))
                {
                    var canvas = surface.Canvas;
                    canvas.Render(qr, info.Width, info.Height);

                    using (var image = surface.Snapshot())
                    {
                        using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            return data.ToArray();
                        }
                    }
                }
            }
        }
    }
}
