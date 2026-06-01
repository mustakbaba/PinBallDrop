using System;

namespace ElephantSDK
{
    [Serializable]
    public class AgeRangeAndroidResult
    {
        public string userStatus;  // VERIFIED, DECLARED, SUPERVISED, SUPERVISED_APPROVAL_PENDING, SUPERVISED_APPROVAL_DENIED, UNKNOWN
        public int? ageLower;      // 0-18 (inclusive lower bound)
        public int? ageUpper;       // 2-18 (inclusive upper bound)
        public string installId;    // Play-generated ID for supervised users
        public long? mostRecentApprovalDate;  // Timestamp of most recent approved significant change
    }
}
