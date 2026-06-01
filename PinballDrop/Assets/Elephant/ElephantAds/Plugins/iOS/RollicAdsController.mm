#import "RollicAdsController.h"
#import <StoreKit/StoreKit.h>
#import "ElephantBuildConfig.h"

#if !EXCLUDE_EXTRA_NETWORKS
#import <FBAudienceNetwork/FBAdSettings.h>
#endif


@implementation RollicAdsController

void updateConversionValue(int value) {
    if (@available(iOS 14.0, *)) {
        [SKAdNetwork updateConversionValue:value];
    }
}

void setTrackingEnabled(bool isEnabled) {
#if !EXCLUDE_EXTRA_NETWORKS
    [FBAdSettings setAdvertiserTrackingEnabled:isEnabled];
#endif
}

float getPixelValue(float point) {
    return point * UIScreen.mainScreen.scale;
}

@end

