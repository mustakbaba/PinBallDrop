#import "BasePopUpView.h"
#import "PopUpViewModel.h"
#import "ReturningUserPopUpViewModel.h"
#import "Constants.h"
#import "TextView.h"
#import "Button.h"
#import "Fonts.h"
#import "Hyperlink.h"

@interface ReturningUserPopUpView : BasePopUpView

// MARK: - Properties

@property(nonatomic, readwrite) UIStackView* contentStackView;
@property(nonatomic, readwrite) UILabel* titleLabel;
@property(nonatomic, readwrite) TextView* contentTextView;
@property(nonatomic, readwrite) Button* backToGameButton;

@property(nonatomic, readwrite) ActionType action;


// MARK: - Setup

-(void)setupContentStackView;
-(void)setupTitleLabel;
-(void)setupContentTextView;
-(void)setupBackToGameButton;


// MARK: - Configure

-(void)configureTitleLabelWithTitle:(NSString*) title;
-(void)configureContentTextViewWithText:(NSString*)text hyperlinks:(NSArray<Hyperlink*>*)hyperlinks;
-(void)configureBackToGameButtonWithTitle:(NSString*) title;


// MARK: - Actions

-(void)backToGameButtonTapped:(Button *)sender;

@end
