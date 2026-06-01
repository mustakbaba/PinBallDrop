using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class UserOps
    {
        private bool UseNewPopupSystem(PopupType popupType)
        {
            return Elephant.UseNewPopupSystem(popupType);
        }

        public IEnumerator CreateOrGetNewUser(Action<GenericResponse<OpenResponse>> onResponse, Action<string> onError)
        {
            var data = new NewUserRequest();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            var usercentricsElephantAdapter = ElephantCore.Instance.UsercentricsElephantAdapter;
            data.tc_string = ElephantCore.Instance.GetTCString();


            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<OpenResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.UserEpV4, bodyJson, onResponse, onError);

            ElephantCore.Instance.userId = "";
            Utils.SaveToFile(ElephantConstants.USER_DB_ID, "");
#if UNITY_IOS && !UNITY_EDITOR
            KeyChainUtils.SaveValue(ElephantConstants.USER_DB_ID, "");
#endif
            
            return postWithResponse;
        }
        
        public IEnumerator PinRequest()
        {
            if (UseNewPopupSystem(PopupType.Loading))
            {
                ElephantLog.Log("ELEPHANT", "ShowLoading - Using New Popup System");
                LoadingPopup loadingPopup = ElephantPopupManager.Instance.ShowPopup<LoadingPopup>("ElephantUI/Loading/LoadingPopup");
                if (loadingPopup != null)
                {
                    loadingPopup.Initialize();
                }

            }
            else
            {
#if UNITY_EDITOR
                ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Loading");
#elif UNITY_IOS
                ElephantIOS.showPopUpView("LOADING", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
                ElephantAndroid.ShowConsentDialogOnUiThread("LOADING", "", "", "", "", "", "", "", "");
#endif
            }

            var data = new ComplianceRequestData();
            var json = JsonConvert.SerializeObject(data);
            var bodyJson =
                JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<Pin>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.PIN_EP, bodyJson, response =>
            {
                var pinData = response.data;

                if (pinData != null)
                {
                    if (UseNewPopupSystem(PopupType.Pin))
                    {
                        ElephantPopupManager.Instance.CloseCurrentPopup();
                    	ElephantLog.Log("ELEPHANT", "ShowPin - Using New Popup System");
                        PINPopup popup = ElephantPopupManager.Instance.ShowPopup<PINPopup>("ElephantUI/PIN/PinPopup");
                        if (popup != null)
                        {
                            popup.Initialize(
                                pinData,
                                "Go Back",
                                () => { ElephantPopupManager.Instance.CloseCurrentPopup(); }
                            );
                        }
                    }
                    else
                    {
                        // Old native system - Show content
#if UNITY_EDITOR
                        ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Content");
#elif UNITY_IOS
                        ElephantIOS.showPopUpView("CONTENT", pinData.content, "Go Back", pinData.privacy_policy_text, pinData.privacy_policy_url,
                                pinData.terms_of_service_text, pinData.terms_of_service_url, pinData.data_request_text,
                                pinData.data_request_url);
#elif UNITY_ANDROID
                        ElephantAndroid.ShowConsentDialogOnUiThread("CONTENT", pinData.content, "Go Back",
                                pinData.privacy_policy_text, pinData.privacy_policy_url, pinData.terms_of_service_text,
                                pinData.terms_of_service_url, pinData.data_request_text, pinData.data_request_url);
#endif
                    }
                }
                else
                {
                    if (UseNewPopupSystem(PopupType.Error))
                    {
                        ElephantPopupManager.Instance.CloseCurrentPopup();
                    	ElephantLog.Log("ELEPHANT", "ShowError - Using New Popup System");
                        ErrorPopup errorPopup = ElephantPopupManager.Instance.ShowPopup<ErrorPopup>("ElephantUI/Error/ErrorPopup");
                        if (errorPopup != null)
                        {
                            errorPopup.Initialize(
                                "Something went wrong.\nPlease try again.",
                                "OK",
                                () => { ElephantPopupManager.Instance.CloseCurrentPopup(); }
                            );
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Error");
#elif UNITY_IOS
                        ElephantIOS.showPopUpView("ERROR", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
                        ElephantAndroid.ShowConsentDialogOnUiThread("ERROR", "", "", "", "", "", "", "", "");
#endif
                    }
                }
            }, s =>
            {
                if (UseNewPopupSystem(PopupType.Error))
                {
                    ElephantPopupManager.Instance.CloseCurrentPopup();
                    ElephantLog.Log("ELEPHANT", "ShowError - Using New Popup System");
                    ErrorPopup errorPopup = ElephantPopupManager.Instance.ShowPopup<ErrorPopup>("ElephantUI/Error/ErrorPopup");
                    if (errorPopup != null)
                    {
                        errorPopup.Initialize(
                            "Something went wrong.\nPlease try again.",
                            "OK",
                            () => { ElephantPopupManager.Instance.CloseCurrentPopup(); }
                        );
                    }
                }
                else
                {
#if UNITY_EDITOR
                    ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Error");
#elif UNITY_IOS
                    ElephantIOS.showPopUpView("ERROR", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
                    ElephantAndroid.ShowConsentDialogOnUiThread("ERROR", "", "", "", "", "", "", "", "");
#endif
                }
            });
            
            return postWithResponse;
        }
    }
}