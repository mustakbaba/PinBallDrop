using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    [Serializable]
    public class PurchaseOption
    {
        public string name;
        public OfferProduct[] offer_products;
        public string type;
        public string value;

        public PurchaseType typeEnum
        {
            get
            {
                PurchaseType parsedType;
                if (Enum.TryParse(type, true, out parsedType))
                {
                    return parsedType;
                }
                else
                {
                    return PurchaseType.unknown; // Default or error handling
                }
            }
            set
            {
                type = value.ToString();
            }
        }
        
        public int? CurrencyCost
        {
            get
            {
                return (typeEnum == PurchaseType.soft_currency || typeEnum == PurchaseType.hard_currency) ? int.Parse(value) : (int?)null;
            }
        }
        
        public int? RewardedAdCount
        {
            get
            {
                return typeEnum == PurchaseType.rewarded ? int.Parse(value) : (int?)null;
            }
        }

        public string StoreProductId
        {
            get
            {
                return typeEnum == PurchaseType.iap ? value : null;
            }
        }
    }
    
    [Serializable]
    public class OfferProduct
    {
        public string name = "";
        public int amount = 0;
    }
    public enum PurchaseType
    {
        soft_currency,
        hard_currency,
        rewarded,
        iap,
        unknown
    }
}