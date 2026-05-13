namespace ElephantSDK
{
    public class NewUserRequest: BaseData
    {
        public new string locale;
        public string tc_string;
        
        public NewUserRequest()
        {
            locale = "";
        }
    }
}