using System;

namespace ElephantSDK
{
    public partial class Ilrd
    {
        public string networkName;
        public string adUnitId;
        public string placement;
        public string networkPlacement;
        public string creativeId;
        public string adFormat;
        public int cycleId;
        public bool isReadyToShow;
        public string errorMessage;
        public string adLoadFailInfo;

        public static Ilrd CreateIlrd(MaxSdk.AdInfo adInfo, string adFormat, int adCycleId = -1)
        {
            try
            {
                var ilrd = new Ilrd
                {
                    revenue = adInfo.Revenue,
                    networkName = adInfo.NetworkName,
                    adUnitId = adInfo.AdUnitIdentifier,
                    placement = adInfo.Placement,
                    networkPlacement = adInfo.NetworkPlacement,
                    creativeId = adInfo.CreativeIdentifier,
                    adFormat = adFormat,
                    cycleId = adCycleId
                };

                return ilrd;
            }
            catch (Exception e)
            {
                return new Ilrd();
            }
        }

        public static Ilrd CreateIlrd(string adUnitId, string adFormat, int adCycleId = -1)
        {
            try
            {
                var ilrd = new Ilrd
                {
                    adUnitId = adUnitId,
                    adFormat = adFormat,
                    cycleId = adCycleId
                };

                return ilrd;
            }
            catch (Exception e)
            {
                return new Ilrd();
            }
        }

        public static Ilrd CreateError(MaxSdkBase.ErrorInfo error, string adUnitId, string adFormat, int adCycleId = -1)
        {
            try
            {
                var ilrd = new Ilrd
                {
                    adUnitId = adUnitId,
                    adFormat = adFormat,
                    cycleId = adCycleId,
                    errorMessage = error.Message,
                    adLoadFailInfo = error.AdLoadFailureInfo
                };

                return ilrd;
            }
            catch (Exception e)
            {
                return new Ilrd();
            }
        }
    }
}