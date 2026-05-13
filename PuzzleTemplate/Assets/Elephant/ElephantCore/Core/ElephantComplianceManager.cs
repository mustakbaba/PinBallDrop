using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantComplianceManager
    {
        private static ElephantComplianceManager instance;

        private OpenResponse _openResponse;

        public event Action<bool> OnCCPAStateChangeEvent;
        public event Action<bool> OnGDPRStateChangeEvent;

        public static ElephantComplianceManager GetInstance(OpenResponse openResponse)
        {
            return instance ?? (instance = new ElephantComplianceManager(openResponse));
        }

        private ElephantComplianceManager(OpenResponse openResponse)
        {
            _openResponse = openResponse;
        }

        public void UpdateOpenResponse(OpenResponse openResponse)
        {
            this._openResponse = openResponse;
        }


        #region Compliance Display

        public void ShowTosAndPp(OnOpenResult onOpen)
        {
            if (_openResponse.compliance.tos == null) return;
            
            var tos = _openResponse.compliance.tos;
            var vppa = _openResponse.compliance.vppa;
            if (tos.required)
            {
                ElephantLog.Log("ELEPHANT-FLOW", "1. ElephantComplianceManager - ShowTosAndPp - TOS REQUIRED");
                onOpen(true, tos);
                ElephantCore.Instance.ZyngaPublishingElephantAdapter?.LogShowTosEvent();
            }
            else if (ShouldShowVppa())
            {
                ShowVppa(vppa);
#if UNITY_EDITOR
                ElephantUI.Instance.LoadGameSceneWithCmp();
#endif
            }
            else
            {
                ElephantLog.Log("ELEPHANT-FLOW", "1. ElephantComplianceManager - ShowTosAndPp - TOS NOT REQUIRED");
                ElephantCore.Instance.OpenIdfaConsent();
                onOpen(false, tos);
            }
        }

        public void ShowCcpa()
        {
            if (_openResponse.compliance.ccpa == null) return;

            if (!_openResponse.compliance.ccpa.required) return;
            
            var ccpa = _openResponse.compliance.ccpa;
#if UNITY_EDITOR
// no-op
            ElephantLog.Log("COMPLIANCE","ShowCcpa Content");
#elif UNITY_ANDROID
            ElephantAndroid.ShowCcpaDialog("CCPA", ccpa.title, ccpa.content, ccpa.privacy_policy_text, 
                ccpa.privacy_policy_url, ccpa.decline_text_action_button, ccpa.agree_text_action_button, 
                ccpa.back_to_game_text_action_button);
#elif UNITY_IOS
            ElephantIOS.showCcpaPopUpView("CCPA", ccpa.title, ccpa.content, ccpa.privacy_policy_text, 
                ccpa.privacy_policy_url, ccpa.decline_text_action_button, ccpa.agree_text_action_button, 
                ccpa.back_to_game_text_action_button);
#endif
        }

        public void ShowGdprAdConsent()
        {
            if (_openResponse.compliance.gdpr_ad_consent == null) return;

            if (!_openResponse.compliance.gdpr_ad_consent.required) return;
            
            var gdpr = _openResponse.compliance.gdpr_ad_consent;
#if UNITY_EDITOR
// no-op
            ElephantLog.Log("COMPLIANCE","ShowGdprAdConsent Content");    
#elif UNITY_ANDROID
            ElephantAndroid.ShowCcpaDialog("GDPR_AD_CONSENT", gdpr.title, gdpr.content, gdpr.privacy_policy_text, 
                gdpr.privacy_policy_url, gdpr.decline_text_action_button, gdpr.agree_text_action_button, 
                gdpr.back_to_game_text_action_button);
#elif UNITY_IOS
            ElephantIOS.showCcpaPopUpView("GDPR_AD_CONSENT", gdpr.title, gdpr.content, gdpr.privacy_policy_text, 
                gdpr.privacy_policy_url, gdpr.decline_text_action_button, gdpr.agree_text_action_button, 
                gdpr.back_to_game_text_action_button);
#endif
        }

        public void ShowBlockedPopUp()
        {
            if (_openResponse.compliance.blocked == null) return;

            if (!_openResponse.compliance.blocked.is_blocked) return;
            
            var blocked = _openResponse.compliance.blocked;
            
#if UNITY_EDITOR
            // No-op
#elif UNITY_IOS
            ElephantIOS.showBlockedPopUpView(blocked.title, blocked.content, blocked.warning_text, blocked.button_text);
#elif UNITY_ANDROID
            ElephantAndroid.showBlockedDialog(blocked.title, blocked.content, blocked.warning_text, blocked.button_text);
#endif
        }
        
        public bool CheckForceUpdate()
        {
            if (!IsForceUpdateNeeded()) return false;
            
            var forceUpdateEventParams = Params.New()
                .Set("version_seen", Application.version);

            Elephant.Event("force_update_seen", -1, forceUpdateEventParams);

#if UNITY_EDITOR
            // no-op
#elif UNITY_ANDROID
                ElephantAndroid.showForceUpdate("Update needed", "Please update your application");
#elif UNITY_IOS
                ElephantIOS.showForceUpdate("Update needed", "Please update your application");
#else
                // no-op
#endif

            return true;

        }
        
        private bool IsForceUpdateNeeded()
        {
            var internalConfig = _openResponse?.internal_config;
            if (internalConfig == null) return false;

            if (string.IsNullOrEmpty(internalConfig.min_app_version)) return false;

            return VersionCheckUtils.GetInstance()
                .CompareVersions(Application.version, internalConfig.min_app_version) < 0;
            
        }
        
        private void ShowVppa(ComplianceTosResponse vppa)
        {
#if UNITY_EDITOR
            ElephantLog.Log("COMPLIANCE","Show Vppa Content: " + vppa.content);
#elif UNITY_ANDROID
            ElephantAndroid.ShowVppaDialog(vppa.content, vppa.consent_text_action_button);
#elif UNITY_IOS
            ElephantIOS.showVppaDialog(vppa.content, vppa.consent_text_action_button);
#endif
        }
        
        private void EndConsent()
        {
#if UNITY_IOS
            var isUcEnabled = ElephantCore.Instance.isUcEnabled;
            if (!isUcEnabled)
            {
                ElephantUI.Instance.PlayGame();
            }
#elif UNITY_ANDROID
            ElephantUI.Instance.PlayGame();
#endif
            
            ElephantCore.Instance.OpenIdfaConsent();
            ElephantCore.Instance.FacebookElephantAdapter?.AllowDataTracking();
            ElephantCore.Instance.ZyngaPublishingElephantAdapter?.LogAcceptTosEvent();

#if UNITY_ANDROID
            ShowCcpa();
            ShowGdprAdConsent();
#endif 
        }
        
        private bool ShouldShowVppa()
        {
            if (!RemoteConfig.GetInstance().GetBool("show_vppa", true))
                return false;
        
            var vppa = _openResponse.compliance.vppa;
            return vppa != null && vppa.required;
        }

        #endregion

        #region Compliance Results

        public void SendTosAccept()
        {
            var data = new ComplianceRequestData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().session_id);
            var request = new ElephantRequest(ElephantConstants.TOS_ACCEPT_EP, data);
            
            ElephantCore.Instance.AddToQueue(request);
            
            var vppa = _openResponse.compliance.vppa;
            if (ShouldShowVppa())
            {
                ShowVppa(vppa);
            }
            else
            {
                EndConsent();
            }
        }
        
        public void SendVppaAccept()
        {
            var data = new ComplianceRequestData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().session_id);
            var request = new ElephantRequest(ElephantConstants.VPPA_ACCEPT_EP, data);
            
            ElephantCore.Instance.AddToQueue(request);
            
            EndConsent();
        }
        
        public void SendCcpaStatus(bool accepted)
        {
            var data = CcpaGdprStatusRequestData.CreateCcpaGdprStatusRequestData(accepted);
            var request = new ElephantRequest(ElephantConstants.CCPA_STATUS, data);
            
            SetFirebaseConsentForCcpa(accepted);
            SetTrackThirdPartySharingForCcpa(accepted);

            ElephantCore.Instance.AddToQueue(request);
            instance.OnCCPAStateChangeEvent(accepted);
        }
        
        private void SetFirebaseConsentForCcpa(bool accepted)
        {
            ElephantCore.Instance.FirebaseElephantAdapter?.SetConsentForCcpa(accepted);
        }
        
        private void SetTrackThirdPartySharingForCcpa(bool accepted)
        {
            ElephantCore.Instance.AdjustElephantAdapter?.SetTrackThirdPartySharingForCcpa(accepted);
        }
        
        public void SendGdprAdConsentStatus(bool accepted)
        {
            var data = CcpaGdprStatusRequestData.CreateCcpaGdprStatusRequestData(accepted);
            var request = new ElephantRequest(ElephantConstants.GDPR_AD_CONSENT, data);
            
            ElephantCore.Instance.AddToQueue(request);
            instance.OnGDPRStateChangeEvent(accepted);
        }

        #endregion
    }
}