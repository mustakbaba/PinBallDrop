using System;
using System.Collections;
using System.Globalization;
using System.Text;
using ElephantSDK;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ElephantSDK
{
    public class IapOps
    {
        public IEnumerator VerifyPurchase(IapVerifyRequest request, Action<bool> callback)
        {
            var json = JsonConvert.SerializeObject(request);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<IapVerification>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.IAP_VERIFY_EP, bodyJson, response =>
            {
                var responseData = response.data;
                if (responseData != null)
                {
                    callback(responseData.verified);
                    if (!responseData.verified) return;

                    var usdPrice = response.data.usd_price;
                    var isCvServiceEnabled =
                        RemoteConfig.GetInstance().GetBool("conversion_value_service_enabled", false);
                    ElephantCore.Instance.RollicAdsElephantAdapter?.LogLtv(usdPrice, isCvServiceEnabled);
                    ElephantCore.Instance.RollicAdsElephantAdapter?.LogIapLtv(usdPrice);
                    ElephantCore.Instance.ZyngaPublishingElephantAdapter?.LogPurchaseEvent(request, responseData);
                }
            }, s => { callback(false); }, timeout: 60);
            return postWithResponse;
        }
        
        public IEnumerator IsIapBannedRequest(Action<bool, string> callback)
        {
            var iapStatusRequest = IapStatusRequest.Create();

            var json = JsonConvert.SerializeObject(iapStatusRequest);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<IapStatusResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.IAP_STATUS_EP, bodyJson, response =>
            {
                var iapStatusResponse = response.data;
                if (iapStatusResponse != null)
                {
                    var isIapBanned = iapStatusResponse.is_banned;
                    callback(isIapBanned, iapStatusResponse.message);
                    if (iapStatusResponse.is_banned)
                    {
                        Elephant.ShowAlertDialog(iapStatusResponse.link, iapStatusResponse.message);
                    }
                }
                else
                {
                    callback(false, "Something went wrong. Please try again.");
                    ElephantLog.LogError("IAP CHECK", "iapStatusResponse is null");
                }
            }, s =>
            {
                callback(false, "Something went wrong. Please try again.");
                ElephantLog.LogError("IAP CHECK", "Request failed with error: " + s);
            });

            return postWithResponse;
        }
    }
}