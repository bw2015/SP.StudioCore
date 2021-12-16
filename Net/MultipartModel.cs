using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Net
{
    /// <summary>
    /// 文件上传的对象
    /// </summary>
    public struct MultipartModel
    {
        public string Name;

        public string FileName;

        public string ContentType;

        public byte[] Data;
    }
}
