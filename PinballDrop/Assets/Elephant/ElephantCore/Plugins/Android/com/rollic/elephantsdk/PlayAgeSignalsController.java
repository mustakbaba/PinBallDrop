/* 
package com.rollic.elephantsdk;

import android.content.Context;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.google.android.play.agesignals.AgeSignalsManager;
import com.google.android.play.agesignals.AgeSignalsManagerFactory;
import com.google.android.play.agesignals.AgeSignalsRequest;
import com.google.android.play.agesignals.AgeSignalsResult;
import com.google.android.play.agesignals.AgeSignalsException;
import com.google.android.play.agesignals.model.AgeSignalsVerificationStatus;
import com.google.android.play.agesignals.model.AgeSignalsErrorCode;

import org.json.JSONObject;

import com.unity3d.player.UnityPlayer;

import java.util.Date;

 */
/**
 * Controller class for Google Play Age Signals API.
*//* 

public class PlayAgeSignalsController {

    private static final String LOG_TAG = "[AGE RANGE]";
    private static final int MAX_RETRY_ATTEMPTS = 3;
    private static final long RETRY_DELAY_MS = 1000;

    private Context ctx;
    private AgeSignalsManager ageSignalsManager;
    private Handler mainHandler;
    private int currentRetryAttempt;
    
    public PlayAgeSignalsController(Context context) {
        this.ctx = context;
        this.mainHandler = new Handler(Looper.getMainLooper());
        //initializeAgeSignalsManager();
    }
    
    private void initializeAgeSignalsManager() {
        try {
            // Check if device supports Play Age Signals API (requires Android 6.0 / API level 23)
            if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
                Log.w(LOG_TAG, "Play Age Signals API requires Android 6.0 (API level 23) or higher. Current: " + Build.VERSION.SDK_INT);
                return;
            }

            try {
                ageSignalsManager = AgeSignalsManagerFactory.create(ctx);
                Log.d(LOG_TAG, "AgeSignalsManager created successfully");
            } catch (Exception e) {
                Log.w(LOG_TAG, "Failed to create AgeSignalsManager: " + e.getMessage());
            }
        } catch (Exception e) {
            Log.e(LOG_TAG, "Failed to initialize AgeSignalsManager: " + e.getMessage());
        }
    }
    
     */
/**
     * Check if Play Age Signals API is available.
     * @return true if Play Age Signals API is available
     *//* 

    public boolean isAvailable() {
        try {
            // Check if device supports Play Age Signals API (requires Android 6.0 / API level 23)
            if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
                Log.w(LOG_TAG, "Play Age Signals API requires Android 6.0 (API level 23) or higher. Current: " + Build.VERSION.SDK_INT);
                return false;
            }
            
            if (ageSignalsManager == null) {
                return false;
            }
            
            return true;
        } catch (Exception e) {
            Log.e(LOG_TAG, "Error checking Play Age Signals availability: " + e.getMessage());
            return false;
        }
    }
    
     */
