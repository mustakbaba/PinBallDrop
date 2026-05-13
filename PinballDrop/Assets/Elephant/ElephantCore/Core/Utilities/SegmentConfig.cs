using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ElephantSDK
{
    [Serializable]
    public class SegmentConfig
    {
        public List<SegmentGroup> segments;
    }

    [Serializable]
    public class SegmentGroup
    {
        public string name;
        [JsonProperty("segment_id")]
        public int segmentId;
        public List<SegmentCategory> categories;
    }

    [Serializable]
    public class SegmentCategory
    {
        public string name;
        [JsonProperty("category_id")]
        public int categoryId;
        public string condition;
    }
}