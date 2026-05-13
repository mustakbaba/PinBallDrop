package com.rollic.elephantsdk;
import com.unity3d.player.UnityPlayerActivity;

import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.NonNull;

public class ElephantActivity extends UnityPlayerActivity {

  protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
  }
  
    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == 112) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "ReceiveNotificationPermission", "granted");
            } else {
                com.unity3d.player.UnityPlayer.UnitySendMessage("Elephant", "ReceiveNotificationPermission", "denied");
            }
        }
    }
}