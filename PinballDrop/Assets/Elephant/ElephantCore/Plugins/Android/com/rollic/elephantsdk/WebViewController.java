package com.rollic.elephantsdk;

import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.graphics.Color;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.InputMethodManager;
import android.webkit.RenderProcessGoneDetail;
import android.webkit.WebChromeClient;
import android.webkit.WebResourceRequest;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.FrameLayout;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.window.OnBackInvokedDispatcher;

import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.unity3d.player.R;
import com.unity3d.player.UnityPlayer;

/**
 * Handles a full-screen WebView experience with a toolbar and deep link handling.
 */
public class WebViewController extends AppCompatActivity {
    private static final String LOG_TAG = "[WEB VIEW CONTROLLER]";
    public static final String EXTRA_URL = "EXTRA_URL";

    // UI constants
    private static final int TOOLBAR_HEIGHT = 56;
    private static final int CLOSE_BUTTON_SIZE = 48;
    private static final int CLOSE_BUTTON_MARGIN = 16;
    private static final int PROGRESS_BAR_COLOR = Color.parseColor("#2196F3");
    private static final int CLOSE_BUTTON_ICON = android.R.drawable.ic_menu_close_clear_cancel;

    private LinearLayout root;
    private FrameLayout toolbar;
    private ImageButton closeButton;
    private ProgressBar progressBar;
    private WebView webView;
    private boolean isWebViewClosing = false;
    private boolean isDeepLinkHandled = false;
    private static boolean isWebViewOpen = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (isWebViewOpen) {
            finish();
            return;
        }

        isWebViewOpen = true;
        isWebViewClosing = false;
        isDeepLinkHandled = false;

        lockOrientation();
        hideSystemBars();

        int statusBarHeight = getStatusBarHeight();
        if (statusBarHeight == 0) {
            statusBarHeight = dpToPx(24);
        }

