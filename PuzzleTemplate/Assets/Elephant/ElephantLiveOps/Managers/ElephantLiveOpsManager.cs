using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public class ElephantLiveOpsManager : ILiveOpsElephantAdapter
    {
        private bool isOfferAssetsReady = false;
        private bool isOfferProductsReady = false;
        
        private List<Coroutine> _activeDownloads = new List<Coroutine>();

        public Offer GetCurrentOfferResponse()
        {
            return OfferAssetManager.GetInstance().currentOfferResponse;
        }
        
        public void RetrieveOfferAssetUrls()
        {
            ElephantLog.Log("LIVEOPS-ELEPHANT", "RetrieveOfferAssetUrls is Called");
            
            var isLiveOpsEnabled = RemoteConfig.GetInstance().GetBool("live_ops_tool_enabled", false);
            if(!isLiveOpsEnabled)
                return;
            
            var data = new BaseData();
            data.FillBaseData(ElephantCore.Instance.GetCurrentSession().GetSessionID());
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<OfferUiUrls>();
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.OFFERURLS_EP, bodyJson, response =>
            {
                var offerUiUrls = response.data;
                DownloadAndCacheOfferAssets(offerUiUrls);
            }, s =>
            {
                ElephantLog.Log("OFFERUI", "Could not retrieve URL's: " + s);
            });
            ElephantCore.Instance.StartCoroutine(postWithResponse);
        }
        
        private void DownloadAndCacheOfferAssets(OfferUiUrls offerUiUrls)
        {
            foreach (var url in offerUiUrls.image_urls)
            {
                DownloadTexture(url);
            }
            
            ElephantCore.Instance.StartCoroutine(WaitForAllDownloads());

            OfferAssetManager.GetInstance().purchaseOptions = offerUiUrls.purchase_options.ToList();

            if (offerUiUrls.purchase_options != null)
            {
                var productIds = new List<string>();
                foreach (var purchaseOption in offerUiUrls.purchase_options)
                {
                    switch (purchaseOption.typeEnum)
                    {
                        case PurchaseType.soft_currency:
                            break;
                        case PurchaseType.hard_currency:
                            break;
                        case PurchaseType.rewarded:
                            break;
                        case PurchaseType.iap:
                            if(purchaseOption.StoreProductId != null)
                                productIds.Add(purchaseOption.StoreProductId);
                            else
                                ElephantLog.LogError("OFFERUI", "Missing product id.");
                            break;
                        default:
                            ElephantLog.LogError("OFFERUI", "Purchase option type not defined.");
                            break;
                    }
                }
                
                var concatenatedProductIds = string.Join(";", productIds.ToArray());
                #if UNITY_EDITOR
                
                //These functions call receiveLocalizedPrice in ElephantCore
#elif UNITY_IOS
            ElephantIOS.requestLocalizedPrices(concatenatedProductIds);
#elif UNITY_ANDROID
                ElephantAndroid.requestLocalizedPrice(concatenatedProductIds);
#endif
            }

            CleanupUnusedAssets(offerUiUrls.image_urls.Concat(offerUiUrls.anim_urls).ToList());
        }
        
        private void DownloadTexture(string url)
        {
            var filename = Utils.GetFileNameFromUrl(url);
            if (Utils.IsFileExistsInSubdirectory(filename))
            {
                OfferAssetManager.GetInstance().offerUrls.Add(url);
            }
            else
            {
                Coroutine downloadCoroutine = ElephantCore.Instance.StartCoroutine(DownloadTextureCoroutine(url));
                _activeDownloads.Add(downloadCoroutine);
            }
        }
        
        private IEnumerator DownloadTextureCoroutine(string url)
        {
            var networkManager = new GenericNetworkManager<OfferUiUrls>();
            var postWithResponse = networkManager.DownloadTexture(url, texture =>
            {
                OfferAssetManager.GetInstance().offerUrls.Add(url);
                ElephantLog.Log("OFFERUI", "Downloaded: " + url);
            }, s =>
            {
                ElephantLog.Log("OFFERUI", "Could not download texture: " + s);
            });

            yield return postWithResponse;
        }
        
        public void OfferGenerateRequest(OfferMetaData offerMetaData, Action<OfferData> callback)
        {
            ElephantLog.Log("LIVEOPS-ELEPHANT", "OfferGenerateRequest is Called");

            var sessionTimeSpend = Utils.Timestamp() - ElephantCore.Instance.realSessionId;
            offerMetaData.sessionPlaytime = sessionTimeSpend;
            offerMetaData.totalPlaytime = ElephantCore.Instance.timeSpend + sessionTimeSpend;

            var offerMetaDataRequest = OfferMetaDataRequest.FillOfferMetaDataRequest(offerMetaData);
            var json = JsonConvert.SerializeObject(offerMetaDataRequest);
            var bodyJson = JsonConvert.SerializeObject(new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID()));
            var networkManager = new GenericNetworkManager<Offer>();
            var timeOut = RemoteConfig.GetInstance().GetFloat("offer_time_out", 3f);
            var postWithResponse = networkManager.PostWithResponse(ElephantConstants.OFFER_EP, bodyJson, response =>
            {
                var responseData = response.data;
                if (responseData == null)
                    return;
                var newOffer = OfferData.FromResponse(responseData);
                callback(newOffer);
                
                if (!newOffer.Show)
                    return;

                var offerAssetManager = OfferAssetManager.GetInstance();

                offerAssetManager.currentOfferResponse = responseData;
                offerAssetManager.offerUIData = newOffer.OfferUIData;
                offerAssetManager.SetTemplateFields(newOffer.TemplateFields, newOffer.OfferName);

                offerAssetManager.currentOffer = newOffer;
                offerAssetManager.offerMetaData = offerMetaData;

                AddOfferUIManager();
                ElephantCore.Instance.TriggerOnOfferUIFetched();
            }, s =>
            {
                callback(null);
            }, timeOut);

            ElephantCore.Instance.StartCoroutine(postWithResponse);
        }
        
        private IEnumerator WaitForAllDownloads()
        {
            foreach (var download in _activeDownloads)
            {
                yield return download;
            }

            isOfferAssetsReady = true;
            ElephantLog.Log("OFFEUI", "Offer assets ready.");
            Elephant.TriggerLiveOpsReady(isOfferAssetsReady, isOfferProductsReady);
        }

        private void CleanupUnusedAssets(List<string> currentUrls)
        {
            var currentFileNames = new HashSet<string>(currentUrls.Select(url => Utils.GetFileNameFromUrl(url)));
            var existingFiles = new HashSet<string>(Directory.GetFiles(Utils.GetSubdirectoryPath()).Select(Path.GetFileName));
            var filesToDelete = existingFiles.Except(currentFileNames);
            foreach (var file in filesToDelete)
            {
                File.Delete(Path.Combine(Utils.GetSubdirectoryPath(), file));
            }
        }
        
        //For testing purposes and emergencies
        private void DeleteAllAssets()
        {
            var pathToSubdirectory = Utils.GetSubdirectoryPath();

            if (Directory.Exists(pathToSubdirectory))
            {
                var files = Directory.GetFiles(pathToSubdirectory);
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                var directories = Directory.GetDirectories(pathToSubdirectory);
                foreach (var dir in directories)
                {
                    Directory.Delete(dir, true);
                }

                ElephantLog.Log("OFFERUI","All assets deleted.");
            }
            else
            {
                ElephantLog.Log("OFFERUI","Subdirectory not found.");
            }
        }
        
        private void AddOfferUIManager()
        {
            var offerUIManagerType = Type.GetType("ElephantSDK.OfferUIManager, Assembly-CSharp");
            if (offerUIManagerType != null)
            {
                var existingManager = ElephantCore.Instance.gameObject.GetComponent(offerUIManagerType);
                if (existingManager != null)
                    return;
                ElephantLog.Log("OFFERUI", "OfferUIManager added.");
                ElephantCore.Instance.gameObject.AddComponent(offerUIManagerType);
            }
            else
            {
                ElephantLog.Log("OFFERUI", "Could not add OfferUIManager.");
            }
        }
        
        public void ReceiveLocalizedPrice(string concatenatedPrices)
        {
            ElephantLog.Log("LIVEOPS-ELEPHANT", "ReceiveLocalizedPrice is Called");

            ElephantLog.Log("OFFERUI", concatenatedPrices);
            var productPriceEntries = concatenatedPrices.Split(';');
            foreach (var productPriceEntry in productPriceEntries)
            {
                if (string.IsNullOrEmpty(productPriceEntry))
                    continue;
                var parts = productPriceEntry.Split(':');
                if (parts.Length != 4)
                    continue;
        
                var productId = parts[0];
                var formattedPrice = parts[1];
                var numericPrice = parts[2];
                var currencyCode = parts[3];
                ElephantLog.Log($"OFFER PRICE for {productId}", $"{formattedPrice} {numericPrice} {currencyCode}");
                OfferAssetManager.GetInstance().localPricingCache.Add(productId, $"{formattedPrice} {numericPrice} {currencyCode}");
            }
            isOfferProductsReady = true;
            ElephantLog.Log("OFFEUI", "Offer products ready.");
            Elephant.TriggerLiveOpsReady(isOfferAssetsReady, isOfferProductsReady);
        }
    }
}