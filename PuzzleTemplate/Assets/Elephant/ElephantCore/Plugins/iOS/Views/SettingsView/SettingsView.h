#import "SettingsPopUpViewModel.h"
#import "Fonts.h"
#import "Button.h"
#import "PersonalizedAdsConsentPopUpView.h"
#import "PopUpView.h"

@interface SettingsView : BasePopUpView

// MARK: - Properties

@property(nonatomic, readwrite) UILabel* titleLabel;
@property(nonatomic, readwrite) UIButton* closeButton;
@property(nonatomic, readwrite) UIStackView* complianceActionButtonsStackView;
@property (nonatomic, strong) NSString *idStr;

// MARK: - Initializers


// MARK: - Setup

-(void)setupTitleLabel;
-(void)setupCloseButton;
-(void)setupComplianceActionButtonsStackView;


// MARK: - Actions

-(void)closeButtonTapped:(UIButton *)sender;
-(void)complianceActionButtonTapped:(Button*)sender;


// MARK: - Methods

-(void)setComplianceActionButtonsWithActions:(NSArray<ComplianceAction*>*)actions;
-(void)addComplianceActionButtonWithAction:(ComplianceAction*)action;
-(void)removeAllComplianceActionButtons;
-(void)addUnitySendMessageButton;
-(void)unitySendMessageButtonTapped:(Button *)sender;
-(void)configureWithModel:(SettingsPopUpViewModel *)model showButton:(BOOL)showButton id:(NSString *)idStr;
-(void)addCopyIdButton;
-(void)copyIdButtonTapped:(UIButton *)sender;


@end
