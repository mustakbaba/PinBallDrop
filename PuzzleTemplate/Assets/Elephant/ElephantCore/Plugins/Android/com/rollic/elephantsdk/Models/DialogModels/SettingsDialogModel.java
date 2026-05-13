package com.rollic.elephantsdk.Models.DialogModels;

import com.rollic.elephantsdk.Interaction.InteractionInterface;
import com.rollic.elephantsdk.Models.ComplianceAction;

public class SettingsDialogModel extends BaseDialogModel {

    public ComplianceAction[] complianceActions;
    public  boolean showCMPButton;
    public String elephantId;

    public SettingsDialogModel(InteractionInterface interactionInterface, ComplianceAction[] complianceActions, boolean showCMPButton, String elephantId) {
        super(interactionInterface);

        this.complianceActions = complianceActions;
        this.showCMPButton = showCMPButton;
        this.elephantId = elephantId;
    }
}
