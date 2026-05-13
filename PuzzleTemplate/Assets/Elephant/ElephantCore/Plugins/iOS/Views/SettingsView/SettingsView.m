#import "SettingsView.h"

@implementation SettingsView

// MARK: - Properties


// MARK: - Initializers

- (instancetype)init {
    self = [super init];
    
    if (self) {
        [self setupTitleLabel];
        [self setupCloseButton];
        [self setupComplianceActionButtonsStackView];
    }
    
    return self;
}


// MARK: - Setup

- (void)setupTitleLabel {
    [self setTitleLabel:[UILabel new]];
    
    [[self titleLabel] setText:@"Settings"];
    [[self titleLabel] setFont:[Fonts settingsButtonTitle]];
    
    [[self contentView] addSubview:[self titleLabel]];
    [[self titleLabel] setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[[self titleLabel] topAnchor] constraintEqualToAnchor:[[self contentView] topAnchor] constant:25.0] setActive:YES];
    [[[[self titleLabel] centerXAnchor] constraintEqualToAnchor:[[self contentView] centerXAnchor]] setActive:YES];
}

- (void)setupCloseButton {
    [self setCloseButton:[[Button alloc] initWithTitle:@"X" titleColor:[UIColor blackColor] backgroundColor:nil]];
    
    [[self closeButton] addTarget:self action:@selector(closeButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    [[self contentView] addSubview:[self closeButton]];
    [[self closeButton] setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[[self closeButton] topAnchor] constraintEqualToAnchor:[[self contentView] topAnchor] constant:15.0] setActive:YES];
    [[[[self closeButton] trailingAnchor] constraintEqualToAnchor:[[self contentView] trailingAnchor] constant:-10.0] setActive:YES];
    [[[[self closeButton] widthAnchor] constraintEqualToConstant:40.0] setActive:YES];
    [[[[self closeButton] heightAnchor] constraintEqualToConstant:40.0] setActive:YES];
}

- (void)setupComplianceActionButtonsStackView {
    [self setComplianceActionButtonsStackView:[UIStackView new]];
    
    [[self complianceActionButtonsStackView] setAxis:UILayoutConstraintAxisVertical];
    [[self complianceActionButtonsStackView] setSpacing:15.0];
    
    [[self contentView] addSubview:[self complianceActionButtonsStackView]];
    [[self complianceActionButtonsStackView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[[self complianceActionButtonsStackView] topAnchor] constraintEqualToAnchor:[[self titleLabel] bottomAnchor] constant:25.0] setActive:YES];
    [[[[self complianceActionButtonsStackView] leadingAnchor] constraintEqualToAnchor:[[self contentView] leadingAnchor] constant:20.0] setActive:YES];
    [[[[self complianceActionButtonsStackView] trailingAnchor] constraintEqualToAnchor:[[self contentView] trailingAnchor] constant:-20.0] setActive:YES];
    [[[[self complianceActionButtonsStackView] bottomAnchor] constraintEqualToAnchor:[[self contentView] bottomAnchor] constant:-30.0] setActive:YES];

    [self addUnitySendMessageButton];
}

- (void)addUnitySendMessageButton {
    Button* unityButton = [[Button alloc] initWithTitle:@"Consent Settings" titleColor:[UIColor blackColor] backgroundColor:[UIColor whiteColor]];
    
    [[unityButton layer] setMasksToBounds:YES];
    [unityButton setClipsToBounds:YES];
    [[unityButton layer] setBorderWidth:1.0];
    [[unityButton layer] setBorderColor:[[UIColor blackColor] CGColor]];
    [[unityButton layer] setCornerRadius:5.0];
    [unityButton addTarget:self action:@selector(unitySendMessageButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    
    [unityButton setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[unityButton heightAnchor] constraintGreaterThanOrEqualToConstant:60.0] setActive:YES];
    
    [[self complianceActionButtonsStackView] addArrangedSubview:unityButton];
}

// Implement the action for the button
- (void)unitySendMessageButtonTapped:(Button *)sender {
    UnitySendMessage("Elephant", "ShowSecondLayer", "Pressed");
}


// MARK: - Configure

- (void)configureWithModel:(SettingsPopUpViewModel *)model showButton:(BOOL)showButton id:(NSString *)idStr {
    [super configureWithModel:model];
    
    // Store the ID if needed
    self.idStr = idStr;
    
    // Set up the compliance action buttons
    [self setComplianceActionButtonsWithActions:[model actions]];
    
    // Conditionally add the Unity send message button
    if (showButton) {
        [self addUnitySendMessageButton];
    }
    
    [self addCopyIdButton];

}

- (void)addCopyIdButton {
    Button* copyButton = [[Button alloc] initWithTitle:self.idStr titleColor:[UIColor blackColor] backgroundColor:[UIColor whiteColor]];
    
    // Adjust titleLabel properties to accommodate long text
    copyButton.titleLabel.lineBreakMode = NSLineBreakByTruncatingMiddle;
    copyButton.titleLabel.adjustsFontSizeToFitWidth = YES;
    copyButton.titleLabel.minimumScaleFactor = 0.5;
    
    [[copyButton layer] setMasksToBounds:YES];
    [copyButton setClipsToBounds:YES];
    [[copyButton layer] setBorderWidth:1.0];
    [[copyButton layer] setBorderColor:[[UIColor blackColor] CGColor]];
    [[copyButton layer] setCornerRadius:5.0];
    [copyButton addTarget:self action:@selector(copyIdButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    
    [copyButton setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[copyButton heightAnchor] constraintGreaterThanOrEqualToConstant:60.0] setActive:YES];
    
    [[self complianceActionButtonsStackView] addArrangedSubview:copyButton];
}

- (void)copyIdButtonTapped:(UIButton *)sender {
    if (self.idStr) {
        UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
        pasteboard.string = self.idStr;
    }
}


// MARK: - Actions

- (void)closeButtonTapped:(UIButton *)sender {
    [self hide];
}

- (void)complianceActionButtonTapped:(Button *)sender {
    ActionType actionType = [[sender complianceAction] action];
    
    if (actionType == URL) {
        URLPayload* urlPayload = [[sender complianceAction] getPayload];
        
        [Utils openURL:urlPayload.url];
    } else if (actionType == DATA_REQUEST) {
        [[self interactable] onButtonTapped:CALL_DATA_REQUEST];
        [self hide];
    } else if (actionType == CCPA || actionType == GDPR_AD_CONSENT) {
        PersonalizedAdsConsentPopUpView* personalizedAdsConsentPopUpView = [PersonalizedAdsConsentPopUpView createView];
        
        [personalizedAdsConsentPopUpView configureWithComplianceAction:[sender complianceAction] interactable:[self interactable]];
        
        [personalizedAdsConsentPopUpView showWithSubviewType:CONTENT];
    } else if (actionType == CUSTOM_POPUP) {
        CustomPayload* customPayload = [[sender complianceAction] getPayload];
    }
}


// MARK: - Methods

- (void)setComplianceActionButtonsWithActions:(NSArray<ComplianceAction *> *)actions {
    [self removeAllComplianceActionButtons];
    
    for(ComplianceAction* action in actions) {
        [self addComplianceActionButtonWithAction:action];
    }
}

- (void)addComplianceActionButtonWithAction:(ComplianceAction *)action {
    Button* button = [[Button alloc] initWithAction:action titleColor:[UIColor blackColor] backgroundColor:[UIColor whiteColor]];
    
    [[button layer] setMasksToBounds:YES];
    [button setClipsToBounds:YES];
    [[button layer] setBorderWidth:1.0];
    [[button layer] setBorderColor:[[UIColor blackColor] CGColor]];
    [[button layer] setCornerRadius:5.0];
    [button addTarget:self action:@selector(complianceActionButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    
    [button setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[button heightAnchor] constraintGreaterThanOrEqualToConstant:60.0] setActive:YES];
    
    [[self complianceActionButtonsStackView] addArrangedSubview:button];
}

- (void)removeAllComplianceActionButtons {
    for (Button* button in [[self complianceActionButtonsStackView] subviews]) {
        [button removeFromSuperview];
    }
}

@end
