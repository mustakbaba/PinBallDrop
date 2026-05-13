package com.rollic.elephantsdk;

import android.app.ActivityManager;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Resources;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Build;
import android.text.TextUtils;
import androidx.preference.PreferenceManager;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.core.app.NotificationCompat;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.util.List;
import java.util.Map;

public class MyFirebaseMessagingService extends FirebaseMessagingService {
    private static final String TAG = "MyFirebaseMsgService";
    
    @Override
    public void onCreate() {
        super.onCreate();
        try {
            Resources res = UnityPlayer.currentActivity.getResources();
            int iconId = res.getIdentifier("app_icon", "mipmap", UnityPlayer.currentActivity.getPackageName());

            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(this);
            SharedPreferences.Editor editor = preferences.edit();
            editor.putInt("saved_icon_id", iconId);
            editor.apply();
        }
        catch(Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        try {
            Log.d(TAG, "From: " + remoteMessage.getFrom());
            
            if (isAppInForeground()) {
                Log.d(TAG, "App is in foreground, not showing notification");
                UnityPlayer.UnitySendMessage("Elephant", "ReceiveNotificationMessage", "Notification received in foreground");
                return;
            }
            
            Log.d(TAG, "App is in background, showing notification");
            
            Map<String, String> data = remoteMessage.getData();
            
            String title = "";
            String body = "";
            
            if (remoteMessage.getNotification() != null) {
                title = remoteMessage.getNotification().getTitle();
                body = remoteMessage.getNotification().getBody();
            }
            
            if (TextUtils.isEmpty(title)) {
                title = data.get("title");
            }
            if (TextUtils.isEmpty(body)) {
                body = data.get("body");
            }
            
            String notificationId = data.get("notification_id");
            String messageId = data.get("message_id");
            String jobId = data.get("job_id");
            String scheduledAt = data.get("scheduled_at");
            
            sendNotification(body, notificationId, messageId, jobId, scheduledAt, title, body);

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onNewToken(@NonNull String token) {
        Log.d(TAG, "Refreshed token: " + token);
        UnityPlayer.UnitySendMessage("Elephant", "SetDeviceToken", token);
    }

    private boolean isAppInForeground() {
        ActivityManager activityManager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
        List<ActivityManager.RunningAppProcessInfo> appProcesses = activityManager.getRunningAppProcesses();
        
        if (appProcesses == null) {
            return false;
        }
        
        final String packageName = getPackageName();
        for (ActivityManager.RunningAppProcessInfo appProcess : appProcesses) {
            if (appProcess.importance == ActivityManager.RunningAppProcessInfo.IMPORTANCE_FOREGROUND 
                && appProcess.processName.equals(packageName)) {
                return true;
            }
        }
        return false;
    }

    private void sendNotification(String messageBody, String notificationId, String messageId, 
                                String jobId, String scheduledAt, String title, String body) {

        Intent intent = new Intent(this, UnityPlayerActivity.class);
        intent.setAction("notification_opened");
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        
        if (!TextUtils.isEmpty(notificationId)) {
            intent.putExtra("notification_id", notificationId);
        }
        if (!TextUtils.isEmpty(messageId)) {
            intent.putExtra("message_id", messageId);
        }
        if (!TextUtils.isEmpty(jobId)) {
            intent.putExtra("job_id", jobId);
        }
        if (!TextUtils.isEmpty(scheduledAt)) {
            intent.putExtra("scheduled_at", scheduledAt);
        }
        
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        String channelId = "fcm_default_channel";
        Uri defaultSoundUri = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);

        SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(this);
        int savedIconId = preferences.getInt("saved_icon_id", 0);

        if (savedIconId == 0) {
            try {
                savedIconId = getApplicationInfo().icon;
            } catch (Exception e) {
                savedIconId = android.R.drawable.ic_dialog_info;
            }
        }

        NotificationCompat.Builder notificationBuilder =
                new NotificationCompat.Builder(this, channelId)
                        .setSmallIcon(savedIconId)
                        .setContentTitle(title)
                        .setContentText(body)
                        .setAutoCancel(true)
                        .setSound(defaultSoundUri)
                        .setContentIntent(pendingIntent);

        NotificationManager notificationManager =
                (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(channelId,
                    "Push Notifications",
                    NotificationManager.IMPORTANCE_DEFAULT);
            channel.setDescription("Notifications from the app");
            notificationManager.createNotificationChannel(channel);
        }

        notificationManager.notify(0, notificationBuilder.build());
    }

    private void handleNow() {
        Log.d(TAG, "Short lived task is done.");
    }
}