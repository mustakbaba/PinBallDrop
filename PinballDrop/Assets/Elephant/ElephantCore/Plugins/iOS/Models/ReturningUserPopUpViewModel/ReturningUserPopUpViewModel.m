#import "ReturningUserPopUpViewModel.h"

@implementation ReturningUserPopUpViewModel

// MARK: - Initializers

- (instancetype)initWithAction:(ActionType)action title:(NSString *)title text:(NSString *)text  backToGameButtonTitle:(NSString *)backToGameButtonTitle hyperlinks:(NSArray<Hyperlink *> *)hyperlinks interactable:(id<Interactable>)interactable {
    self = [super initWithTitle:title text:text buttonTitle:backToGameButtonTitle hyperlinks:hyperlinks interactable:interactable];
    
    if (self) {
        [self setAction:action];
    }
    
    return self;
}

@end

