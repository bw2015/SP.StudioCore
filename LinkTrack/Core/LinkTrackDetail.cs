using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SP.StudioCore.Web;

namespace SP.StudioCore.LinkTrack.Core
{
    public class LinkTrackDetail
    {
        public LinkTrackDetail()
        {
            var lstFrames = new StackTrace(true).GetFrames();
            var stack     = lstFrames?.LastOrDefault(o => o.GetFileLineNumber() != 0 && !o.GetMethod().Module.Name.Contains("SP.StudioCore") && o.GetMethod().Name!="Callback");
            if (stack != null)
            {
                var methodBase = stack.GetMethod();

                CallClass      = methodBase.DeclaringType?.Name;
                CallMethod     = methodBase.Name;
                FileLineNumber = stack.GetFileLineNumber();
                FileName       = stack.GetFileName();
            }
        }

        /// <summary>
        /// 调用类
        /// </summary>
        public string CallClass { get; }

        /// <summary>
        /// 调用方法
        /// </summary>
        public string CallMethod { get; set; }

        /// <summary>
        /// 执行文件名称
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 方法执行行数
        /// </summary>
        public int FileLineNumber { get; }

        /// <summary>
        /// 调用类型
        /// </summary>
        public EumCallType CallType { get; set; }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        public DbLinkTrackDetail DbLinkTrackDetail { get; set; }

        /// <summary>
        /// 埋点数据
        /// </summary>
        public Dictionary<string,string> Data { get; set; }

        /// <summary>
        /// 调用开始时间戳
        /// </summary>
        public long StartTs { get; set; } = WebAgent.GetTimestamps(DateTime.Now);

        /// <summary>
        /// 调用停止时间戳
        /// </summary>
        public long EndTs { get; set; }

        /// <summary>
        /// 总共使用时间毫秒
        /// </summary>
        public long UseTs => EndTs > StartTs ? EndTs - StartTs : 0;

        /// <summary>
        /// 是否执行异常
        /// </summary>
        public bool IsException { get; set; }
    }
}