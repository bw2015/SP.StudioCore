using Newtonsoft.Json;
using SP.StudioCore.API;
using SP.StudioCore.API.Ali;
using SP.StudioCore.Array;
using SP.StudioCore.Cache.Redis;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// 多语种
    /// </summary>
    //[JsonConverter(typeof(TranslateConverter))]
    public class TranslateModel : Dictionary<Language, string>
    {
        public TranslateModel() : base()
        {

        }

        public TranslateModel(TranslateModel translate)
        {
            this.Clear();
            foreach (var item in translate)
            {
                this.Add(item.Key, item.Value);
            }
        }

        public TranslateModel(Dictionary<Language, string> data)
        {
            this.Clear();
            foreach (var item in data)
            {
                this.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 全部语种的默认值（只写入英文和中文）
        /// </summary>
        /// <param name="value"></param>
        public TranslateModel(string value) : base()
        {
            this.Clear();
            this.Add(Language.CHN, value);
            this.Add(Language.ENG, value);
        }

        public TranslateModel(Language language, string value) : this()
        {
            this.Clear();
            this.Add(language, value);
        }

        /// <summary>
        /// 获取语言内容（本身语种/英文/中文）
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public string Get(Language language)
        {
            if (this == null) return string.Empty;
            if (this.ContainsKey(language)) return this[language];
            if (this.ContainsKey(Language.ENG)) return this[Language.ENG];
            if (this.ContainsKey(Language.CHN)) return this[Language.CHN];
            return string.Empty;
        }

        /// <summary>
        /// 返回简体中文内容
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            return this.Get(Language.CHN);
        }

        /// <summary>
        /// 写入语种内容
        /// </summary>
        public void Set(Language language, string content)
        {
            if (this.ContainsKey(language))
            {
                this[language] = content;
            }
            else
            {
                this.Add(language, content);
            }
        }

        /// <summary>
        /// JSON格式输出（可被反序列化）
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public JsonString ToJson()
        {
            return new JsonString(this.ToString());
        }


        public static implicit operator bool(TranslateModel model)
        {
            return model != null && model.Any();
        }

        public static implicit operator string?(TranslateModel model)
        {
            return model?.ToString();
        }

        public static implicit operator TranslateModel?(string value)
        {
            if (string.IsNullOrEmpty(value)) return new TranslateModel();
            try
            {
                return JsonConvert.DeserializeObject<TranslateModel>(value);
            }
            catch
            {
                return default;
            }
        }

        public static implicit operator RedisValue(TranslateModel model)
        {
            return model.ToString().GetRedisValue();
        }

        /// <summary>
        /// 替换语言包内容
        /// </summary>
        /// <param name="name"></param>
        /// <param name="translate"></param>
        /// <returns></returns>
        public TranslateModel Replace(string name, TranslateModel translate)
        {
            if (translate == null) return this;
            foreach (Language language in this.Keys.ToArray())
            {
                this[language] = this[language].Replace(name, translate.Get(language));
            }
            return this;
        }

        /// <summary>
        /// 固定内容替换
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TranslateModel Replace(string name, string value)
        {
            foreach (Language language in this.Keys.ToArray())
            {
                this[language] = this[language].Replace(name, value);
            }
            return this;
        }

        /// <summary>
        /// 是否执行
        /// </summary>
        /// <param name="name"></param>
        /// <param name="translate"></param>
        /// <param name="isReplace"></param>
        /// <returns></returns>
        public TranslateModel Replace(string name, TranslateModel translate, bool isReplace)
        {
            if (!isReplace) return this;
            return this.Replace(name, translate);
        }

        public static TranslateModel operator +(TranslateModel key1, TranslateModel key2)
        {
            TranslateModel translate = new();
            foreach (Language language in key1.Keys)
            {
                translate[language] = key1[language] + key2.Get(language);
            }
            return translate;
        }

        public static TranslateModel operator +(TranslateModel key1, string key)
        {
            TranslateModel translate = new();
            foreach (Language language in key1.Keys)
            {
                translate[language] = key1[language] + key;
            }
            return translate;
        }

        /// <summary>
        /// 复制自身，生成一个新的对象
        /// </summary>
        /// <returns></returns>
        public TranslateModel Clone()
        {
            return new TranslateModel(this);
        }

        /// <summary>
        /// 加入原来的内容
        /// </summary>
        /// <param name="format">一定要包含 {0} </param>
        /// <returns></returns>
        public TranslateModel ToString(string format)
        {
            if (this == null) return this;
            TranslateModel translate = new TranslateModel();
            foreach (Language language in this.Keys.ToArray())
            {
                translate.Add(language, string.Format(format, this[language]));
            }
            return translate;
        }

        /// <summary>
        /// 调用机器翻译（修改自身）
        /// </summary>
        /// <param name="setting">API参数配置</param>
        /// <param name="source">来源语言</param>
        /// <param name="languages">要翻译的语言</param>
        /// <returns></returns>
        public TranslateModel Translate(ITranslateAPI setting, Language source, params Language[] languages)
        {
            string content = this.Get(source, string.Empty);
            if (string.IsNullOrWhiteSpace(content)) return this;

            foreach (Language language in languages)
            {
                string target = this.Get(language, string.Empty);
                // 如果已有翻译则不做处理
                if (!string.IsNullOrWhiteSpace(target) && target != content) continue;

                if (setting.Execute(content, source, language, out target))
                {
                    this[language] = target;
                }
            }
            return this;
        }

    }
}
