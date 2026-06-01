#import <Foundation/Foundation.h>
#import "PopUpView.h"
#import "SettingsView.h"
#import "Colors.h"
#import "Constants.h"
#import "PersonalizedAdsConsentPopUpView.h"
#import "BlockedPopUpView.h"
#import "ComplianceActions.h"
#import "Utils.h"
#import "InteractableHandler.h"
#import "ReturningUserPopUpView.h"
#import "ElephantBuildConfig.h"

NS_ASSUME_NONNULL_BEGIN

@interface ElephantController : NSObject
@end

#ifdef __cplusplus
extern "C" {
#endif

InteractableHandler* interactableHandler = [InteractableHandler new];
const char* IDFA();
const char* getConsentStatus();
const char* getBuildNumber();
const char* getLocale();
void TestFunction(const char * string);
void showForceUpdate(const char * title, const char * message);
void showAlertDialog(const char * title, const char * message);
void showIdfaConsent(int type, int delay, int position, const char * titleText,
                     const char * descriptionText,
                     const char * buttonText,
                     const char * termsText,
                     const char * policyText,
                     const char * termsUrl,
                     const char * policyUrl);
void ElephantPost(const char * url, const char * body, const char * gameID, const char * authToken, int tryCount);
void showPopUpView(const char * popUpSubviewType, const char * text, const char * buttonTitle, 
                   const char * privacyPolicyText, const char * privacyPolicyUrl,
                   const char * termsOfServiceText, const char * termsOfServiceUrl,
                   const char * dataRequestText, const char * dataRequestUrl);
void showCcpaPopUpView(const char * action, const char * title, const char * content, 
                       const char * privacyPolicyText, const char * privacyPolicyUrl, 
                       const char * declineActionButtonText, const char * agreeActionButtonText,
                       const char * backToGameActionButtonText);
void showSettingsView(const char * popUpSubviewType, const char * actions, bool showCMPButton, const char * elephantId);
void showBlockedPopUpView(const char * title, const char * content, const char * warningContent, const char * buttonTitle);
void showNetworkOfflinePopUpView(const char * content, const char * buttonTitle);
void showVppaDialog(const char * content, const char * buttonTitle);
const char * ElephantCopyString(const char * string)
{
   char * copy = (char*)malloc(strlen(string) + 1);
   strcpy(copy, string);
   return copy;
}

int gameMemoryUsagePercent();
int gameMemoryUsage();
float getBatteryLevel();
const long long getFirstInstallTime();
void getNotificationPermission();
void requestLocalizedPrices(const char *concatenatedProductIds);
void setAppLovinGdprConsent(bool consent);
#if !EXCLUDE_EXTRA_NETWORKS
void setUnityAdsGdprConsent(bool consent);
void setChartboostGdprConsent(bool consent);
void setIronSourceGdprConsent(bool consent);
void setPangleGdprConsent(bool consent);
#endif
void showReturningUserPopUpView(const char * action,
    const char * title, const char * content, const char * privacyPolicyText, 
    const char * privacyPolicyUrl,const char * backToGameActionButtonText);
void showCollectiblePopUpView(const char * message, const char * buttonText);
void openURL(const char * urlString);
void openURLInWebView(const char * urlString);

    
#ifdef __cplusplus
} // extern "C"
#endif


NS_ASSUME_NONNULL_END
