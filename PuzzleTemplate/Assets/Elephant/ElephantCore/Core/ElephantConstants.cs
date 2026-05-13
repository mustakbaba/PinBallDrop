namespace ElephantSDK
{
    public class ElephantConstants
    {
        #region EndPoints

        public const string ELEPHANT_BASE_URL = "https://newapi.rollic.gs/v3";
        private const string ElephantBaseUrlv4 = "https://newapi.rollic.gs/v4";
        public const string LIVEOPS_BASE_URL = "https://liveopsapi.rollic.gs/api/v2";

        public const string OPEN_EP = ELEPHANT_BASE_URL + "/open";
        public const string USER_EP = ELEPHANT_BASE_URL + "/user";
        public const string UserEpV4 = ElephantBaseUrlv4 + "/user";
        public const string EVENT_EP = ELEPHANT_BASE_URL + "/event";
        public const string AD_EVENT_EP = ELEPHANT_BASE_URL + "/ad_event";
        public const string SESSION_EP = ELEPHANT_BASE_URL + "/session";
        public const string MONITORING_EP = ELEPHANT_BASE_URL + "/monitoring";
        public const string TRANSACTION_EP = ELEPHANT_BASE_URL + "/transaction";
        public const string IAP_STATUS_EP = ELEPHANT_BASE_URL + "/user/iap/status";
        public const string IAP_VERIFY_EP = ELEPHANT_BASE_URL + "/iap/verify";
        public const string PIN_EP = ELEPHANT_BASE_URL + "/gdpr/pin";
        public const string TOS_ACCEPT_EP = ELEPHANT_BASE_URL + "/tos/accept";
        public const string VPPA_ACCEPT_EP = ELEPHANT_BASE_URL + "/vppa/accept";
        public const string CCPA_STATUS = ELEPHANT_BASE_URL + "/ccpa/status";
        public const string GDPR_AD_CONSENT = ELEPHANT_BASE_URL + "/gdpr/status";
        public const string SETTINGS_EP = ELEPHANT_BASE_URL + "/settings";
        public const string StorageDownloadEp = ELEPHANT_BASE_URL + "/storage/download";
        public const string StorageSyncEp = ELEPHANT_BASE_URL + "/storage/sync";
        public const string CollectibleEp = LIVEOPS_BASE_URL + "/collectibles";
        public const string OFFER_EP = LIVEOPS_BASE_URL + "/offers";
        public const string HEALTH_CHECK_EP = "https://newapi.rollic.gs/health_check";
        public const string NOTIFICATION_EP = "https://notificationapi.rollic.gs/api/v1/register";
        public const string OFFERURLS_EP = LIVEOPS_BASE_URL + "/offers/assets";
        public const string TIME_EP = "https://newapi.rollic.gs/v3/time";
        public const string LOGICS_EP = ELEPHANT_BASE_URL + "/event/retrieve";
        public const string ZYNGA_PLAYER_ID_EP = ELEPHANT_BASE_URL + "/user/zynga_id";
        
        public const string MDS_EP = LIVEOPS_BASE_URL + "/mds";
        public const string MDS_AUTH_EP = ELEPHANT_BASE_URL + "/auth/login";

        #endregion

        #region Keys

        public static string REMOTE_CONFIG_FILE = "ELEPHANT_REMOTE_CONFIG_DATA";
        public static string FIRST_OPEN_TIME = "ELEPHANT_FIRST_OPEN_TIME";
        public static string USER_DB_ID = "USER_DB_ID";
        public static string CACHED_OPEN_RESPONSE = "CACHED_OPEN_RESPONSE";
        public static string OFFLINE_FLAG = "OFFLINE_FLAG";
        public static string FIRST_OPEN_TS = "FIRST_OPEN_TS";
        public static string INSTALL_TIME_FOR_CV = "INSTALL_TIME_FOR_CV";
        public static string TimeSpend = "Time_Spend";
        public static string STORAGE_VERSION = "STORAGE_VERSION";
        public static string STORAGE_LOCAL = "ELEPHANT_STORAGE_LOCAL";
        public static string TC_STRING = "ELEPHANT_TC_STRING";

        #endregion
    }
}