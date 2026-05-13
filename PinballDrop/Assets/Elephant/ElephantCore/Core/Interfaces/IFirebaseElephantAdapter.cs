using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    public interface IFirebaseElephantAdapter : IElephantAdapter
    {
        // Lifecycle
        void InitializeFirebase(Action onInitialized);
        bool IsAvailable { get; }

        // Analytics
        void SetAnalyticsCollectionEnabled(bool enabled);
        void GetAnalyticsInstanceId(Action<string> callback);
        void LogEvent(string name, IDictionary<string, object> parameters);
        void SetAnalyticsConsent(bool granted);
        void SetConsentForCcpa(bool accepted);

        // Crashlytics
        void SetCrashlyticsUserId(string userId);
        void SetCrashlyticsCollectionEnabled(bool enabled);
        void SetCustomKey(string key, string value);
        void LogMessage(string message);
        void LogException(Exception exception);
    }
}