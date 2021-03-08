using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SP.StudioCore.Cache.Memory;
using SP.StudioCore.Enums;
using SP.StudioCore.Json;
using SP.StudioCore.Net;
using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace SP.StudioCore.API.Translates
{
    /// <summary>
    /// 翻译工具
    /// </summary>
    public static class TranslateUtils
    {
        /// <summary>
        /// 远程接口地址
        /// </summary>
        private static readonly string APIURL;

        private const string KEY = "TRANSLATE";

        /// <summary>
        /// 临时标记
        /// </summary>
        private const string KEY_TEMP = "TRANSLATE_TEMP";

        /// <summary>
        /// 临时存储的新词条路径
        /// </summary>
        private static readonly Dictionary<long, string> tempData = new Dictionary<long, string>();

        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// 本地的更新时间
        /// </summary>
        private static long UPDATETIME = 0;

        static TranslateUtils()
        {
            APIURL = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .Build()["studio:language"];

            if (string.IsNullOrEmpty(APIURL)) return;

            Console.WriteLine("触发翻译器静态构造");

            Timer timer = new Timer(60 * 1000)
            {
                Enabled = true
            };
            timer.Elapsed += async (sender, e) =>
            {
                if (UPDATETIME != GetDataTime())
                {
                    Console.WriteLine("语言包存在更新");
                    Dictionary<long, Dictionary<Language, string>> data = GetAPIData();
                    if (data == null && MemoryUtils.Contains(KEY)) return;
                    MemoryUtils.Set(KEY, data ?? new Dictionary<long, Dictionary<Language, string>>(), TimeSpan.FromDays(7));
                }
                await PostWord().ConfigureAwait(false);
            };
            timer.Start();
        }


        /// <summary>
        /// 提交要保存的单个单词（异步）
        /// </summary>
        private static async Task PostWord()
        {
            if (tempData.Count == 0) return;
            Uri url = new Uri(APIURL + "&action=SaveWord");
            string postData = tempData.Select(t => t.Value).ToJson();
            Console.WriteLine("提交新词汇：" + postData);
            await client.PostAsync(url, new StringContent(postData)).ConfigureAwait(false);
            tempData.Clear();
        }

        #region ========  对外抛出的公共工具方法  ========

        /// <summary>
        /// 加载远程数据
        /// </summary>
        /// <returns></returns>
        public static Dictionary<long, Dictionary<Language, string>> GetAPIData()
        {
            if (string.IsNullOrEmpty(APIURL))
            {
                return null;
            }
            string result = NetAgent.UploadData(APIURL, "action=GetData");
            try
            {
                JObject info = JObject.Parse(result);
                if (info.Get<int>("success") == 0) return null;
                UPDATETIME = info.Get<long>("msg");
                List<TranslateContent> list = new List<TranslateContent>();
                foreach (JObject item in (JArray)info["info"])
                {
                    list.Add(new TranslateContent(item));
                }
                Dictionary<long, Dictionary<Language, string>> data = new Dictionary<long, Dictionary<Language, string>>();
                foreach (TranslateContent content in list)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(result);
                return null;
            }
        }

        /// <summary>
        /// 获取频道的更新时间（远程读取）
        /// </summary>
        /// <returns></returns>
        public static long GetDataTime()
        {
            if (string.IsNullOrEmpty(APIURL)) return 0;
            string result = NetAgent.UploadData(APIURL, "action=GetData&Time=1");
            try
            {
                JObject info = JObject.Parse(result);
                if (info.Get<int>("success") == 1)
                {
                    return info.Get<long>("msg");
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取远程接口的数据（本地缓存)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<long, Dictionary<Language, string>> GetData()
        {
            if (string.IsNullOrEmpty(APIURL)) return null;
            return MemoryUtils.Get(KEY, TimeSpan.FromDays(7), () =>
               {
                   return GetAPIData() ?? new Dictionary<long, Dictionary<Language, string>>();
               });
        }

        /// <summary>
        /// 返回翻译之后的语言包
        /// </summary>
        /// <param name="input">简体中文内容</param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string Get(this string input, Language language)
        {
            Dictionary<long, Dictionary<Language, string>> data = GetData();
            if (data == null || string.IsNullOrEmpty(input)) return input;
            string keyword = $"~{input}~";
            long hashCode = keyword.GetLongHashCode();
            if (!data.ContainsKey(hashCode))
            {
                if (!tempData.ContainsKey(hashCode)) tempData.Add(hashCode, keyword);
                return input;
            }
            if (!data[hashCode].ContainsKey(language)) return input;
            return string.IsNullOrEmpty(data[hashCode][language]) ? input : data[hashCode][language];
        }

        public static Dictionary<string, Dictionary<Language, string>> Get(string[] words, string translateUrl, Language[] languages)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
