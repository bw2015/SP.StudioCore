using System;
using System.Collections.Generic;
using SP.StudioCore.ElasticSearch;

namespace SP.StudioCore.LinkTrack
{
    public class LinkTrackEs : EsBase<LinkTrackEs, LinkTrackContextPO>
    {
        public LinkTrackEs(DateTime? indexDateTime = null) : base("LinkTrack", new[] {"LinkTrack"}, 0, 3, indexDateTime)
        {
        }
    }
}