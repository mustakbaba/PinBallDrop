using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class UserOps
    {
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
#if UNITY_EDITOR
            // No-op
            ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Loading");
#elif UNITY_IOS
            ElephantIOS.showPopUpView("LOADING", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
            ElephantAndroid.ShowConsentDialogOnUiThread("LOADING", "", "", "", "", "", "", "", "");
#endif

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
#if UNITY_EDITOR
                    // No-op
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
                else
                {
#if UNITY_EDITOR
                    // No-op
                    ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Error");
#elif UNITY_IOS
                    ElephantIOS.showPopUpView("ERROR", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
                    ElephantAndroid.ShowConsentDialogOnUiThread("ERROR", "", "", "", "", "", "", "", "");
#endif
                }
            }, s =>
            {
#if UNITY_EDITOR
                // No-op
                ElephantLog.Log("COMPLIANCE TEST", "showPopUpView Error");
#elif UNITY_IOS
                ElephantIOS.showPopUpView("ERROR", "", "", "", "", "", "", "", "");
#elif UNITY_ANDROID
                ElephantAndroid.ShowConsentDialogOnUiThread("ERROR", "", "", "", "", "", "", "", "");
#endif
            });
            
            return postWithResponse;
        }
    }
}