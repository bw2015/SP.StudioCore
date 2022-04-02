using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Draw
{
    /// <summary>
    /// 图片工具
    /// </summary>
    public static class ImageUtils
    {
        /// <summary>
        /// 图片转化成为 base64
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToBase64(this Bitmap bitmap, ImageFormat? format = null)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, format ?? ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    return Convert.ToBase64String(arr);
                }
            }
            catch
            {
                throw;
            }
        }

        public static byte[] ToArray(this Image image, ImageFormat? format = null)
        {
            format ??= ImageFormat.Png;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return arr;
            }
        }

        /// <summary>
        /// base图片转化成为图片对象
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static Image ToImage(this string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return bytes.ToImage();
        }

        public static Image ToImage(this byte[] bytes)
        {
            using (Stream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        /// <summary>
        /// 比对两张图片是否完全一致
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="secondImage"></param>
        /// <returns></returns>
        public static bool Compare(this Bitmap bitmap, Bitmap secondImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                string firstBitmap = bitmap.ToBase64(ImageFormat.Png);
                string secondBitmap = secondImage.ToBase64(ImageFormat.Png);
                if (firstBitmap.Equals(secondBitmap))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取图片的指纹哈希
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static string GetHash(this Image bitmap, int width = 8, int height = 8)
        {
            //#1 Reduce size to 8*8
            //将图片缩小到8x8的尺寸, 总共64个像素. 这一步的作用是去除各种图片尺寸和图片比例的差异, 只保留结构、明暗等基本信息.
            Image image = bitmap.GetThumbnailImage(width, height, () => false, IntPtr.Zero);

            //#2 Reduce Color
            //将缩小后的图片, 转为64级灰度图片.
            Bitmap bitMap = new(image);
            byte[] grayValues = new byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color color = bitMap.GetPixel(x, y);
                    byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    grayValues[x * image.Width + y] = grayValue;
                }
            }

            //#3 Average the colors
            //计算图片中所有像素的灰度平均值
            int sum = 0;
            for (int i = 0; i < grayValues.Length; i++)
                sum += (int)grayValues[i];
            byte avgByte = Convert.ToByte(sum / grayValues.Length);

            //#4 Compute the bits
            //将每个像素的灰度与平均值进行比较, 如果大于或等于平均值记为1, 小于平均值记为0.
            char[] result = new char[grayValues.Length];
            for (int i = 0; i < grayValues.Length; i++)
            {
                if (grayValues[i] < avgByte)
                    result[i] = '0';
                else
                    result[i] = '1';
            }

            // 将上一步的比较结果, 组合在一起, 就构成了一个64位的二进制整数, 这就是这张图片的指纹.
            return new(result);
        }

        /// <summary>
        /// 比对图片指纹
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="secondImage"></param>
        /// <returns>如果不相同的数据位数不超过5, 就说明两张图片很相似, 如果大于10, 说明它们是两张不同的图片.</returns>
        public static int CompareImage(this Image bitmap, Image secondImage)
        {
            string hash1 = bitmap.GetHash();
            string hash2 = secondImage.GetHash();

            return hash1.CompareImage(hash2);
        }

        public static int CompareImage(this Image bitmap, string hash2)
        {
            string hash1 = bitmap.GetHash();
            return hash1.CompareImage(hash2);
        }

        public static int CompareImage(this string hash1, string hash2)
        {
            if (hash1.Length != hash2.Length)
                throw new ArgumentException();
            int count = 0;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    count++;
            }
            return count;
        }

        public static int CompareImage(this string hash1, string[] hashs)
        {
            if (hashs == null || !hashs.Any()) return 64;
            List<int> list = new();
            foreach (string hash in hashs)
            {
                list.Add(hash1.CompareImage(hash));
            }
            return list.Min();
        }

        /// <summary>
        /// 多重比对
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="hashs"></param>
        /// <returns></returns>
        public static int CompareImage(this string[] hash, string[] hashs)
        {
            if (hash == null || !hash.Any() || hashs == null || !hashs.Any()) return 64;
            List<int> list = new();
            foreach (string code in hash)
            {
                list.Add(code.CompareImage(hashs));
            }
            return list.Min();
        }

    }
}
