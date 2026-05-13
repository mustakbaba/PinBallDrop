using System.Collections;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class VitalOps
    {
        public IEnumerator CheckApiHealth()
        {
            var bodyJson =
                JsonConvert.SerializeObject(new ElephantData("", ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<HealthCheckResponse>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.HEALTH_CHECK_EP, bodyJson, response =>
            {
                if (response.responseCode != 200)
                {
                    ElephantCore.Instance.circuitBreakerEnabled = true;
                }
                else
                {
                    ElephantCore.Instance.circuitBreakerEnabled = false;
                    if (response.data == null) return;

                    ElephantCore.Instance.healthCheckRetryPeriod = response.data.retry_period;
                    ElephantCore.Instance.failRetryCount = response.data.retry_count;
                }
            }, s => { });

            return postWithResponse;
        }
    }
}