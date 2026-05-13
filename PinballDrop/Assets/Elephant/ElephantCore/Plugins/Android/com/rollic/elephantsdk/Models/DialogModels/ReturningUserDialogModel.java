package com.rollic.elephantsdk.Models.DialogModels;

import android.text.SpannableString;

import com.rollic.elephantsdk.Hyperlink.Hyperlink;
import com.rollic.elephantsdk.Interaction.InteractionInterface;
import com.rollic.elephantsdk.Models.ActionType;
import com.rollic.elephantsdk.Models.ComplianceAction;
import com.rollic.elephantsdk.Payload.PersonalizedAdsPayload;

public class ReturningUserDialogModel extends GenericDialogModel {

    public ActionType action;

    public ReturningUserDialogModel(InteractionInterface interactionInterface, ActionType action, String title,
                              String content, String backToGameButtonTitle, Hyperlink[] hyperlinks) {
        super(interactionInterface, title, content, backToGameButtonTitle, hyperlinks);

        this.action = action;
    }



}