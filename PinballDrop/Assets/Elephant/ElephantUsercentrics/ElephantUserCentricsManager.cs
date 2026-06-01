#define EXCLUDE_EXTRA_NETWORKS
using System;
using System.Collections.Generic;
using Unity.Usercentrics;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantUsercentricsManager : IUsercentricsElephantAdapter
    {
        private static bool _didCrashlyticsConsentSet = false;
        private static bool _didAnalyticsConsentSet = false;
        private static bool _didAdjustConsentSet = false;
        private static UsercentricsServiceConsent _crashlyticsConsent;
        private static UsercentricsServiceConsent _analyticsConsent;
        private static UsercentricsServiceConsent _adjustConsent;
        
        // CrashlyticsConsent property with getter and setter
        private static UsercentricsServiceConsent CrashlyticsConsent
        {
            get { return _didCrashlyticsConsentSet ? _crashlyticsConsent : null; }
            set
            {
                _crashlyticsConsent = value;
                _didCrashlyticsConsentSet = true; // Set the flag to true when the setter is used
            }
        }

        // AnalyticsConsent property with getter and setter
        private static UsercentricsServiceConsent AnalyticsConsent
        {
            get { return _didAnalyticsConsentSet ? _analyticsConsent : null; }
            set
            {
                _analyticsConsent = value;
                _didAnalyticsConsentSet = true; // Set the flag to true when the setter is used
            }
        }
        
        // AdjustContent property with getter and setter
        private static UsercentricsServiceConsent AdjustConsent
        {
            get { return _didAdjustConsentSet ? _adjustConsent : null; }
            set
            {
                _adjustConsent = value;
                _didAdjustConsentSet = true; // Set the flag to true when the setter is used
            }
        }
        
        public void InitializeUc(bool isUcEnabled, bool isEea, bool isUcForced, bool isAutoDeny, bool shouldInitWithDelay, Action<bool, bool> onInitialize,
            Action onAdjustTrackingSet, Action onLoadGame)
        {
            Usercentrics.Instance.RulesetID = "u2FxE_VG2sq_De";
            Usercentrics.Instance.Options.Android.DisableSystemBackButton = true;

            if (shouldInitWithDelay)
            {
                var cmpDelayHours = RemoteConfig.GetInstance().GetInt("cmp_delay_hour", 24);
                var currentTime = Utils.Timestamp();
                var askPermissionTime = ElephantCore.Instance.installTime + cmpDelayHours * 3600000;
                if (askPermissionTime < currentTime)
                {
                    ShowUc(isUcEnabled, isEea, isUcForced, isAutoDeny, onInitialize, onAdjustTrackingSet, onLoadGame);
                }
                else
                {
                    onInitialize?.Invoke(false, false);
                }
            }
            else
            {
                ShowUc(isUcEnabled, isEea, isUcForced, isAutoDeny, onInitialize, onAdjustTrackingSet, onLoadGame);
            }
        }

        private static void ShowUc(bool isUcEnabled, bool isEea, bool isUcForced, bool isAutoDeny, Action<bool, bool> onInitialize,
            Action onAdjustTrackingSet, Action onLoadGame)
        {
            if ((isUcEnabled && isEea) || isUcForced)
            {
                Usercentrics.Instance.Initialize(status =>
                {
                    if (status.shouldCollectConsent)
                    {
                        if (isAutoDeny)
                        {
                            Usercentrics.Instance.DenyAll();
                            onInitialize?.Invoke(true, false);
                            Usercentrics.Instance.GetTCFData(tcfData =>
                            {
                                var tcString = tcfData.tcString;
                                ElephantCore.Instance.SetTCString(tcString);
                            });
                            return;
                        }
                        
                        onInitialize?.Invoke(true, true);
                
                        var bannerSettings = new BannerSettings(firstLayerStyleSettings: new FirstLayerStyleSettings(layout: UsercentricsLayout.PopupCenter));
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            return;
                        }

                        Usercentrics.Instance.ShowFirstLayer(bannerSettings, userResponse =>
                        {
                            var userDecisionParams = Params.New();
                            userDecisionParams.Set("user_interaction", (int)userResponse.userInteraction);
                            ApplyConsentResults(userResponse, onAdjustTrackingSet, onLoadGame);
                    
                            Elephant.Event("cmp_popup_dismissed", -1, userDecisionParams);
                            
                            Usercentrics.Instance.GetTCFData(tcfData =>
                            {
                                var tcString = tcfData.tcString;
                                ElephantCore.Instance.SetTCString(tcString);
                            });
                        });
                    }
                    else
                    {
                        onInitialize?.Invoke(true, false);
                        Usercentrics.Instance.GetTCFData(tcfData =>
                        {
                            var tcString = tcfData.tcString;
                            ElephantCore.Instance.SetTCString(tcString);
                        });
                    }
                }, errorMessage =>
                {
                    onInitialize?.Invoke(false, false);
                });
            }
            else
            {
                onInitialize?.Invoke(false, false);
            }
        }

        public void ShowSecondLayer()
        {
            Usercentrics.Instance.ShowSecondLayer(GetBannerSettings(), (usercentricsConsentUserResponse) =>
            {
                UpdateServices(usercentricsConsentUserResponse.consents);
            });
        }
        
        private void UpdateServices(List<UsercentricsServiceConsent> consents)
        { 
            foreach (var consent in consents)
            {
                switch (consent.templateId)
                {
                    case "Jy6PlrM3":
                        // Adjust
                        AdjustConsent = consent;
                        break;
                    case "diWdt4yLB":
                        // Firebase Analytics
                        AnalyticsConsent = consent;
                        break;
                    case "cE0B0wy4Z":
                        // Firebase Crashlytics
                        CrashlyticsConsent = consent;
                        break;
#if !UNITY_EDITOR && UNITY_ANDROID
                    case "fHczTMzX8":
                        // AppLovin
                        ElephantAndroid.setAppLovinGdprConsent(consent.status);
                        break;
#if !EXCLUDE_EXTRA_NETWORKS
                    case "IEbRp3saT":
                        // Chartboost
                        ElephantAndroid.setChartboostGdprConsent(consent.status);
                        break;
                    case "9dchbL797":
                        // IronSource
                        ElephantAndroid.setIronSourceGdprConsent(consent.status);
                        break;
                    case "hpb62D82I":
                        // UnityAds
                        ElephantAndroid.setUnityAdsGdprConsent(consent.status);
                        break;
                    case "HWSNU_Ll1":
                        // Pangle
                        ElephantAndroid.setPangleGdprConsent(consent.status);
                        break;
#endif
                    default:
                        break;
#elif !UNITY_EDITOR && UNITY_IOS
                    case "fHczTMzX8":
                        // AppLovin
                        ElephantIOS.setAppLovinGdprConsent(consent.status);
                        break;
#if !EXCLUDE_EXTRA_NETWORKS
                    case "IEbRp3saT":
                        // Chartboost
                        ElephantIOS.setChartboostGdprConsent(consent.status);
                        break;
                    case "9dchbL797":
                        // IronSource
                        ElephantIOS.setIronSourceGdprConsent(consent.status);
                        break;
                    case "hpb62D82I":
                        // UnityAds
                        ElephantIOS.setUnityAdsGdprConsent(consent.status);
                        break;
                    case "HWSNU_Ll1":
                        // Pangle
                        ElephantIOS.setPangleGdprConsent(consent.status);
                        break;
#endif
                    default:
                        break;
#endif
                }
            }
        }
        
        private BannerSettings GetBannerSettings()
        {
            return new BannerSettings(generalStyleSettings: GetGeneralStyleSettings(),
                firstLayerStyleSettings: GetFirstLayerStyleSettings(),
                secondLayerStyleSettings: new SecondLayerStyleSettings(showCloseButton: true),
                variantName: "");
        }
        
        private GeneralStyleSettings GetGeneralStyleSettings()
        {
            return new GeneralStyleSettings(androidDisableSystemBackButton: true,
                androidStatusBarColor: "#f51d7e");
        }
        
                private FirstLayerStyleSettings GetFirstLayerStyleSettings()
        {
            var logoImageUrl = "https://drive.google.com/uc?export=download&id=1Cd6o0FBqsGVb3zZW8KRSUOmFAcCAvZ9o";
            var headerImageSettings = HeaderImageSettings.Custom(imageUrl: logoImageUrl,
                                                                 alignment: SectionAlignment.Center,
                                                                 height: 100);

            var buttons = new List<ButtonSettings>
            {
                new ButtonSettings(type: Unity.Usercentrics.ButtonType.More,
                                   textSize: 10f,
                                   textColor: "#001d3b",
                                   backgroundColor: "#00d0fc",
                                   cornerRadius: 12,
                                   isAllCaps: false),
                new ButtonSettings(type: Unity.Usercentrics.ButtonType.AcceptAll,
                                   textSize: 10f,
                                   textColor: "#001d3b",
                                   backgroundColor: "#00d0fc",
                                   cornerRadius: 12,
                                   isAllCaps: false)
            };

            var buttonLayout = ButtonLayout.Row(buttons);

            var titleSettings = new TitleSettings(textSize: 24f, alignment: SectionAlignment.Center, textColor: "#FFFFFF");

            var messageSettings = new MessageSettings(textSize: 10f,
                                                      alignment: SectionAlignment.Start,
                                                      textColor: "#e8daef",
                                                      linkTextColor: "#00d0fc",
                                                      underlineLink: true);

            return new FirstLayerStyleSettings(layout: UsercentricsLayout.PopupCenter,
                                               headerImage: headerImageSettings,
                                               title: titleSettings,
                                               message: messageSettings,
                                               buttonLayout: buttonLayout,
                                               backgroundColor: "#001d3b",
                                               cornerRadius: 30f,
                                               overlayColor: "#350aab",
                                               overlayAlpha: 0.5f);
        }

        public bool DidCrashlyticsConsentSet()
        {
            return _didCrashlyticsConsentSet;
        }

        public bool DidAnalyticsConsentSet()
        {
            return _didAnalyticsConsentSet;
        }

        public bool DidAdjustConsentSet()
        {
            return _didAdjustConsentSet;
        }

        public bool GetCrashlyticsConsentStatus()
        {
            return CrashlyticsConsent.status;
        }

        public bool GetAnalyticsConsentStatus()
        {
            return AnalyticsConsent.status;
        }

        public bool GetAdjustConsentStatus()
        {
            return AdjustConsent.status;
        }

        private static void ApplyConsentResults(UsercentricsConsentUserResponse userResponse, Action onAdjustTrackingSet, Action onLoadGame)
        {
            foreach (var consent in userResponse.consents)
            {
                switch (consent.templateId)
                {
                    case "Jy6PlrM3":
                        // Adjust
                        AdjustConsent = consent;
                        onAdjustTrackingSet?.Invoke();
                        break;
                    case "diWdt4yLB":
                        // Firebase Analytics
                        AnalyticsConsent = consent;
                        break;
                    case "cE0B0wy4Z":
                        // Firebase Crashlytics
                        CrashlyticsConsent = consent;
                        break;
#if !UNITY_EDITOR && UNITY_ANDROID
                    case "fHczTMzX8":
                        // AppLovin
                        ElephantAndroid.setAppLovinGdprConsent(consent.status);
                        break;
#if !EXCLUDE_EXTRA_NETWORKS
                    case "IEbRp3saT":
                        // Chartboost
                        ElephantAndroid.setChartboostGdprConsent(consent.status);
                        break;
                    case "9dchbL797":
                        // IronSource
                        ElephantAndroid.setIronSourceGdprConsent(consent.status);
                        break;
                    case "hpb62D82I":
                        // UnityAds
                        ElephantAndroid.setUnityAdsGdprConsent(consent.status);
                        break;
                    case "HWSNU_Ll1":
                        // Pangle
                        ElephantAndroid.setPangleGdprConsent(consent.status);
                        break;
#endif
                    default:
                        break;
#elif !UNITY_EDITOR && UNITY_IOS
                    case "fHczTMzX8":
                        // AppLovin
                        ElephantIOS.setAppLovinGdprConsent(consent.status);
                        break;
#if !EXCLUDE_EXTRA_NETWORKS
                    case "IEbRp3saT":
                        // Chartboost
                        ElephantIOS.setChartboostGdprConsent(consent.status);
                        break;
                    case "9dchbL797":
                        // IronSource
                        ElephantIOS.setIronSourceGdprConsent(consent.status);
                        break;
                    case "hpb62D82I":
                        // UnityAds
                        ElephantIOS.setUnityAdsGdprConsent(consent.status);
                        break;
                    case "HWSNU_Ll1":
                        // Pangle
                        ElephantIOS.setPangleGdprConsent(consent.status);
                        break;
#endif
                    default:
                        break;
#endif
                }
            }
            
            onLoadGame?.Invoke();
        }
        
        public bool GetIsUcInitialized()
        {
            return Usercentrics.Instance.IsInitialized;
        }
    }
}