/**
     * Request age signals from the Play Age Signals API.
     * Implements retry with exponential backoff for retryable errors (max 3 attempts).
     *//* 

    public void requestAgeSignals() {
        currentRetryAttempt = 0;
        performAgeSignalsRequest();
    }

    private void performAgeSignalsRequest() {
        if (ageSignalsManager == null) {
            Log.e(LOG_TAG, "AgeSignalsManager is not initialized");
            sendError("AgeSignalsManager is not initialized");
            return;
        }

        try {
            AgeSignalsRequest request = AgeSignalsRequest.builder().build();

            ageSignalsManager.checkAgeSignals(request)
                .addOnSuccessListener(result -> {
                    mainHandler.post(() -> {
                        try {
                            JSONObject jsonResult = new JSONObject();
                            
                            Integer userStatusInt = result.userStatus();
                            Integer ageLower = result.ageLower();
                            Integer ageUpper = result.ageUpper();
                            String installId = result.installId();
                            Date mostRecentApprovalDate = result.mostRecentApprovalDate();
                            
                            String userStatus;
                            if (userStatusInt == null) {
                                userStatus = null; // Not in applicable region or user does not share age with apps
                            } else if (userStatusInt == AgeSignalsVerificationStatus.VERIFIED) {
                                userStatus = "VERIFIED";
                            } else if (userStatusInt == AgeSignalsVerificationStatus.SUPERVISED) {
                                userStatus = "SUPERVISED";
                            } else if (userStatusInt == AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING) {
                                userStatus = "SUPERVISED_APPROVAL_PENDING";
                            } else if (userStatusInt == AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED) {
                                userStatus = "SUPERVISED_APPROVAL_DENIED";
                            } else if (userStatusInt == AgeSignalsVerificationStatus.UNKNOWN) {
                                userStatus = "UNKNOWN";
                            } else if (userStatusInt == AgeSignalsVerificationStatus.DECLARED) {
                                userStatus = "DECLARED";
                            } else {
                                userStatus = "UNKNOWN";
                            }
                            boolean isEmpty = (userStatusInt == null && ageLower == null && ageUpper == null);
                            if (isEmpty) {
                                JSONObject errorJson = new JSONObject();
                                errorJson.put("error", "AGE_SIGNALS_EMPTY: No age data (not applicable region or user did not share age)");
                                errorJson.put("errorCode", "EMPTY_OR_NOT_APPLICABLE");
                                UnityPlayer.UnitySendMessage("Elephant", "OnAgeRangeResult", errorJson.toString());
                                return;
                            }

                            jsonResult.put("userStatus", userStatus);
                            if (ageLower != null) {
                                jsonResult.put("ageLower", ageLower);
                            }
                            if (ageUpper != null) {
                                jsonResult.put("ageUpper", ageUpper);
                            }
                            if (installId != null) {
                                jsonResult.put("installId", installId);
                            }
                            if (mostRecentApprovalDate != null) {
                                jsonResult.put("mostRecentApprovalDate", mostRecentApprovalDate.getTime());
                            }

                            UnityPlayer.UnitySendMessage("Elephant", "OnAgeRangeResult", jsonResult.toString());
                        } catch (Exception e) {
                            Log.e(LOG_TAG, "Error creating JSON result: " + e.getMessage());
                            sendError("Error creating JSON result: " + e.getMessage());
                        }
                    });
                })
                .addOnFailureListener(exception -> {
                    mainHandler.post(() -> {
                        String errorMessage = "Unknown error";
                        int errorCode = -100;
                        if (exception instanceof AgeSignalsException) {
                            AgeSignalsException ageSignalsException = (AgeSignalsException) exception;
                            errorCode = ageSignalsException.getErrorCode();

                            switch (errorCode) {
                                case AgeSignalsErrorCode.API_NOT_AVAILABLE:
                                    errorMessage = "API_NOT_AVAILABLE: The Play Age Signals API is not available";
                                    break;
                                case AgeSignalsErrorCode.PLAY_STORE_NOT_FOUND:
                                    errorMessage = "PLAY_STORE_NOT_FOUND: No Play Store app found on the device";
                                    break;
                                case AgeSignalsErrorCode.NETWORK_ERROR:
                                    errorMessage = "NETWORK_ERROR: No available network found";
                                    break;
                                case AgeSignalsErrorCode.PLAY_SERVICES_NOT_FOUND:
                                    errorMessage = "PLAY_SERVICES_NOT_FOUND: Play Services is not available or version is too old";
                                    break;
                                case AgeSignalsErrorCode.CANNOT_BIND_TO_SERVICE:
                                    errorMessage = "CANNOT_BIND_TO_SERVICE: Failed to bind to Play Store service. Try updating Play Store";
                                    break;
                                case AgeSignalsErrorCode.PLAY_STORE_VERSION_OUTDATED:
                                    errorMessage = "PLAY_STORE_VERSION_OUTDATED: The Play Store app needs to be updated";
                                    break;
                                case AgeSignalsErrorCode.PLAY_SERVICES_VERSION_OUTDATED:
                                    errorMessage = "PLAY_SERVICES_VERSION_OUTDATED: Play Services needs to be updated";
                                    break;
                                case AgeSignalsErrorCode.CLIENT_TRANSIENT_ERROR:
                                    errorMessage = "CLIENT_TRANSIENT_ERROR: Transient error in client device. Please try again";
                                    break;
                                case AgeSignalsErrorCode.APP_NOT_OWNED:
                                    errorMessage = "APP_NOT_OWNED: The app was not installed by Google Play";
                                    break;
                                case AgeSignalsErrorCode.SDK_VERSION_OUTDATED:
                                    errorMessage = "SDK_VERSION_OUTDATED: The Play Age Signals SDK version is no longer supported. Ask the user to update your app to a later version that uses a recent version of the Play Age Signals SDK.";
                                    break;
                                case AgeSignalsErrorCode.INTERNAL_ERROR:
                                    errorMessage = "INTERNAL_ERROR: Unknown internal error";
                                    break;
                                default:
                                    errorMessage = "Error code: " + errorCode;
                                    break;
                            }
                            
                            Log.e(LOG_TAG, "Play Age Signals error: " + errorMessage + " (code: " + errorCode + ")");
                        } else {
                            errorMessage = exception.getMessage();
                            if (errorMessage == null || errorMessage.isEmpty()) {
                                errorMessage = "Unknown error: " + exception.getClass().getSimpleName();
                            }
                            Log.e(LOG_TAG, "Play Age Signals error: " + errorMessage);
                        }

                        if (isRetryableError(errorCode) && currentRetryAttempt < MAX_RETRY_ATTEMPTS) {
                            currentRetryAttempt++;
                            long delayMs = RETRY_DELAY_MS * (1L << (currentRetryAttempt - 1));
                            Log.d(LOG_TAG, "Retrying in " + delayMs + " ms (attempt " + currentRetryAttempt + "/" + MAX_RETRY_ATTEMPTS + ")");
                            mainHandler.postDelayed(PlayAgeSignalsController.this::performAgeSignalsRequest, delayMs);
                            return;
                        }

                        try {
                            JSONObject errorJson = new JSONObject();
                            errorJson.put("error", errorMessage);
                            if (exception instanceof AgeSignalsException) {
                                errorJson.put("errorCode", ((AgeSignalsException) exception).getErrorCode());
                            }
                            UnityPlayer.UnitySendMessage("Elephant", "OnAgeRangeResult", errorJson.toString());
                        } catch (Exception e) {
                            sendError(errorMessage);
                        }
                    });
                });
        } catch (Exception e) {
            Log.e(LOG_TAG, "Error requesting Play Age Signals: " + e.getMessage());
            sendError("Error requesting Play Age Signals: " + e.getMessage());
        }
    }
    
     */
/**
     * Returns true if the error code is retryable per Play Age Signals API documentation.
     * APP_NOT_OWNED (-9), SDK_VERSION_OUTDATED (-10), and INTERNAL_ERROR (-100) are not retryable.
     *//* 

    private boolean isRetryableError(int errorCode) {
        return errorCode != AgeSignalsErrorCode.APP_NOT_OWNED
                && errorCode != AgeSignalsErrorCode.SDK_VERSION_OUTDATED
                && errorCode != AgeSignalsErrorCode.INTERNAL_ERROR;
    }

    private void sendError(String error) {
        try {
            JSONObject errorJson = new JSONObject();
            errorJson.put("error", error);
            UnityPlayer.UnitySendMessage("Elephant", "OnAgeRangeResult", errorJson.toString());
        } catch (Exception e) {
            Log.e(LOG_TAG, "Error creating error JSON: " + e.getMessage());
        }
    }
}
 */
