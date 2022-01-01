using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Draw
{
    /// <summary>
    /// 图片的剪裁工具
    /// </summary>
    public static class CutUtils
    {
        /// <summary>
        /// 剪裁图片
        /// </summary>
        /// <param name="image">源图</param>
        /// <param name="x">坐标 X</param>
        /// <param name="y">坐标 Y</param>
        /// <param name="width">剪裁的宽度</param>
        /// <param name="height">剪裁的高度</param>
        /// <returns></returns>
        public static Image Cut(this Image image, int x, int y, int width, int height)
        {
            using (Bitmap bitmap = new(image))
            {
                Rectangle area = new(x, y, width, height);
                Bitmap bmpCrop = bitmap.Clone(area, bitmap.PixelFormat);
                return bmpCrop;
            }
        }

        public static Image Cut(this Image image, Rectangle area)
        {
            return image.Cut(area.X, area.Y, area.Width, area.Height);
        }
    }
}
