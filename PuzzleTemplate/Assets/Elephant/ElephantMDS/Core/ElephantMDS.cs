using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantMDS
    {
        private static IMDSRewardProcessor _rewardProcessor;
        private static bool _isInitialized;
        
        public static System.Action<MDSCollectible> OnCollectibleReceived;
        public static System.Action<string> OnError;

        public static void Initialize(IMDSRewardProcessor rewardProcessor)
        {
            if (_isInitialized) return;
            
            _rewardProcessor = rewardProcessor;
            
            Application.deepLinkActivated += ProcessDeepLink;
            
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                ProcessDeepLink(Application.absoluteURL);
            }
            
            _isInitialized = true;
            Debug.Log("[ElephantMDS] Initialized");
            
            RequestCollectibles();
        }

        public static void RequestCollectibles()
        {
            if (ElephantCore.Instance == null)
            {
                OnError?.Invoke("ElephantCore not initialized");
                return;
            }

            var data = MDSRequest.Create();
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            
            var networkManager = new GenericNetworkManager<List<MDSCollectible>>();
            var coroutine = networkManager.PostWithResponse(
                ElephantConstants.MDS_EP,
                bodyJson,
                OnCollectiblesReceived,
                error => OnError?.Invoke($"Failed to fetch collectibles: {error}")
            );

            ElephantCore.Instance.StartCoroutine(coroutine);
        }

        public static void NotifyCollectibleClaimed(string collectibleId)
        {
            var collectibleOps = new CollectibleOps();
            ElephantCore.Instance.StartCoroutine(collectibleOps.NotifyClaimed(collectibleId));
        }

        public static void ProcessDeepLink(string url)
        {
            Debug.Log($"[ElephantMDS] Processing deep link: {url}");
            
            var param = Params.New().Set("url", url);
            Elephant.Event("mds_deeplink", -1, param);

            try
            {
                var uri = new System.Uri(url);
                var path = uri.AbsolutePath;
                
                if (path.Contains("/store-open/"))
                {
                    HandleStoreOpen(uri, url);
                }
                else if (path.Contains("/store-return/"))
                {
                    HandleStoreReturn();
                }
            }
            catch (System.Exception ex)
            {
                OnError?.Invoke($"Deep link processing failed: {ex.Message}");
            }
        }
        
        public static void OpenWebstore(string webstoreDomain, string landingPath = "/", Dictionary<string, string> additionalParams = null)
        {
            RequestAuthentication(
                token => {
                    var url = BuildWebstoreUrl(webstoreDomain, token, landingPath, additionalParams);
                    Debug.Log($"[ElephantMDS] Opening webstore: {url}");
                    Application.OpenURL(url);
                },
                error => Debug.LogError($"[ElephantMDS] Failed to open webstore: {error}")
            );
        }
        
        public static void OpenDirectCheckout(string webstoreDomain, string productId, string checkoutKey = null, Dictionary<string, string> additionalParams = null)
        {
            RequestAuthentication(
                token => {
                    var checkoutParams = new Dictionary<string, string>(additionalParams ?? new Dictionary<string, string>())
                    {
                        ["landing"] = "/checkout",
                        ["product_id"] = productId,
                        ["utm_source"] = "game_client"
                    };

                    if (!string.IsNullOrEmpty(checkoutKey))
                    {
                        checkoutParams["checkout_key"] = checkoutKey;
                    }

                    var url = BuildWebstoreUrl(webstoreDomain, token, null, checkoutParams);
                    Debug.Log($"[ElephantMDS] Opening direct checkout: {url}");
                    
                    // Track the checkout event
                    var eventParams = Params.New()
                        .Set("product_id", productId)
                        .Set("source", "direct_checkout");
                    Elephant.Event("mds_checkout_opened", -1, eventParams);
                    
                    Application.OpenURL(url);
                },
                error => Debug.LogError($"[ElephantMDS] Failed to open checkout: {error}")
            );
        }
        
        private static void RequestAuthentication(System.Action<string> onSuccess, System.Action<string> onError = null)
        {
            if (ElephantCore.Instance == null)
            {
                onError?.Invoke("ElephantCore not initialized");
                return;
            }

            var data = MDSRequest.Create();
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            
            var networkManager = new GenericNetworkManager<MDSAuthResponse>();
            var coroutine = networkManager.PostWithResponse(
                ElephantConstants.MDS_AUTH_EP,
                bodyJson,
                response => {
                    if (response?.data?.token != null)
                    {
                        onSuccess?.Invoke(response.data.token);
                    }
                    else
                    {
                        var error = "Authentication failed: No token received";
                        OnError?.Invoke(error);
                        onError?.Invoke(error);
                    }
                },
                error => {
                    OnError?.Invoke($"Authentication failed: {error}");
                    onError?.Invoke(error);
                }
            );

            ElephantCore.Instance.StartCoroutine(coroutine);
        }
        
        private static string BuildWebstoreUrl(string webstoreDomain, string token, string landingPath = null, Dictionary<string, string> additionalParams = null)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append($"https://{webstoreDomain}/connect/gateway?");
            builder.Append($"token={token}");

            if (!string.IsNullOrEmpty(landingPath) && (additionalParams == null || !additionalParams.ContainsKey("landing")))
            {
                builder.Append($"&landing={System.Uri.EscapeDataString(landingPath)}");
            }

            var defaultParams = new Dictionary<string, string>
            {
                ["utm_source"] = "LCM",
                ["utm_medium"] = "IAM",
                ["locale"] = "en_US"
            };

            foreach (var param in defaultParams)
            {
                if (additionalParams == null || !additionalParams.ContainsKey(param.Key))
                {
                    builder.Append($"&{param.Key}={param.Value}");
                }
            }

            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    builder.Append($"&{param.Key}={System.Uri.EscapeDataString(param.Value)}");
                }
            }

            return builder.ToString();
        }

        private static void OnCollectiblesReceived(GenericResponse<List<MDSCollectible>> response)
        {
            if (response.data == null) 
            {
                Debug.Log("[ElephantMDS] No collectibles received");
                return;
            }

            foreach (var collectible in response.data)
            {
                var param = Params.New()
                    .Set("collectible_id", collectible.id)
                    .Set("product_id", collectible.mds?.product_id ?? "unknown");
                Elephant.Event("mds_collectible_received", -1, param);
                
                _rewardProcessor.ProcessCollectible(collectible);
                
                OnCollectibleReceived?.Invoke(collectible);
            }
        }

        private static void HandleStoreOpen(System.Uri uri, string originalUrl)
        {
            var query = uri.Query;
            var ztMatch = Regex.Match(query, @"zt=([-]?\d+)");

            if (ztMatch.Success)
            {
                var ztValue = ztMatch.Groups[1].Value;
                AuthenticateAndRedirect(ztValue, originalUrl);
            }
        }

        private static void HandleStoreReturn()
        {
            Debug.Log("[ElephantMDS] Handling store return");
        }

        private static void AuthenticateAndRedirect(string ztValue, string originalUrl)
        {
            RequestAuthentication(
                token => {
                    if (ExtractWebLink(originalUrl, out string webLink))
                    {
                        var redirectUrl = $"https://{webLink}/connect/gateway?zt={ztValue}&locale=en_US&token={token}";
                        Debug.Log($"[ElephantMDS] Redirecting to: {redirectUrl}");
                        Application.OpenURL(redirectUrl);
                    }
                },
                error => OnError?.Invoke($"Authentication failed: {error}")
            );
        }

        private static bool ExtractWebLink(string bounceUrl, out string webLink)
        {
            string pattern = @"https?://link\.([a-zA-Z0-9.-]+\.\w+)/";
            var match = Regex.Match(bounceUrl, pattern);

            if (match.Success)
            {
                webLink = match.Groups[1].Value;
                return true;
            }

            webLink = "";
            return false;
        }
    }
}