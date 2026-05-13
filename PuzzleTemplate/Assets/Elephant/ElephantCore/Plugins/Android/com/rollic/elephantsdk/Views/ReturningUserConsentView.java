package com.rollic.elephantsdk.Views;

import android.content.Context;
import android.graphics.Color;
import android.graphics.Typeface;
import android.os.Debug;
import android.text.method.LinkMovementMethod;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.Space;
import android.widget.TextView;
import android.content.Intent;
import android.net.Uri;

import com.rollic.elephantsdk.Hyperlink.Hyperlink;
import com.rollic.elephantsdk.Interaction.InteractionInterface;
import com.rollic.elephantsdk.Interaction.InteractionType;
import com.rollic.elephantsdk.Models.ActionType;
import com.rollic.elephantsdk.Models.ComplianceAction;
import com.rollic.elephantsdk.Models.DialogModels.ReturningUserDialogModel;
import com.rollic.elephantsdk.Models.RollicButton;
import com.rollic.elephantsdk.Utils.StringUtils;
import com.rollic.elephantsdk.Utils.Utils;

public class ReturningUserConsentView extends BaseDialog<ReturningUserDialogModel> {

    public static ReturningUserConsentView instance;

    TextView titleTextView;
    TextView contentTextView;
    RollicButton backToGameButton;

    ActionType action;

    public ReturningUserConsentView(Context ctx) {
        super(ctx);

        setupTitleTextView();
        setupContentTextView();
        setupBackToGameButton();
    }

    private void setupTitleTextView() {
        LinearLayout.LayoutParams textViewLayoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.WRAP_CONTENT);
        textViewLayoutParams.setMargins(0, 0, 0, Utils.dpToPx(10));

        titleTextView = new TextView(getContext());
        titleTextView.setTextColor(Color.WHITE);
        titleTextView.setTextAlignment(View.TEXT_ALIGNMENT_CENTER);
        titleTextView.setTextSize(20.0f);
        titleTextView.setSingleLine();
        titleTextView.setTypeface(null, Typeface.BOLD);


        contentView.addView(titleTextView, textViewLayoutParams);
    }

    private void setupContentTextView() {
        LinearLayout.LayoutParams textViewLayoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.WRAP_CONTENT);

        textViewLayoutParams.setMargins(0, 0, 0, Utils.dpToPx(10));

        contentTextView = new TextView(getContext());
        contentTextView.setTextColor(Color.WHITE);
        contentTextView.setTextSize(15.0f);
        contentTextView.setLinksClickable(true);
        contentTextView.setClickable(true);
        contentTextView.setTextIsSelectable(true);
        contentTextView.setMovementMethod(LinkMovementMethod.getInstance());

        contentView.addView(contentTextView, textViewLayoutParams);


    }


    private void setupBackToGameButton() {
        backToGameButton = new RollicButton(getContext());
        backToGameButton.setLayoutParams(new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                Utils.dpToPx(60)));
        backToGameButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                OnButtonClicked(v, true);
            }
        });

        contentView.addView(backToGameButton);
    }


    @Override
    public void configureWithModel(ReturningUserDialogModel model) {
        super.configureWithModel(model);

        this.action = action;
        configureTitleTextView(model.title);
        configureContentTextView(model.content, model.hyperlinks);
        configureBackToGameButton(model.actionButtonTitle);
    }

    private void configureTitleTextView(String title) {
        titleTextView.setText(title);
    }

    private void configureContentTextView(String content, Hyperlink[] hyperlinks) {
        contentTextView.setText(StringUtils.configurePopUpHtmlContent(content, hyperlinks));
    }

    private void configureBackToGameButton(String title) {
        backToGameButton.setText(title);
    }


    @Override
    protected void OnButtonClicked(View v, boolean shouldDismissDialog) {
        super.OnButtonClicked(v, shouldDismissDialog);

        RollicButton button = (RollicButton) v;

        if (button == backToGameButton){
            interactionInterface.OnButtonClick(InteractionType.RETURNING_USER_INFORMED);
        }

    }

    public static ReturningUserConsentView newInstance(Context ctx) {
        if (instance == null) {
            instance = new ReturningUserConsentView(ctx);
        }

        return instance;
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);

        int dialogHeight = this.getWindow().getDecorView().getHeight();
        int screenHeight = Utils.screenHeight();

        if (dialogHeight > (double)screenHeight * 0.9) {
            int maxHeight = (int) ((double) Utils.screenHeight() * 0.45);

            contentTextView.setMaxHeight(maxHeight);
        }
    }
}
