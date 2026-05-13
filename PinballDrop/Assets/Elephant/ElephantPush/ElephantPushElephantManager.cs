using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantPushElephantManager : IPushElephantAdapter
    {
        private string _deviceToken;
        
        public void AskPushPermission()
        {
            ElephantLog.Log("PUSH-ELEPHANT", "AskPushPermission is Called");

            var pushDelay = RemoteConfig.GetInstance().GetInt("push_delay_hour", 24);
            var currentTime = Utils.Timestamp();
            var askPermissionTime = ElephantCore.Instance.installTime + pushDelay * 3600000;
            if (askPermissionTime < currentTime)
            {
#if UNITY_EDITOR
                SetDeviceToken("EditorDeviceToken");
#elif UNITY_IOS
                ElephantIOS.getNotificationPermission();
#elif UNITY_ANDROID
                ElephantAndroid.getNotificationPermission();
                ElephantAndroid.getToken();
#endif
            }
        }

        private IEnumerator SendDeviceToken()
        {
            yield return new WaitUntil(() => ElephantCore.Instance.sdkIsReady);
            try
            {
                var data = DeviceTokenRequest.CreateDeviceTokenRequest(_deviceToken);
                var json = JsonConvert.SerializeObject(data);
                var bodyJson =
                    JsonConvert.SerializeObject(new ElephantData(json,
                        ElephantCore.Instance.GetCurrentSession().GetSessionID()));

                ElephantLog.Log("SendDeviceToken", bodyJson);

                var networkManager = new GenericNetworkManager<OpenResponse>();
                var postWithResponse = networkManager.PostWithResponse(ElephantConstants.NOTIFICATION_EP, bodyJson,
                    response =>
                    {
                        ElephantLog.Log("SendDeviceToken", response.responseCode.ToString());
                        var parameters = Params.New();
                        parameters.Set("device_token", _deviceToken);
                        Elephant.Event("elephant_send_device_token", -1, parameters);
                    }, s => { ElephantLog.Log("SendDeviceToken", s); });

                ElephantCore.Instance.StartCoroutine(postWithResponse);
            }
            catch (Exception e)
            {
                ElephantLog.Log("SendDeviceToken", e.Message);
            }
        }

        public void SetDeviceToken(string token)
        {
            ElephantLog.Log("PUSH-ELEPHANT", "SetDeviceToken is Called");

            ElephantLog.Log("SetDeviceToken", token);
            _deviceToken = token;
            ElephantCore.Instance.StartCoroutine(SendDeviceToken());
        }

        public void SendPushNotificationOpenEvent(string combinedIds)
        {
            ElephantLog.Log("PUSH-ELEPHANT", "SendPushNotificationOpenEvent is Called");

            var ids = combinedIds.Split(';');
            if (ids.Length >= 4)
            {
                var notificationId = ids[0];
                var messageId = ids[1];
                var jobId = ids[2];
                var scheduledAt = ids[3];

                var parameters = Params.New();
                parameters.Set("notification_id", notificationId);
                parameters.Set("message_id", messageId);
                parameters.Set("job_id", jobId);
                parameters.Set("scheduled_at", scheduledAt);

                Elephant.Event("elephant_push_notification_open", -1, parameters);
            }
            else
            {
                ElephantLog.Log("SendPushNotificationOpenEvent", "Invalid combinedIds format");
            }
        }
        
        public void ReceiveNotificationPermission(string response)
        {
            ElephantLog.Log("PUSH-ELEPHANT", "ReceiveNotificationPermission is Called");

            var parameter = Params.New();
            parameter.Set("status", response);
            Elephant.Event("push_notification_permission", -1, parameter);
        }
    }
}