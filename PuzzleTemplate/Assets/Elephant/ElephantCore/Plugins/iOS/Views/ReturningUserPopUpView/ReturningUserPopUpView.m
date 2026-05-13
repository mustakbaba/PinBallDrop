#import "ReturningUserPopUpView.h"

@implementation ReturningUserPopUpView

// MARK: - Properties

// MARK: - Initializers

- (instancetype)init {
    self = [super init];
    
    if (self) {
        [self setupContentStackView];
        [self setupTitleLabel];
        [self setupContentTextView];
        [self setupBackToGameButton];
    }
    
    return self;
}


// MARK: - Setup

- (void)setupContentStackView {
    [self setContentStackView:[UIStackView new]];
    
    [[self contentStackView] setAxis:UILayoutConstraintAxisVertical];
    
    [[self contentView] addSubview:[self contentStackView]];
    [[self contentStackView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    [[[[self contentStackView] topAnchor] constraintEqualToAnchor: [[self contentView] topAnchor] constant:15.0] setActive:YES];
    [[[[self contentStackView] leadingAnchor] constraintEqualToAnchor: [[self contentView] leadingAnchor]] setActive:YES];
    [[[[self contentStackView] trailingAnchor] constraintEqualToAnchor: [[self contentView] trailingAnchor]] setActive:YES];
    [[[[self contentStackView] bottomAnchor] constraintEqualToAnchor: [[self contentView] bottomAnchor]] setActive:YES];
}

- (void)setupTitleLabel {
    [self setTitleLabel:[UILabel new]];
    
    [[self titleLabel] setFont:[Fonts popupTitle]];
    [[self titleLabel] setTextColor:[Colors textViewText]];
    [[self titleLabel] setTextAlignment:NSTextAlignmentCenter];
    
    [[self contentStackView] addArrangedSubview:[self titleLabel]];
}

- (void)setupContentTextView {
    [self setContentTextView:[TextView new]];
    
    [[self contentTextView] setTextContainerInset:UIEdgeInsetsMake(20.0, 10.0, 15.0, 10.0)];
    [[self contentStackView] addArrangedSubview:[self contentTextView]];
}

- (void)setupBackToGameButton {
    [self setBackToGameButton:[Button new]];
    
    [[self backToGameButton] setTitleColor:[Colors buttonTitle]];
    [[self backToGameButton] setBackgroundColor:[Colors buttonBackground]];
    [[self backToGameButton] setBorderWithWidth:1.0 color:[Colors separatorBackground]];
    [[[[self backToGameButton] heightAnchor] constraintGreaterThanOrEqualToConstant:60.0] setActive:YES];
    [[self backToGameButton] addTarget:self action:@selector(backToGameButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    [[self contentStackView] addArrangedSubview:[self backToGameButton]];
}


// MARK: - Configure

- (void)configureWithModel:(BasePopUpViewModel *)model {
    [super configureWithModel:model];
    
    ReturningUserPopUpViewModel* returningUserModel =
        (ReturningUserPopUpViewModel*) model;
    
    [self setAction:[returningUserModel action]];
    [self configureTitleLabelWithTitle:[returningUserModel title]];
    [self configureContentTextViewWithText:[returningUserModel text] hyperlinks:[returningUserModel hyperlinks]];
    [self configureBackToGameButtonWithTitle:[returningUserModel buttonTitle]];
}

- (void)configureTitleLabelWithTitle:(NSString *)title {
    [[self titleLabel] setText:title];
}

- (void)configureContentTextViewWithText:(NSString *)text hyperlinks:(NSArray<Hyperlink *> *)hyperlinks {
    [[self contentTextView] setTextWithHtmlString:[Hyperlink configurePopUpHtmlContentWithContent:text hyperlinks:hyperlinks]];
}

- (void)configureBackToGameButtonWithTitle:(NSString *)title {
    [[self backToGameButton] setTitle:title];
}


// MARK: - Actions

- (void)backToGameButtonTapped:(Button *)sender {
    UnitySendMessage("Elephant", "receiveReturningPopUpResponse", "OK");
    [self hide];
}

@end
