using System;
using SP.StudioCore.Web;

namespace SP.StudioCore.LinkTrack.Core
{
    /// <summary>
    /// 本次操作完成
    /// </summary>
    public class TrackEnd : IDisposable
    {
        private readonly LinkTrackDetail  _linkTrackDetail;
        private readonly LinkTrackContext _linkTrackContext;

        public TrackEnd(LinkTrackContext linkTrackContext)
        {
            _linkTrackContext = linkTrackContext;
        }

        public TrackEnd(LinkTrackDetail linkTrackDetail)
        {
            _linkTrackDetail = linkTrackDetail;
        }

        /// <summary>
        /// 设置下游系统API响应内容
        /// </summary>
        public void SetDownstreamResponseBody(string responseBody)
        {
            _linkTrackContext.ResponseBody = responseBody;
        }

        /// <summary>
        /// 设置Http响应内容
        /// </summary>
        public void SetHttpResponseBody(string responseBody)
        {
            _linkTrackDetail.Data["ResponseBody"] = responseBody;
        }

        /// <summary>
        /// 完成本次追踪
        /// </summary>
        public void End()
        {
            if (_linkTrackDetail != null) _linkTrackDetail.EndTs   = WebAgent.GetTimestamps(DateTime.Now);
            if (_linkTrackContext != null) _linkTrackContext.EndTs = WebAgent.GetTimestamps(DateTime.Now);
        }

        /// <summary>
        /// 完成本次追踪
        /// </summary>
        public void Dispose()
        {
            End();
        }
    }
}