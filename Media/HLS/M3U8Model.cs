using SP.StudioCore.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.StudioCore.Media.HLS
{
    /// <summary>
    /// m3u8格式的输出文件
    /// </summary>
    public class M3U8Model
    {
        public M3U8Model()
        {
            this.INFLIST = new List<INF>();
        }

        /// <summary>
        /// 指定日期范围的ID
        /// </summary>
        /// <param name="dateRangeId"></param>
        public M3U8Model(string dateRangeId, int sequence, string baseUrl, TimeSpan? time = null) : this()
        {
            this.DATERANGE_ID = dateRangeId;
            this.EXT_X_MEDIA_SEQUENCE = sequence;
            this.BaseURL = baseUrl;
            this.Time = time;
        }


        /// <summary>
        /// 表示 HLS 的协议版本号，该标签与流媒体的兼容性相关。该标签为全局作用域，使能整个 m3u8 文件；每个 m3u8 文件内最多只能出现一个该标签定义。如果 m3u8 文件不包含该标签，则默认为协议的第一个版本。
        /// </summary>
        private const int VERSION = 3;

        /// <summary>
        /// EXTINF的最大值指定,但是如果兼容版本号 EXT-X-VERSION 小于3，那么必须使用整型
        /// </summary>
        private const int TARGETDURATION = 6;

        /// <summary>
        /// 指示出现在播放列表文件中的第一个URL的序列号。播放列表中的每个媒体文件URL都有一个唯一的整数序列号。URL的序列号比其前面的URL的序列号高1。媒体序号与文件名无关。
        /// </summary>
        public int EXT_X_MEDIA_SEQUENCE { get; set; }

        /// <summary>
        /// 片段的前缀地址
        /// </summary>
        public string BaseURL { get; private set; }

        /// <summary>
        /// 时间偏移量
        /// </summary>
        public TimeSpan? Time { get; private set; }

        public string DATERANGE_ID { get; set; }

        /// <summary>
        /// 片段列表
        /// </summary>
        public List<INF> INFLIST { get; private set; }

        /// <summary>
        /// 插入资源
        /// </summary>
        /// <param name="inf"></param>
        public void Push(INF inf)
        {
            INFLIST.Add(inf);
        }

        public override string ToString()
        {
            DateTime? startDate = this.INFLIST.Min(t => t.PROGRAM_DATE_TIME);
            StringBuilder sb = new StringBuilder()
                .AppendLine("#EXTM3U")
                .AppendLine($"#EXT-X-VERSION:{VERSION}")
                .AppendLine($"#EXT-X-TARGETDURATION:{TARGETDURATION}")
                .AppendLine($"#EXT-X-MEDIA-SEQUENCE:{EXT_X_MEDIA_SEQUENCE}");
            if (!string.IsNullOrEmpty(DATERANGE_ID) && startDate != null)
            {
                if (this.Time != null) startDate = startDate.Value.Add(this.Time.Value);
               // sb.AppendLine($"EXT-X-DATERANGE:ID=\"{this.DATERANGE_ID}\",START-DATE=\"{startDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}\",END-ON-NEXT=YES");
            }
            foreach (INF inf in INFLIST)
            {
                sb.AppendLine(inf.ToString(this.BaseURL, this.Time));
            }

            return sb.ToString();
        }

        public struct INF
        {
            public INF(INF inf)
            {
                this.EXTINF = inf.EXTINF;
                this.EXTINF_TYPE = inf.EXTINF_TYPE;
                this.PROGRAM_DATE_TIME = inf.PROGRAM_DATE_TIME;
                this.URL = inf.URL;
            }

            /// <summary>
            /// 指定片段的时长
            /// </summary>
            public float EXTINF;

            /// <summary>
            /// 片段类型（直播为live）
            /// </summary>
            public string EXTINF_TYPE;

            /// <summary>
            /// 该标签使用一个绝对日期/时间表明第一个样本片段的取样时间。
            /// 如果为空则使用 EXT-X-PROGRAM-DATE-TIME 标签
            /// </summary>
            public DateTime? PROGRAM_DATE_TIME;

            /// <summary>
            /// 片段的链接地址
            /// </summary>
            public string URL;

            /// <summary>
            /// 输出
            /// </summary>
            /// <param name="baseUrl">URL前缀</param>
            /// <param name="time">时间便宜</param>
            /// <returns></returns>
            public string ToString(string baseUrl, TimeSpan? time)
            {
                StringBuilder sb = new StringBuilder();
                if (PROGRAM_DATE_TIME != null)
                {
                    if (time != null) PROGRAM_DATE_TIME = PROGRAM_DATE_TIME.Value.Add(time.Value);
                  //  sb.AppendLine($"#EXT-X-PROGRAM-DATE-TIME:{this.PROGRAM_DATE_TIME.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff")}Z");
                }
                sb.AppendLine($"#EXTINF:{this.EXTINF.ToString("0.000")},{this.EXTINF_TYPE}")
                .Append(baseUrl + this.URL);
                return sb.ToString();
            }
        }


        /// <summary>
        /// m3u8文件的解析
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static M3U8Model Parse(string content)
        {
            var lines = content.Split('\n');
            if (!lines.Any())
            {
                throw new InvalidOperationException("The content is Empty");
            }

            string firstLine = lines[0];
            if (firstLine != "#EXTM3U")
            {
                throw new InvalidOperationException("The provided URL does not link to a well-formed M3U8 playlist.");
            }

            Regex regex = new Regex(@"^\#[\w\-]+:", RegexOptions.IgnoreCase);

            M3U8Model m3u8 = new M3U8Model();
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!regex.IsMatch(line)) continue;
                string tag = regex.Match(line).Value;

                switch (tag)
                {
                    case "#EXTINF:":
                        string preLine = lines[i - 1];
                        string nextLine = lines[i + 1];
                        INF inf = new INF()
                        {
                            URL = nextLine,
                            EXTINF = Regex.Match(line, @"^\#EXTINF:(?<Value>[\d\.]+)").Groups["Value"].Value.GetValue<float>(),
                            EXTINF_TYPE = "live"
                        };
                        if (preLine.StartsWith("#EXT-X-PROGRAM-DATE-TIME:"))
                        {
                            inf.PROGRAM_DATE_TIME = preLine.Replace("#EXT-X-PROGRAM-DATE-TIME:", string.Empty).GetValue<DateTime>();
                        }
                        m3u8.INFLIST.Add(inf);
                        break;
                }
            }
            return m3u8;
        }
    }


}
