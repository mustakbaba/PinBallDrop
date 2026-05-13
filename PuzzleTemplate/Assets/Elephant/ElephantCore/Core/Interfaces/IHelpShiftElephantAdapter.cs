namespace ElephantSDK
{
    public interface IHelpShiftElephantAdapter : IElephantAdapter
    {
        void Init(string domainName, string appId);
        void ShowConversation();
        void ShowFAQs();
    }
}