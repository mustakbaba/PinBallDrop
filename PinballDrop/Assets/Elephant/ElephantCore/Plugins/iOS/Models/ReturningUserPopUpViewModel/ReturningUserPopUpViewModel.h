#import "PopUpViewModel.h"
#import "ActionType.h"

@interface ReturningUserPopUpViewModel : PopUpViewModel

// MARK: - Properties

@property(nonatomic, readwrite) ActionType action;


// MARK: - Initializers

-(instancetype)initWithAction:(ActionType)action
                        title:(NSString*)title
                         text:(NSString*)text
        backToGameButtonTitle:(NSString*)backToGameButtonTitle
                   hyperlinks:(NSArray<Hyperlink*>*)hyperlinks
                 interactable:(id<Interactable>)interactable;

@end
