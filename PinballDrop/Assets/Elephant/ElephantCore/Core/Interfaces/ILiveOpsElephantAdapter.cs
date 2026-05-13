using System;
using UnityEngine;

namespace ElephantSDK
{
    public interface ILiveOpsElephantAdapter : IElephantAdapter
    {
        void RetrieveOfferAssetUrls();

        void OfferGenerateRequest(OfferMetaData offerMetaData, Action<OfferData> callback);

        void ReceiveLocalizedPrice(string concatenatedPrices);
        
        Offer GetCurrentOfferResponse();
    }
}