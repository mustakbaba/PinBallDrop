using System;

namespace ElephantSDK
{
    [Serializable]
    public class Offer
    {
        public bool show;
        public int pop_up_type;
        public string offer_name;
        public string segment;
        public string status;
        public long timestamp;
        public OfferUIData template;
        public Pair[] template_fields;
    }

    public class OfferData
    {
        public bool Show;
        public int PopUpType;
        public string OfferName;
        public string Segment;
        public OfferUIData OfferUIData;
        public Pair[] TemplateFields;

        public static OfferData FromResponse(Offer offer)
        {
            return new OfferData
            {
                Show = offer.show,
                PopUpType = offer.pop_up_type,
                OfferName = offer.offer_name,
                Segment = offer.segment,
                OfferUIData = offer.template,
                TemplateFields = offer.template_fields,
            };
        }
    }

    [Serializable]
    public class Pair
    {
        public string key;
        public string value;
    }
}