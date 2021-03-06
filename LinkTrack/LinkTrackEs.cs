using System;
using System.Collections.Generic;
using SP.StudioCore.ElasticSearch;

namespace SP.StudioCore.LinkTrack
{
    public class LinkTrackEs : EsBase<LinkTrackEs, LinkTrackContextPO>
    {
        public LinkTrackEs(DateTime? indexDateTime = null) : base("link_track", new[] {"link_track"}, 0, 3, indexDateTime)
        {
        }
    }
}