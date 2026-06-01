package com.rollic.elephantsdk;

import static com.google.firebase.messaging.Constants.MessageNotificationKeys.TAG;

import android.Manifest;
import android.app.Activity;
import android.app.ActivityManager;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.graphics.Typeface;
import android.net.Uri;
import android.os.BatteryManager;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.os.RemoteException;
import android.text.Html;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.TextUtils;
import android.text.method.LinkMovementMethod;
import android.text.style.StyleSpan;
import android.util.Log;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.android.billingclient.api.AcknowledgePurchaseParams;
import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
import com.android.installreferrer.api.InstallReferrerClient;
import com.android.installreferrer.api.InstallReferrerStateListener;
import com.android.installreferrer.api.ReferrerDetails;
import com.android.volley.AuthFailureError;
import com.android.volley.NoConnectionError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.applovin.sdk.AppLovinPrivacySettings;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.messaging.FirebaseMessaging;
import com.rollic.elephantsdk.Models.ActionType;
import com.rollic.elephantsdk.Models.DialogModels.BlockedDialogModel;
import com.rollic.elephantsdk.Models.DialogModels.GenericDialogModel;
import com.rollic.elephantsdk.Models.DialogModels.PersonalizedAdsDialogModel;
import com.rollic.elephantsdk.Models.DialogModels.SettingsDialogModel;
import com.rollic.elephantsdk.Models.DialogSubviewType;
import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import com.google.android.gms.common.GooglePlayServicesNotAvailableException;
import com.google.android.gms.common.GooglePlayServicesRepairableException;
import com.rollic.elephantsdk.Hyperlink.Hyperlink;
import com.rollic.elephantsdk.Interaction.InteractionInterface;
import com.rollic.elephantsdk.Interaction.InteractionType;
import com.rollic.elephantsdk.Models.ComplianceActions;
import com.rollic.elephantsdk.Utils.Constants;
import com.rollic.elephantsdk.Views.BlockedDialog;
import com.rollic.elephantsdk.Views.GenericDialog;
import com.rollic.elephantsdk.Views.PersonalizedAdsConsentView;
import com.rollic.elephantsdk.Views.SettingsView;
import com.unity3d.player.R;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.HashMap;

import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.SkuDetails;
import com.android.billingclient.api.SkuDetailsParams;
import com.android.billingclient.api.PurchasesUpdatedListener;

import com.rollic.elephantsdk.Views.ReturningUserConsentView;
import com.rollic.elephantsdk.Models.DialogModels.ReturningUserDialogModel;

public class ElephantController implements InteractionInterface, PurchasesUpdatedListener {

    private static final String LOG_TAG = "[ELEPHANT SDK]";
    private RequestQueue queue;

    private Context ctx;

    InstallReferrerClient referrerClient;

    private String previousCombinedIds;

    private BillingClient billingClient;
    private static boolean isBillingSetup = false;

    //private PlayAgeSignalsController playAgeSignalsController;
    
	private static final boolean UNITY_ADS_AVAILABLE = isClassPresent("com.rollic.elephantsdk.Consent.UnityAdsConsentHelper");
	private static final boolean CHARTBOOST_AVAILABLE = isClassPresent("com.rollic.elephantsdk.Consent.ChartboostConsentHelper");
	private static final boolean IRONSOURCE_AVAILABLE = isClassPresent("com.rollic.elephantsdk.Consent.IronSourceConsentHelper");
	private static final boolean PANGLE_AVAILABLE = isClassPresent("com.rollic.elephantsdk.Consent.PangleConsentHelper");
    
    private static boolean isClassPresent(String className) {
        try {
            Class.forName(className);
            return true;
        } catch (ClassNotFoundException e) {
            return false;
        }
    }

    private ElephantController(Context ctx) {
        this.ctx = ctx;
        this.queue = Volley.newRequestQueue(this.ctx);
        //this.playAgeSignalsController = new PlayAgeSignalsController(ctx);

        referrerClient = InstallReferrerClient.newBuilder(ctx).build();
        referrerClient.startConnection(new InstallReferrerStateListener() {
            @Override
            public void onInstallReferrerSetupFinished(int responseCode) {
                switch (responseCode) {
                    case InstallReferrerClient.InstallReferrerResponse.OK:
                        forwardReferralData();
                        break;
                    case InstallReferrerClient.InstallReferrerResponse.FEATURE_NOT_SUPPORTED:
                    case InstallReferrerClient.InstallReferrerResponse.SERVICE_UNAVAILABLE:
                        break;
                }
            }

            @Override
            public void onInstallReferrerServiceDisconnected() {
            }
        });
        initializeBillingClient();
    }

