using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantIOS 
    {
        
#if UNITY_IOS
    [DllImport ("__Internal")]
    public static extern void TestFunction(string a);
    
    [DllImport ("__Internal")]
    public static extern string IDFA();
    
    [DllImport ("__Internal")]
    public static extern void ElephantPost(string url, string body, string gameID, string authToken, int tryCount);
    
    [DllImport ("__Internal")]
    public static extern void showIdfaConsent(int type, int delay, int position, string titleText, string descriptionText, 
        string buttonText, string termsText, string policyText, string termsUrl, string policyUrl);
    
    [DllImport ("__Internal")]
    public static extern string getConsentStatus();
    
    [DllImport ("__Internal")]
    public static extern string getBuildNumber();
    
    [DllImport ("__Internal")]
    public static extern string getLocale();
    
    [DllImport ("__Internal")]
    public static extern void showAlertDialog(string title, string message);
    
    [DllImport ("__Internal")]
    public static extern void showForceUpdate(string title, string message);
    
    [DllImport("__Internal")]
    public static extern void showPopUpView(string subviewType, string text, string buttonTitle, string privacyPolicyText, 
                                            string privacyPolicyUrl, string termsOfServiceText, 
                                            string termsOfServiceUrl, 
                                            string dataRequestText = "", string dataRequestUrl = "");
    
    [DllImport("__Internal")]
    public static extern void showCcpaPopUpView(string action, string title, string content, 
                       string privacyPolicyText, string privacyPolicyUrl, 
                       string declineActionButtonText, string agreeActionButtonText,
                       string backToGameActionButtonText);

    [DllImport("__Internal")]
    public static extern void showSettingsView(string subviewType, string actions, bool showCMPButton, string elephantId);
    
    [DllImport("__Internal")]
    public static extern void showBlockedPopUpView(string title, string content, string warningContent, string buttonTitle);

    [DllImport("__Internal")]
    public static extern void showNetworkOfflinePopUpView(string content, string buttonTitle);
    
    [DllImport("__Internal")]
    public static extern int gameMemoryUsage();
    
    [DllImport("__Internal")]
    public static extern int gameMemoryUsagePercent();
    
    [DllImport("__Internal")]
    public static extern float getBatteryLevel();
    
    [DllImport("__Internal")]
    public static extern long getFirstInstallTime();
    
    [DllImport("__Internal")]
    public static extern void getNotificationPermission();
    
    [DllImport("__Internal")]
    public static extern void requestLocalizedPrices(string concatenatedProductIds);
    
    [DllImport("__Internal")]
    public static extern void setAppLovinGdprConsent(bool consent);
    
    [DllImport("__Internal")]
    public static extern void setUnityAdsGdprConsent(bool consent);
    
    [DllImport("__Internal")]
    public static extern void setChartboostGdprConsent(bool consent);
    
    [DllImport("__Internal")]
    public static extern void setIronSourceGdprConsent(bool consent);
    
    [DllImport("__Internal")]
    public static extern void setPangleGdprConsent(bool consent);

    [DllImport("__Internal")]
    public static extern void showReturningUserPopUpView(string action, string title, string content, 
                       string privacyPolicyText, string privacyPolicyUrl,
                       string backToGameActionButtonText);
    
    [DllImport("__Internal")]
    public static extern bool keyExistsInKeyChain(string key);

    [DllImport("__Internal")]
    public static extern IntPtr getValueForKey(string key);

    [DllImport("__Internal")]
    public static extern void saveValueForKey(string key, string value);

    [DllImport("__Internal")]
    public static extern void showCollectiblePopUpView(string message, string buttonText);
    
    [DllImport("__Internal")]
    public static extern void openURL(string url);

    [DllImport("__Internal")]
    public static extern void showVppaDialog(string content, string buttonTitle);
#endif
    }
}
