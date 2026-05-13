using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    [Serializable]
    public class OfferUiUrls
    {
        public List<string> image_urls;
        public List<string> anim_urls;
        public PurchaseOption[] purchase_options;
    }
}