    private void initializeBillingClient() {
        billingClient = BillingClient.newBuilder(ctx)
                .setListener(this)
                .enablePendingPurchases()
                .build();

        billingClient.startConnection(new BillingClientStateListener() {
            @Override
            public void onBillingSetupFinished(BillingResult billingResult) {
                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
                    Log.e(LOG_TAG, "Billing setup OK.");
                    isBillingSetup = true; // Set the flag to true when billing is set up
                } else {
                    Log.e(LOG_TAG, "Billing setup failed.");
                }
            }

            @Override
            public void onBillingServiceDisconnected() {
                Log.e(LOG_TAG, "Billing service disconnected.");
                isBillingSetup = false; // Set the flag to false if disconnected
            }
        });
    }

    @Override
    public void onPurchasesUpdated(BillingResult billingResult, List<Purchase> purchases) {
        if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK && purchases != null) {
            for (Purchase purchase : purchases) {
                // Handle the purchased items here
                // For example, grant the items to the user, and then confirm the purchase
                handlePurchase(purchase);
            }
        } else if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.USER_CANCELED) {
            // Handle the error caused by user cancelling the purchase
        } else {
            // Handle any other errors here
        }
    }

    private void handlePurchase(Purchase purchase) {
        // Here, handle the purchased item (e.g., grant premium features, coins, etc. to the user)

        // Once done, if the purchase is a consumable item, you can confirm it like this:
        if (purchase.getPurchaseState() == Purchase.PurchaseState.PURCHASED) {
            if (!purchase.isAcknowledged()) {
                AcknowledgePurchaseParams acknowledgePurchaseParams =
                        AcknowledgePurchaseParams.newBuilder()
                                .setPurchaseToken(purchase.getPurchaseToken())
                                .build();
                billingClient.acknowledgePurchase(acknowledgePurchaseParams, new AcknowledgePurchaseResponseListener() {
                    @Override
                    public void onAcknowledgePurchaseResponse(BillingResult billingResult) {
                        // Handle the acknowledge response here
                    }
                });
            }
        }
    }


    public void requestLocalizedPrices(String concatenatedProductIds) {
        if (!isBillingSetup) {
            Log.e(LOG_TAG, "Billing client not ready.");
            return;
        }
        String[] productIds = concatenatedProductIds.split(";");
        querySkuDetails(productIds);
    }


    private void querySkuDetails(String[] productIds) {
        List<String> skuList = Arrays.asList(productIds);
        SkuDetailsParams.Builder params = SkuDetailsParams.newBuilder();
        params.setSkusList(skuList).setType(BillingClient.SkuType.INAPP);
        billingClient.querySkuDetailsAsync(params.build(), (billingResult, skuDetailsList) -> {
            if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK && skuDetailsList != null) {
                StringBuilder concatenatedPrices = new StringBuilder();
                for (SkuDetails skuDetails : skuDetailsList) {
                    String numericPrice = getCurrencyAmount(skuDetails.getPrice(), skuDetails.getPriceCurrencyCode());
                    concatenatedPrices.append(skuDetails.getSku())
                            .append(":")
                            .append(skuDetails.getPrice())
                            .append(":")
                            .append(numericPrice)
                            .append(":")
                            .append(skuDetails.getPriceCurrencyCode())
                            .append(";");
                }
                UnityPlayer.UnitySendMessage("Elephant", "ReceiveLocalizedPrice", concatenatedPrices.toString());
            }
            else {
            UnityPlayer.UnitySendMessage("Elephant", "ReceiveLocalizedPriceError", "No product found");
        }
    });
}

    private String getCurrencyAmount(String price, String currencyCode) {
        return price.replaceAll("[^\\d.,]", "");
    }

    private void forwardReferralData() {
        ReferrerDetails response = null;
        try {
            response = referrerClient.getInstallReferrer();

        } catch (RemoteException e) {
            e.printStackTrace();
        }

        if (response == null) return;

        String referrerUrl = response.getInstallReferrer();
        long referrerClickTime = response.getReferrerClickTimestampSeconds();
        long appInstallTime = response.getInstallBeginTimestampSeconds();
        boolean instantExperienceLaunched = response.getGooglePlayInstantParam();

        JSONObject referralData = new JSONObject();

        try {
            referralData.accumulate("referrerUrl", referrerUrl);
            referralData.accumulate("appInstallTime", appInstallTime);
            referralData.accumulate("referrerClickTime", referrerClickTime);
            referralData.accumulate("instantExperienceLaunched", instantExperienceLaunched);
            referralData.accumulate("installVersion", response.getInstallVersion());
            com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "ReferralData", referralData.toString());
            referrerClient.endConnection();

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    public static ElephantController create(Context ctx) {
        return new ElephantController(ctx);
    }


    public void ElephantPost(final String url, final String body, final String gameID, final String authToken, int _tryCount) {

        try {
            final int tryCount = _tryCount + 1;
            StringRequest stringRequest = new StringRequest(Request.Method.POST, url,
                    new Response.Listener<String>() {
                        @Override
                        public void onResponse(String response) {
                            // Display the first 500 characters of the response string.
                            Log.e(LOG_TAG, "onResponse: " + response);
                        }
                    }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {

                    try {
                        boolean isOffline = false;
                        int statusCode = -1;

                        if (error instanceof NoConnectionError) {
                            isOffline = true;
                        }

                        if (error.networkResponse != null) {
                            statusCode = error.networkResponse.statusCode;
                        }

                        JSONObject failedReq = new JSONObject();
                        failedReq.accumulate("url", url);
                        failedReq.accumulate("isOffline", isOffline);
                        failedReq.accumulate("statusCode", statusCode);
                        failedReq.accumulate("data", body);
                        failedReq.accumulate("tryCount", tryCount);
                        com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "FailedRequest", failedReq.toString());
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }

                    Log.e(LOG_TAG, "error: " + error.networkResponse);
                }
            }) {

                @Override
                public Map<String, String> getHeaders() throws AuthFailureError {
                    Map<String, String> headers = new HashMap<>();
                    headers.put("Content-Type", "application/json; charset=utf-8");
                    headers.put("Authorization", authToken);
                    headers.put("GameID", gameID);
                    return headers;
                }

                @Override
                public byte[] getBody() throws AuthFailureError {
                    return body.getBytes(StandardCharsets.UTF_8);
                }
            };


            queue.add(stringRequest);

        } catch (Exception e) {
            e.printStackTrace();
        }

    }

    public void showAlertDialog(String title, String message) {
        if (message.contains("{{tos}}")) {
            message = message.replace("{{tos}}", "<a href=\"" + title + "\">Terms of Service</a>");

            AlertDialog alertDialog = new AlertDialog.Builder(ctx)
                    .setTitle(title)
                    .setMessage(Html.fromHtml(message))
                    .setCancelable(true)
                    .setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int which) {
                            dialog.dismiss();
                        }
                    }).show();

            ((TextView) alertDialog.findViewById(android.R.id.message)).setMovementMethod(LinkMovementMethod.getInstance());
        } else {
            new AlertDialog.Builder(ctx)
                    .setTitle(title)
                    .setMessage(message)
                    .setCancelable(true)
                    .setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int which) {
                            dialog.dismiss();
                        }
                    }).show();
        }
    }

    public void showForceUpdate(String title, String message) {
        new AlertDialog.Builder(ctx)
                .setTitle(title)
                .setMessage(message)
                .setCancelable(false)
                .setPositiveButton(android.R.string.yes, new DialogInterface.OnClickListener() {
                    public void onClick(DialogInterface dialog, int which) {
                        final String appPackageName = ctx.getPackageName();
                        try {
                            ctx.startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("market://details?id=" + appPackageName)));
                        } catch (android.content.ActivityNotFoundException anfe) {
                            ctx.startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("https://play.google.com/store/apps/details?id=" + appPackageName)));
                        }

                    }
                }).show();

    }

    private void showConsent(String subviewType, String content, String buttonTitle,
                             String privacyPolicyText, String privacyPolicyUrl,
                             String tosText, String TosUrl,
                             String dataRequestText, String dataRequestUrl) {
        Hyperlink[] hyperlinks = {
                new Hyperlink(Constants.PRIVACY_POLICY_MASK, privacyPolicyText, privacyPolicyUrl),
                new Hyperlink(Constants.TERMS_OF_SERVICE_MASK, tosText, TosUrl),
                new Hyperlink(Constants.PERSONAL_DATA_REQUEST_MASK, dataRequestText, dataRequestUrl)
        };
        GenericDialogModel model = new GenericDialogModel(this, "", content, buttonTitle, hyperlinks);
        GenericDialog dialog = GenericDialog.newInstance(ctx);

        dialog.configureWithModel(model);

        dialog.configureButtonActionHandler(new GenericDialog.ButtonActionHandler() {
            @Override
            public void onButtonClickHandler() {
                if (dataRequestText.isEmpty()) {
                    OnButtonClick(InteractionType.TOS_ACCEPT);
                }
            }
        });

        dialog.show(DialogSubviewType.valueOf(subviewType));
    }

    public void showCcpaDialog(String action, String title, String content,
                               String privacyPolicyText, String privacyPolicyUrl,
                               String declineActionButtonText, String agreeActionButtonText,
                               String backToGameActionButtonText) {
        ActionType actionEnum = ActionType.valueOf(action);
        Hyperlink hyperlinks[] = {new Hyperlink(Constants.PRIVACY_POLICY_MASK, privacyPolicyText, privacyPolicyUrl)};
        PersonalizedAdsConsentView personalizedAdsConsentView =
                PersonalizedAdsConsentView.newInstance(ctx);
        PersonalizedAdsDialogModel model =
                new PersonalizedAdsDialogModel(this, actionEnum, title, content,
                        declineActionButtonText, agreeActionButtonText, backToGameActionButtonText, hyperlinks);
        personalizedAdsConsentView.configureWithModel(model);

        personalizedAdsConsentView.show(DialogSubviewType.CONTENT);
    }

    public void showSettingsView(String subviewType, String actions, boolean showCMPButton, String elephantId) {
        SettingsView settingsView = SettingsView.newInstance(ctx);

        try {
            JSONObject jsonObject = new JSONObject(actions);
            ComplianceActions complianceActions = new ComplianceActions(jsonObject);
            SettingsDialogModel model = new SettingsDialogModel(this, complianceActions.actions, showCMPButton, elephantId);

            settingsView.configureWithModel(model);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        settingsView.show(DialogSubviewType.valueOf(subviewType));
    }

    public void showBlockedDialog(String title, String content, String warningContent, String buttonTitle) {
        BlockedDialog blockedDialog = BlockedDialog.newInstance(ctx);
        BlockedDialogModel model = new BlockedDialogModel(this,
                title, content, warningContent, buttonTitle, new Hyperlink[]{});

        blockedDialog.configureWithModel(model);

        blockedDialog.show(DialogSubviewType.CONTENT);
    }

    public void showNetworkOfflineDialog(String content, String buttonTitle) {
        GenericDialog dialog = GenericDialog.newInstance(ctx);
        GenericDialogModel model = new GenericDialogModel(this, content, buttonTitle);

        dialog.configureWithModel(model);

        dialog.configureButtonActionHandler(new GenericDialog.ButtonActionHandler() {
            @Override
            public void onButtonClickHandler() {
                OnButtonClick(InteractionType.RETRY_CONNECTION);
            }
        });

        dialog.show(DialogSubviewType.CONTENT);
    }
    
    public void showVppaDialog(String content, String buttonTitle) {
        GenericDialog dialog = GenericDialog.newInstance(ctx);
        GenericDialogModel model = new GenericDialogModel(this, content, buttonTitle);
    
        dialog.configureWithModel(model);
    
        dialog.configureButtonActionHandler(new GenericDialog.ButtonActionHandler() {
            @Override
            public void onButtonClickHandler() {
                OnButtonClick(InteractionType.VPPA_ACCEPT);
            }
        });
    
        dialog.show(DialogSubviewType.CONTENT);
    }
    
	public void showCollectibleDialog(String content, String buttonTitle) {
		GenericDialog dialog = GenericDialog.newInstance(ctx);
		GenericDialogModel model = new GenericDialogModel(this, content, buttonTitle);

		dialog.configureWithModel(model);

		dialog.configureButtonActionHandler(new GenericDialog.ButtonActionHandler() {
			@Override
			public void onButtonClickHandler() {
				OnButtonClick(InteractionType.COLLECTIBLE_ACCEPT);
			}
		});

		dialog.show(DialogSubviewType.CONTENT);
	}

    public String getBuildNumber() {
        if (getBuildConfigValue() == null) {
            return "";
        }

        return getBuildConfigValue() + "";
    }

    private Object getBuildConfigValue() {
        try {
            Class<?> clazz = Class.forName(ctx.getPackageName() + ".BuildConfig");
            Field field = clazz.getField("VERSION_CODE");
            return field.get(null);
        } catch (ClassNotFoundException e) {
            e.printStackTrace();
        } catch (NoSuchFieldException e) {
            e.printStackTrace();
        } catch (IllegalAccessException e) {
            e.printStackTrace();
        }
        return null;
    }

    public String getLocale() {
        String locale;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.LOLLIPOP) {
            locale = Locale.getDefault().toLanguageTag();
        } else {
            locale = Locale.getDefault().toString();
        }

        return locale;
    }

    public String FetchAdId() {
        String adId = "";

        try {
            AdvertisingIdClient.Info adIdInfo = AdvertisingIdClient.getAdvertisingIdInfo(ctx);
            if (adIdInfo == null) {
                return adId;
            }

            adId = adIdInfo.getId() != null ? adIdInfo.getId() : "";

        } catch (IOException e) {
            e.printStackTrace();
        } catch (GooglePlayServicesNotAvailableException e) {
            e.printStackTrace();
        } catch (GooglePlayServicesRepairableException e) {
            e.printStackTrace();
        }

        return adId;
    }

    public int gameMemoryUsage() {
        try {
            ActivityManager mgr = (ActivityManager) ctx.getSystemService(Context.ACTIVITY_SERVICE);
            List<ActivityManager.RunningAppProcessInfo> processes = mgr.getRunningAppProcesses();
            double memoryUsage = 0;

            if (processes.size() == 0) return 0;

            for (ActivityManager.RunningAppProcessInfo p : processes) {
                int[] pids = new int[1];
                pids[0] = p.pid;
                android.os.Debug.MemoryInfo[] MI = mgr.getProcessMemoryInfo(pids);
                if (MI[0] == null || MI[0].getTotalPss() <= 0) continue;
                memoryUsage = MI[0].getTotalPss() / 1000.0;
            }
            if (memoryUsage <= 0) return 0;
            return (int) memoryUsage;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return -1;
    }

    public int gameMemoryUsagePercentage() {
        try {
            double memoryUsage = gameMemoryUsage();
            if (memoryUsage <= 0) return 0;

            ActivityManager.MemoryInfo mi = new ActivityManager.MemoryInfo();
            ActivityManager activityManager = (ActivityManager) ctx.getSystemService(Context.ACTIVITY_SERVICE);
            activityManager.getMemoryInfo(mi);
            if (mi.totalMem <= 0) return 0;
            double totalMemory = (double) mi.totalMem / 0x100000L;
            return ((int) memoryUsage * 100) / (int) totalMemory;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return -1;
    }

    public float getBatteryLevel() {
		IntentFilter intentFilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
		Intent batteryStatus = ContextCompat.registerReceiver(ctx, null, intentFilter, ContextCompat.RECEIVER_NOT_EXPORTED);

		if (batteryStatus != null) {
			
			int level = batteryStatus.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
			int scale = batteryStatus.getIntExtra(BatteryManager.EXTRA_SCALE, -1);
			
			return level / (float) scale;
		} else {
			return -1;
		}
	}
  
    public long getFirstInstallTime() {
        try {
            return ctx.getPackageManager().getPackageInfo(ctx.getPackageName(), 0).firstInstallTime;
        } catch (PackageManager.NameNotFoundException e) {
            return 0;
        }
    }

    public String test() {
        Log.e(LOG_TAG, "test called");

        return "Hello from Elephant android plugin ";
    }

    @Override
    public void OnButtonClick(InteractionType interactionType) {
        // TO DO: Handle popup button interactions with InteractionType.

        switch (interactionType) {
            case TOS_ACCEPT:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "TOS_ACCEPT");
                break;
            case GDPR_AD_CONSENT_AGREE:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "GDPR_AD_CONSENT_AGREE");
                break;
            case GDPR_AD_CONSENT_DECLINE:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "GDPR_AD_CONSENT_DECLINE");
                break;
            case PERSONALIZED_ADS_AGREE:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "PERSONALIZED_ADS_AGREE");
                break;
            case PERSONALIZED_ADS_DECLINE:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "PERSONALIZED_ADS_DECLINE");
                break;
            case CALL_DATA_REQUEST:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "CALL_DATA_REQUEST");
                break;
            case DELETE_REQUEST_CANCEL:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "DELETE_REQUEST_CANCEL");
                break;
			case VPPA_ACCEPT:
				com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "VPPA_ACCEPT");
				break;
            case RETRY_CONNECTION:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "UserConsentAction", "RETRY_CONNECTION");
            case RETURNING_USER_INFORMED:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "receiveReturningPopUpResponse", "OK");
            case COLLECTIBLE_ACCEPT:
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "ReceiveCollectibleResponse", "OK");
        }
    }

    public void getNotificationPermission() {
        try {
            if (Build.VERSION.SDK_INT > 32) {
                ActivityCompat.requestPermissions((Activity) ctx,
                        new String[]{Manifest.permission.POST_NOTIFICATIONS},
                        112);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void getToken() {
        try {
            FirebaseMessaging.getInstance().getToken()
                    .addOnCompleteListener(new OnCompleteListener<String>() {
                        @Override
                        public void onComplete(@NonNull Task<String> task) {
                            if (!task.isSuccessful()) {
                                Log.w(TAG, "Fetching FCM registration token failed", task.getException());
                                return;
                            }

                            // Get new FCM registration token
                            String token = task.getResult();
                            com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "SetDeviceToken", token);

                            // Log
                            Log.d(TAG, token);
                        }
                    });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void askForIntent() {
        try {
            Activity activity = (Activity) ctx;
            Intent intent = activity.getIntent();

            if (intent != null && intent.hasExtra("notification_id") && intent.hasExtra("message_id")
                    && intent.hasExtra("job_id") && intent.hasExtra("scheduled_at")) {
                String notificationId = intent.getStringExtra("notification_id");
                String messageId = intent.getStringExtra("message_id");
                String jobId = intent.getStringExtra("job_id");
                String scheduledAt = intent.getStringExtra("scheduled_at");

                if (!TextUtils.isEmpty(notificationId) && !TextUtils.isEmpty(messageId)) {
                    String combinedIds = notificationId + ";" + messageId + ";" + jobId + ";" + scheduledAt;

                    if (!combinedIds.equals(previousCombinedIds)) {
                        previousCombinedIds = combinedIds;
                        com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "SendPushNotificationOpenEvent", combinedIds);
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
    
       public void showReturningUserDialog(String action, String title, String content,
                                                String privacyPolicyText, String privacyPolicyUrl,
                                                String backToGameButtonText)
            {
    
                ((Activity) ctx).runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        ActionType actionEnum = ActionType.valueOf(action);
                        Hyperlink hyperlinks[] = {new Hyperlink(Constants.PRIVACY_POLICY_MASK, privacyPolicyText, privacyPolicyUrl)};
                        ReturningUserConsentView returningUserConsentView =
                                ReturningUserConsentView.newInstance(ctx);
                        ReturningUserDialogModel model =
                                new ReturningUserDialogModel(ElephantController.this, actionEnum, title, content, backToGameButtonText, hyperlinks);
                        returningUserConsentView.configureWithModel(model);
    
                        returningUserConsentView.show(DialogSubviewType.CONTENT);
                    }
                });
    
            }
    
    // https://dash.applovin.com/documentation/mediation/android/getting-started/privacy
    public void setAppLovinGdprConsent(String consentString) {
        try {
            boolean consent = Boolean.parseBoolean(consentString);
            AppLovinPrivacySettings.setHasUserConsent(consent, ctx);
        } catch (Exception e) {
            Log.w(LOG_TAG, "Failed to update AppLovin GDPR consent: " + e.getMessage());
        }
    }

    // https://docs.unity.com/ads/en-us/manual/GDPRCompliance#Android_(Java)
    public void setUnityAdsGdprConsent(String consentString) {
        if (UNITY_ADS_AVAILABLE) {
            try {
                Class<?> helper = Class.forName("com.rollic.elephantsdk.Consent.UnityAdsConsentHelper");
                helper.getMethod("setConsent", Context.class, String.class).invoke(null, ctx, consentString);
            } catch (Exception e) {
                Log.w(LOG_TAG, "Failed to update UnityAds GDPR consent: " + e.getMessage());
            }
        }
    }

    // https://docs.chartboost.com/en/monetization/integrate/android/sdk-privacy-methods/
    public void setChartboostGdprConsent(String consentString) {
        if (CHARTBOOST_AVAILABLE) {
            try {
                Class<?> helper = Class.forName("com.rollic.elephantsdk.Consent.ChartboostConsentHelper");
                helper.getMethod("setConsent", Context.class, String.class).invoke(null, ctx, consentString);
            } catch (Exception e) {
                Log.w(LOG_TAG, "Failed to update Chartboost GDPR consent: " + e.getMessage());
            }
        }
    }

    // https://developers.is.com/ironsource-mobile/android/regulation-advanced-settings/#step-1
    public void setIronSourceGdprConsent(String consentString) {
        if (IRONSOURCE_AVAILABLE) {
            try {
                Class<?> helper = Class.forName("com.rollic.elephantsdk.Consent.IronSourceConsentHelper");
                helper.getMethod("setConsent", String.class).invoke(null, consentString);
            } catch (Exception e) {
                Log.w(LOG_TAG, "Failed to update IronSource GDPR consent: " + e.getMessage());
            }
        }
    }
    
    // https://www.pangleglobal.com/integration/android-initialize-pangle-sdk
    public void setPangleGdprConsent(String consentString) {
        if (PANGLE_AVAILABLE) {
            try {
                Class<?> helper = Class.forName("com.rollic.elephantsdk.Consent.PangleConsentHelper");
                helper.getMethod("setConsent", String.class).invoke(null, consentString);
            } catch (Exception e) {
                Log.w(LOG_TAG, "Failed to update Pangle GDPR consent: " + e.getMessage());
            }
        }
    }
    
    /**
     * Opens a given URL in a WebView by launching the WebViewController activity.
     * @param urlString The URL to be opened in the WebView
     */
    public void openURLInWebView(String urlString) {
        if (urlString == null || urlString.isEmpty()) {
            Log.e(LOG_TAG, "[openURLInWebView] URL is null or empty");
            return;
        }

        if (!(ctx instanceof Activity)) {
            Log.e(LOG_TAG, "[openURLInWebView] Context is null or not an Activity");
            return;
        }

        Activity activity = (Activity) ctx;

        activity.runOnUiThread(() -> {
            Log.d(LOG_TAG, "[openURLInWebView] Launching WebViewController with URL: " + urlString);
            Intent intent = new Intent(activity, WebViewController.class);
            intent.putExtra(WebViewController.EXTRA_URL, urlString);
            activity.startActivity(intent);
        });
    }
    
    public void openURLInChromeCustomTab(String urlString) {
        if (urlString == null || urlString.isEmpty()) {
            Log.e(LOG_TAG, "[openURLInChromeCustomTab] URL is null or empty");
            return;
        }
    
        if (!(ctx instanceof Activity)) {
            Log.e(LOG_TAG, "[openURLInChromeCustomTab] Context is not an Activity");
            return;
        }
    
        Activity activity = (Activity) ctx;
    
        activity.runOnUiThread(() -> {
            try {
                androidx.browser.customtabs.CustomTabsIntent.Builder builder = 
                    new androidx.browser.customtabs.CustomTabsIntent.Builder();
                
                androidx.browser.customtabs.CustomTabsIntent customTabsIntent = builder.build();
                customTabsIntent.launchUrl(activity, android.net.Uri.parse(urlString));
                
            } catch (Exception e) {
                try {
                    Intent browserIntent = new Intent(Intent.ACTION_VIEW, android.net.Uri.parse(urlString));
                    browserIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                    browserIntent.setPackage("com.android.chrome");
                    activity.startActivity(browserIntent);
                } catch (android.content.ActivityNotFoundException ex) {
                    Intent browserIntent = new Intent(Intent.ACTION_VIEW, android.net.Uri.parse(urlString));
                    browserIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                    activity.startActivity(Intent.createChooser(browserIntent, "Open with"));
                }
            }
        });
    }
    
    /* public boolean isPlayAgeSignalsAvailable() {
        if (playAgeSignalsController == null) {
            return false;
        }
        return playAgeSignalsController.isAvailable();
    }
    
    public void requestPlayAgeSignals() {
        if (playAgeSignalsController == null) {
            Log.e(LOG_TAG, "PlayAgeSignalsController is not initialized");
            return;
        }
        playAgeSignalsController.requestAgeSignals();
    } */
}