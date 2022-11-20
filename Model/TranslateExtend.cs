using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Model
{
    public static class TranslateExtend
    {
        /// <summary>
        /// 序列化成为多语种对象
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static TranslateModel ToTranslateModel(this string content)
        {
            if (string.IsNullOrEmpty(content)) return new TranslateModel();
            try
            {
                return JsonConvert.DeserializeObject<TranslateModel>(content) ?? new TranslateModel();
            }
            catch
            {
                return new TranslateModel();
            }
        }
    }
}