        root = createRootLayout();
        toolbar = createToolbar();
        root.addView(toolbar, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                dpToPx(TOOLBAR_HEIGHT) + statusBarHeight
        ));

        progressBar = createProgressBar();
        root.addView(progressBar, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, dpToPx(2)
        ));

        String url = getIntent().getStringExtra(EXTRA_URL);
        webView = createWebView();
        if (url != null) {
            webView.loadUrl(url);
        }

        root.addView(webView, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                0, 1f
        ));

        adjustLayoutForInsets(statusBarHeight);

        // Register back button callback for Android 13+
        registerBackCallback();

        setContentView(root);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        isWebViewOpen = false;
        cleanupWebView();
    }

    @Override
    public void onBackPressed() {
        if (isKeyboardVisible()) {
            hideKeyboard();
            return;
        }
        closeWebView("user_dismiss");
    }

    /***
     * Locks the screen orientation to portrait mode
     */
    private void lockOrientation() {
        try {
            setRequestedOrientation(Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR2
                    ? ActivityInfo.SCREEN_ORIENTATION_USER_PORTRAIT
                    : ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
        } catch (Exception e) {
            Log.e(LOG_TAG, "Orientation lock failed: " + e.getMessage(), e);
        }
    }

    private void hideSystemBars() {
        View decorView = getWindow().getDecorView();

        final int uiOptions = View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN;

        int combinedUiOptions = uiOptions;

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) { // API 16+
            combinedUiOptions |= View.SYSTEM_UI_FLAG_FULLSCREEN | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) { // API 19+
            combinedUiOptions |= View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY;
        }

        final int finalUiOptions = combinedUiOptions;

        decorView.setSystemUiVisibility(finalUiOptions);
        decorView.setOnSystemUiVisibilityChangeListener(visibility -> {
            if ((visibility & View.SYSTEM_UI_FLAG_FULLSCREEN) == 0) {
                decorView.setSystemUiVisibility(finalUiOptions);
            }
        });
    }

    private LinearLayout createRootLayout() {
        LinearLayout root = new LinearLayout(this);
        root.setOrientation(LinearLayout.VERTICAL);
        return root;
    }

    /**
     * Creates the toolbar with a close button
     */
    private FrameLayout createToolbar() {
        FrameLayout toolbar = new FrameLayout(this);
        toolbar.setBackgroundColor(resolveAttr(android.R.attr.colorPrimary));

        closeButton = createCloseButton();
        FrameLayout.LayoutParams closeParams = new FrameLayout.LayoutParams(
                dpToPx(CLOSE_BUTTON_SIZE), dpToPx(CLOSE_BUTTON_SIZE), Gravity.START | Gravity.CENTER_VERTICAL
        );
        closeParams.leftMargin = dpToPx(CLOSE_BUTTON_MARGIN);
        toolbar.addView(closeButton, closeParams);

        return toolbar;
    }

    private ImageButton createCloseButton() {
        ImageButton btn = new ImageButton(this);
        btn.setImageResource(CLOSE_BUTTON_ICON);
        btn.setBackgroundColor(Color.TRANSPARENT);
        btn.setColorFilter(resolveAttr(android.R.attr.colorForeground));
        btn.setOnClickListener(v -> closeWebView("user_dismiss"));
        return btn;
    }

    private ProgressBar createProgressBar() {
        ProgressBar pb = new ProgressBar(this, null, android.R.attr.progressBarStyleHorizontal);
        pb.setMax(100);
        pb.setProgress(0);
        if (pb.getProgressDrawable() != null) {
            pb.getProgressDrawable().setTint(PROGRESS_BAR_COLOR);
        }
        pb.setVisibility(View.GONE);
        return pb;
    }

    private WebView createWebView() {
        WebView wv = new WebView(this);
        wv.getSettings().setJavaScriptEnabled(true);
        wv.getSettings().setDomStorageEnabled(true);
        wv.getSettings().setMediaPlaybackRequiresUserGesture(false);
        wv.getSettings().setUseWideViewPort(true);
        wv.getSettings().setLoadWithOverviewMode(true);
        if (!wv.isHardwareAccelerated()) {
            try {
                wv.setLayerType(View.LAYER_TYPE_HARDWARE, null);
            } catch (Throwable t) {
                wv.setLayerType(View.LAYER_TYPE_SOFTWARE, null);
            }
        }

        wv.setWebViewClient(createWebViewClient());
        wv.setWebChromeClient(createWebChromeClient());

        Log.d(LOG_TAG, "WebView created");
        return wv;
    }

    private WebViewClient createWebViewClient() {
        return new WebViewClient() {
            @Override
            // For Android 7+ (API 24+)
            public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
                return handleUrl(request.getUrl().toString());
            }

            @Override
            // For Android 5–6 (API 21–23)
            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                return handleUrl(url);
            }

            @Override
            public void onReceivedHttpError(WebView view, WebResourceRequest request, android.webkit.WebResourceResponse errorResponse) {
                String requestedUrl = request.getUrl().toString();
                int statusCode = errorResponse.getStatusCode();

                if (statusCode >= 400) {
                    Log.e(LOG_TAG, "HTTP error: " + statusCode + " URL: " + requestedUrl);
                }

                super.onReceivedHttpError(view, request, errorResponse);
            }

            @Override
            public void onReceivedError(WebView view, android.webkit.WebResourceRequest request, android.webkit.WebResourceError error) {
                Log.e(LOG_TAG, "WebView load error: " + error.getDescription() + " URL: " + request.getUrl());
                super.onReceivedError(view, request, error);
            }

            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public boolean onRenderProcessGone(WebView view, RenderProcessGoneDetail detail) {
                Log.w(LOG_TAG, "Web process terminated. didCrash: " + detail.didCrash());
                view.reload();
                return true;
            }
        };
    }

    private WebChromeClient createWebChromeClient() {
        return new WebChromeClient() {
            @Override
            public void onProgressChanged(WebView view, int newProgress) {
                progressBar.setProgress(newProgress);
                progressBar.setVisibility(newProgress < 100 ? View.VISIBLE : View.GONE);
            }
        };
    }

    /***
     * Handles URL for deep linking and external store links
     * @param targetUrl
     * @return
     */
    private boolean handleUrl(String targetUrl) {
        if (isDeepLinkHandled) {
            return true;
        }

        if (targetUrl.contains("/checkout-completed-successfully")) {
            isDeepLinkHandled = true;
            closeWebView("store_return");
            return true;
        }

        if (targetUrl.startsWith("https://play.google.com/store")) {
            isDeepLinkHandled = true;
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(targetUrl));
            if (intent.resolveActivity(getPackageManager()) != null) {
                startActivity(intent);
            }
            return true;
        }

        // For older devices (API < 24), load URL manually
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.N) {
            if (webView != null) {
                webView.loadUrl(targetUrl);
            }
            return true;
        }

        // For API 24+ handled by WebView
        return false;
    }

    private void adjustLayoutForInsets(int statusBarHeight) {
        ViewCompat.setOnApplyWindowInsetsListener(root, (v, insets) -> {
            int navBarHeight = insets.getInsets(WindowInsetsCompat.Type.navigationBars()).bottom;
            toolbar.setPadding(0, statusBarHeight, 0, 0);

            if (webView != null) {
                LinearLayout.LayoutParams webParams = (LinearLayout.LayoutParams) webView.getLayoutParams();
                webParams.setMargins(0, 0, 0, navBarHeight);
                webView.setLayoutParams(webParams);
            }

            return insets;
        });
    }

    private int getStatusBarHeight() {
        int statusBarHeight = 0;
        WindowInsetsCompat insets = ViewCompat.getRootWindowInsets(getWindow().getDecorView());
        if (insets != null) {
            statusBarHeight = insets.getInsets(WindowInsetsCompat.Type.statusBars()).top;
        }
        if (statusBarHeight == 0) {
            int resId = getResources().getIdentifier("status_bar_height", "dimen", "android");
            if (resId > 0) {
                statusBarHeight = getResources().getDimensionPixelSize(resId);
            }
        }
        return statusBarHeight;
    }

    private void registerBackCallback() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            getOnBackInvokedDispatcher().registerOnBackInvokedCallback(
                    OnBackInvokedDispatcher.PRIORITY_DEFAULT,
                    () -> {
                        if (isKeyboardVisible()) {
                            hideKeyboard();
                            return;
                        }
                        closeWebView("user_dismiss");
                    }
            );
        }
    }

    /**
     * Closes WebView and sends message to Unity
     */
    private void closeWebView(String param) {
        if (isWebViewClosing) {
            return;
        }
        isWebViewClosing = true;

        try {
            sendUnityMessage("OnWebViewClosed", param);
            finish();
        } catch (Throwable t) {
            Log.e(LOG_TAG, "Error while closing WebView", t);
            finish();
        }
    }

    private void sendUnityMessage(String method, String param) {
        try {
            UnityPlayer.UnitySendMessage("Elephant", method, param);
            Log.d(LOG_TAG, "Unity message sent: " + method);
        } catch (Throwable t) {
            Log.e(LOG_TAG, "Unity message failed", t);
        }
    }

    private boolean isKeyboardVisible() {
        WindowInsetsCompat insets = ViewCompat.getRootWindowInsets(root);
        if (insets == null) {
            return false;
        }
        return insets.isVisible(WindowInsetsCompat.Type.ime());
    }

    private void hideKeyboard() {
        View view = getCurrentFocus();
        if (view != null) {
            InputMethodManager imm = (InputMethodManager) getSystemService(INPUT_METHOD_SERVICE);
            imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
        }
    }

    private int resolveAttr(int attr) {
        TypedValue typedValue = new TypedValue();
        getTheme().resolveAttribute(attr, typedValue, true);
        return typedValue.data;
    }

    private int dpToPx(int dp) {
        return (int) (dp * getResources().getDisplayMetrics().density + 0.5f);
    }

    private void cleanupWebView() {
        if (webView != null && webView.getParent() instanceof ViewGroup) {
            ((ViewGroup) webView.getParent()).removeView(webView);
            webView.loadUrl("about:blank");
            webView.stopLoading();
            webView.setWebViewClient(new WebViewClient());
            webView.setWebChromeClient(new WebChromeClient());
            webView.removeAllViews();
            webView.destroy();
            webView = null;
        }

        isDeepLinkHandled = false;
    }
}
