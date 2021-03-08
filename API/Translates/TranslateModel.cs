using Newtonsoft.Json.Linq;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.API.Translates
{
    /// <summary>
    /// 从翻译接口过来的数据
    /// </summary>
    public struct TranslateModel
    {
        public int success;

        /// <summary>
        /// 频道的数据更新时间(如果正确的话则是一个时间戳）
        /// </summary>
        public string msg;

        public List<TranslateContent> info;

        public static implicit operator Dictionary<long, Dictionary<Language, string>>(TranslateModel model)
        {
            Dictionary<long, Dictionary<Language, string>> data = new Dictionary<long, Dictionary<Language, string>>();
            foreach (TranslateContent content in model.info)
            {
                if (!data.ContainsKey(content.KeyID)) data.Add(content.KeyID, new Dictionary<Language, string>());
                if (data[content.KeyID].ContainsKey(content.Language))
                {
                    data[content.KeyID][content.Language] = content.Content;
                }
                else
                {
                    data[content.KeyID].Add(content.Language, content.Content);
                }
            }
            return data;
        }
    }

    public struct TranslateContent
    {
        //{"KeyID":-7612789669940337767,"Language":0,"Content":"游客登录"}
        public TranslateContent(JObject item)
        {
            this.KeyID = item.Get<long>("KeyID");
            this.Language = (Language)item.Get<byte>("Language");
            this.Content = item.Get<string>("Content");
        }

        public long KeyID;

        public Language Language;

        public string Content;
    }
}
