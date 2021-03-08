using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SP.StudioCore.Utils;
using SP.StudioCore.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Model
{
    /// <summary>
    /// 错误信息的消息队列
    /// </summary>
    public struct ErrorLogModel
    {
        public ErrorLogModel(Exception ex, int siteId = 0, int userId = 0, HttpContext context = null) : 
            this(ex.Message, ErrorHelper.GetExceptionContent(ex, context), siteId, userId, context)
        {

        }

        public ErrorLogModel(string title, string content, int siteId, int userId, HttpContext context = null) : this()
        {
            this.RequestID = Guid.NewGuid();
            this.SiteID = siteId;
            this.UserID = userId;
            this.Title = title;
            this.Content = content;
            this.Url = context == null ? "" : context.Request.Path.ToString();
            this.CreateAt = DateTime.Now;
            this.IP = IPAgent.IP;
        }

        /// <summary>
        /// 错入日志编号
        /// </summary>
        public Guid RequestID;

        public int SiteID;

        public int UserID;

        /// <summary>
        /// 异常标题
        /// </summary>
        public string Title;

        /// <summary>
        /// 详细错误内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 请求地址（如果非web程序则是当前程序运行的路径）
        /// </summary>
        public string Url;

        /// <summary>
        /// 错误发生的时间
        /// </summary>
        public DateTime CreateAt;

        /// <summary>
        /// 错误发生的请求IP地址
        /// </summary>
        public string IP;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
