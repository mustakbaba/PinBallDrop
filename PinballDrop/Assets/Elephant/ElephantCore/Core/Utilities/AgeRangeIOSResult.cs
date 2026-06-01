using System.Collections.Generic;
using System;

namespace ElephantSDK
{
    [Serializable]
    public class AgeRangeIOSResult
    {
        public string response; // Sharing, declinedSharing
        public AgeRangeData range;
    }

    [Serializable]
    public class AgeRangeData
    {
        public int? lowerBound;
        public int? upperBound;
        public string ageRangeDeclaration;
        public List<string> activeParentalControls;
        public int parentalControlsRawValue;
    }
}
