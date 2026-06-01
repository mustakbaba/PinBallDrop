using System;

namespace ElephantSDK
{
    [Serializable]
    public class EventData : BaseData 
    {
        public string type;
        public int level;
        public string level_id;
        public long level_time;
        public float ltv;
        
        public string key_string1;
        public string value_string1;
        public string key_string2;
        public string value_string2;
        public string key_string3;
        public string value_string3;
        public string key_string4;
        public string value_string4;
        public string key_string5;
        public string value_string5;
        public string key_string6;
        public string value_string6;
        public string key_string7;
        public string value_string7;
        public string key_string8;
        public string value_string8;

        public string key_int1;
        public int value_int1;
        public string key_int2;
        public int value_int2;
        public string key_int3;
        public int value_int3;
        public string key_int4;
        public int value_int4;
        public string key_int5;
        public int value_int5;
        public string key_int6;
        public int value_int6;
        public string key_int7;
        public int value_int7;
        public string key_int8;
        public int value_int8;

        public string key_double1;
        public double value_double1;
        public string key_double2;
        public double value_double2;
        public string key_double3;
        public double value_double3;
        public string key_double4;
        public double value_double4;
        public string key_double5;
        public double value_double5;
        public string key_double6;
        public double value_double6;
        public string key_double7;
        public double value_double7;
        public string key_double8;
        public double value_double8;
        
        public string custom_data;
        
        private EventData()
        {
            
        }

        public static EventData CreateEventData()
        {
            var a = new EventData();
            a.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            return a;
        }
    }
}