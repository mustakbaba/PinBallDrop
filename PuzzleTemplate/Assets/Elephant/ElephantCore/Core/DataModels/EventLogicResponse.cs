using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    [Serializable]
    public class Events
    {
        public string token;
        public string condition;
        public bool unique = true;
    }

    [Serializable]
    public class EventLogicResponse
    {
        public List<Events> events;
    }
}