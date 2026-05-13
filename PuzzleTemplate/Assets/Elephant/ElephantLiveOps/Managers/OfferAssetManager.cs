using System.Collections.Generic;

namespace ElephantSDK
{
    public class OfferAssetManager
    {
        private static OfferAssetManager _instance;
        public OfferUIData offerUIData;
        public Dictionary<string, string> localPricingCache = new Dictionary<string, string>();
        public Dictionary<string, string> templateFieldsCache = new Dictionary<string, string>();
        
        public OfferData currentOffer;
        public OfferMetaData offerMetaData;
        public Offer currentOfferResponse;
        
        public List<PurchaseOption> purchaseOptions = new List<PurchaseOption>();
        public string iapNames;
        public List<string> offerUrls = new List<string>();


        public static OfferAssetManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new OfferAssetManager();
            }
            return _instance;
        }

        public void SetTemplateFields(Pair[] templateFields, string offerName)
        {
            if (templateFields == null)
                return;

            foreach (var pair in templateFields)
            {
                templateFieldsCache[offerName + "_" + pair.key] = pair.value;
            }
        }
    }
}