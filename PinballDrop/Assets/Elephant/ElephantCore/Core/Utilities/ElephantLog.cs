using UnityEngine;

namespace ElephantSDK
{
    public class ElephantLog
    {
        private static ElephantLog instance;
        private static bool isLoggingEnabled;
        private static bool isCrashlyticsInitialized;
        private static ElephantLogLevel currentLogLevel;

        public static ElephantLog GetInstance(ElephantLogLevel logLevel)
        {
#if ELEPHANT_DEBUG
            if (instance == null)
            {
                instance = new ElephantLog(ElephantLogLevel.Debug);
            }

            return instance;
#else
        if (instance == null)
        {
            instance = new ElephantLog(logLevel);
        }
        return instance;
#endif
        }

        private ElephantLog(ElephantLogLevel logLevel)
        {
            currentLogLevel = logLevel;
            isLoggingEnabled = (logLevel == ElephantLogLevel.Debug);

            try
            {
                var adapter = ElephantCore.Instance?.FirebaseElephantAdapter;
                isCrashlyticsInitialized = adapter != null && adapter.IsAvailable;
                if (isCrashlyticsInitialized)
                {
                    AddBreadcrumb("INIT", "ElephantLog", $"Initialized with level: {logLevel}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Crashlytics adapter in ElephantLog: {e.Message}");
                isCrashlyticsInitialized = false;
            }
        }

        public static void UpdateLogLevel(ElephantLogLevel logLevel)
        {
#if ELEPHANT_DEBUG
            return;
#else
        if (instance == null)
        {
            instance = new ElephantLog(logLevel);
            return;
        }
        
        if (currentLogLevel != logLevel)
        {
            currentLogLevel = logLevel;
            isLoggingEnabled = (logLevel == ElephantLogLevel.Debug);
            
            AddBreadcrumb("CONFIG", "ElephantLog", $"Log level updated from {currentLogLevel} to: {logLevel}");
            
            if (isLoggingEnabled)
            {
                Debug.Log($"<ElephantLog> Logging level changed to: {logLevel}");
            }
        }
#endif
        }

        public static void Log(string filter, string message)
        {
            if (isLoggingEnabled)
            {
                Debug.Log($"<{filter}> {message}");
            }

            AddBreadcrumb("INFO", filter, message);
        }

        public static void LogError(string filter, string message, bool isCritical = false)
        {
            if (isLoggingEnabled)
            {
                Debug.LogError($"<{filter}> {message}");
            }

            AddBreadcrumb("ERROR", filter, message);

            if (!isCritical || !isCrashlyticsInitialized) return;
            try
            {
                var exception = new System.Exception($"[{filter}] {message}");
                ElephantCore.Instance?.FirebaseElephantAdapter?.LogException(exception);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to log exception to Crashlytics: {e.Message}");
            }
        }

        private static void AddBreadcrumb(string level, string filter, string message)
        {
            if (!isCrashlyticsInitialized) return;

            try
            {
                var breadcrumb = $"{level}|{filter}|{message}";
                ElephantCore.Instance?.FirebaseElephantAdapter?.LogMessage(breadcrumb);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to add breadcrumb: {e.Message}");
            }
        }

        public static void LogCustomKey(string key, string value)
        {
            if (!isCrashlyticsInitialized) return;

            try
            {
                ElephantCore.Instance?.FirebaseElephantAdapter?.SetCustomKey(key, value);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set custom key: {e.Message}");
            }
        }
    }

    public enum ElephantLogLevel
    {
        Debug = 1,
        Prod
    }
}