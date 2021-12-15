using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Draw
{
    /// <summary>
    /// 图片颜色处理
    /// </summary>
    public static class ColorUtils
    {
        /// <summary>
        /// 移除其他颜色，只保留指定的颜色
        /// </summary>
        /// <param name="pixel">需要保留的颜色</param>
        /// <param name="color">删除后被替换的颜色</param>
        /// <param name="a">色差范围</param>
        public static Bitmap RemoveColor(this Bitmap img, Color pixel, Color color, int a = 0)
        {
            Bitmap image = new Bitmap(img.Width, img.Height);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color c = img.GetPixel(x, y);
                    if (Math.Abs(c.R - pixel.R) <= a && Math.Abs(c.G - pixel.G) <= a && Math.Abs(c.B - pixel.B) <= a)
                    {
                        image.SetPixel(x, y, c);
                    }
                    else
                    {
                        image.SetPixel(x, y, color);
                    }

                }
            }
            return image;
        }
    }
}
