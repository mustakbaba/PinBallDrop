#import "IdfaConsentViewController.h"
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <StoreKit/SKStoreProductViewController.h>
#import <UserNotifications/UserNotifications.h>

extern UIViewController *UnityGetGLViewController();
static UIView* unityView = nil;
static UIView* mainView = nil;

@interface IdfaConsentViewController () <UNUserNotificationCenterDelegate>

@end

@implementation IdfaConsentViewController

+ (IdfaConsentViewController*)sharedInstance
{
    static IdfaConsentViewController* sharedInstance = nil;

    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [self alloc];
    });
    return sharedInstance;
}

-(instancetype)init {
    self = [super init];
    if (self) {
        @try {
            UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
            center.delegate = self;
        }
        @catch (NSException *exception) {
            NSLog(@"An exception occurred: %@", exception.reason);
        }
    }
    return self;
}

-(void)showForceUpdate:(NSString*)message
                      :(NSString*)title {
    
    dispatch_async(dispatch_get_main_queue(), ^{
        UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:message preferredStyle:UIAlertControllerStyleAlert];
        [alertController addAction:[UIAlertAction actionWithTitle:@"OK" style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
            
            NSDictionary* infoDictionary = [[NSBundle mainBundle] infoDictionary];
            NSString* appID = infoDictionary[@"CFBundleIdentifier"];
            
            NSURL* url = [NSURL URLWithString:[NSString stringWithFormat:@"https://itunes.apple.com/lookup?bundleId=%@", appID]];
            NSURLSessionDataTask *task = [[NSURLSession sharedSession] dataTaskWithURL:url completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
                if (data) {
                    NSError *jsonError;
                    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:data options:0 error:&jsonError];
                    if (jsonError) {
                        NSLog(@"JSON parsing error: %@", jsonError.localizedDescription);
                        return;
                    }
                    
                    NSArray *results = json[@"results"];
                    if (results.count > 0) {
                        NSDictionary *appInfo = results.firstObject;
                        NSString *appStoreID = appInfo[@"trackId"];
                        if (appStoreID) {
                            NSURL* externalUrl = [NSURL URLWithString:[NSString stringWithFormat:@"https://apps.apple.com/app/id%@", appStoreID]];
                            
                            if ([[UIApplication sharedApplication] canOpenURL:externalUrl]) {
                                [[UIApplication sharedApplication] openURL:externalUrl options:@{} completionHandler:nil];
                            }
                        }
                    }
                }
            }];
            
            [task resume];
        }]];

        [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:^{
        }];
    });
}


- (void) showAlertDialog:(NSString *)titleString
                        :(NSString *)messageString {
    
    if (![messageString containsString:@"{{tos}}"]) {
        dispatch_async(dispatch_get_main_queue(), ^(void) {
           UIAlertView *alert = [[UIAlertView alloc] initWithTitle:titleString
                                                               message:messageString
                                                               delegate:self
                                                               cancelButtonTitle:@"OK"
                                                               otherButtonTitles:nil];
           [alert show];
       });

        return;
    }
 
    mainView.userInteractionEnabled = true;
    
    CGRect screenRect = [[UIScreen mainScreen] bounds];
    float screenWidth = screenRect.size.width;
    float screenHeight = screenRect.size.height;
    
    _frame = [[UIView alloc] initWithFrame:CGRectMake(0, 0, screenWidth, screenHeight)];
    [_frame setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
    _frame.layer.zPosition = MAXFLOAT - 1;
    _frame.userInteractionEnabled = true;
    
    UIView *modal = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 250, 230)];
    if (@available(iOS 13.0, *)) {
        [modal setBackgroundColor:[[UIColor systemGray5Color] colorWithAlphaComponent:0.95f]];
    } else {
        [modal setBackgroundColor:[[UIColor systemGrayColor] colorWithAlphaComponent:0.95f]];
    }
    modal.userInteractionEnabled = true;
    
    NSMutableAttributedString *attributedToSString = [[NSMutableAttributedString alloc] initWithString:@"Terms Of Service"
                                                                              attributes:@{ NSLinkAttributeName: [NSURL URLWithString:titleString],
                                                                            NSUnderlineStyleAttributeName: @(NSUnderlineStyleSingle)
                                                                           }];
    
    NSMutableAttributedString *attributedMessageString = [[NSMutableAttributedString alloc] initWithString:messageString];
    
    NSRange termsRange = [attributedMessageString.mutableString rangeOfString:@"{{tos}}"];
    [attributedMessageString replaceCharactersInRange:termsRange withAttributedString:attributedToSString];
    
    UITextView *message = [[UITextView alloc] init];
    [message setFrame:CGRectMake(12, 12, 226, 150)];
    message.attributedText = attributedMessageString;
    if (@available(iOS 13.0, *)) {
        message.textColor = [UIColor labelColor];
    } else {
        message.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    }
    message.backgroundColor = [UIColor clearColor];
    message.editable = false;
    if (@available(iOS 13.0, *)) {
        message.linkTextAttributes = @{NSForegroundColorAttributeName:[UIColor labelColor]};
    } else {
        message.linkTextAttributes = @{NSForegroundColorAttributeName:[UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0]};
    }
    

    message.dataDetectorTypes = UIDataDetectorTypeLink;
    message.textAlignment = NSTextAlignmentCenter;
    message.font= [UIFont boldSystemFontOfSize:15];
    
    UIButton *acceptButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [acceptButton setBackgroundColor:[UIColor clearColor]];
    [acceptButton setFrame:CGRectMake(0, 182, 250, 48)];
    [acceptButton setTitle:@"OK" forState:UIControlStateNormal];
    [acceptButton.titleLabel setFont:[UIFont systemFontOfSize:16]];
    [acceptButton addTarget:self
                    action:@selector(dismissAlertButton)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath2 = [UIBezierPath bezierPathWithRoundedRect:acceptButton.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer2 = [[CAShapeLayer alloc] init];
    maskLayer2.frame = acceptButton.bounds;
    maskLayer2.path  = maskPath2.CGPath;
    acceptButton.layer.mask = maskLayer2;
    
    UIBezierPath *maskModalPath = [UIBezierPath bezierPathWithRoundedRect:modal.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight | UIRectCornerTopLeft | UIRectCornerTopRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskModalLayer = [[CAShapeLayer alloc] init];
    maskModalLayer.frame = acceptButton.bounds;
    maskModalLayer.path  = maskModalPath.CGPath;
    modal.layer.mask = maskModalLayer;
    
    UIView *seperator = [[UIView alloc] initWithFrame:CGRectMake(0, 181, 250, 1)];
    if (@available(iOS 13.0, *)) {
        [seperator setBackgroundColor:[UIColor separatorColor] ];
    } else {
        [seperator setBackgroundColor:[UIColor colorWithRed:209/255.0 green:209/255.0 blue:214/255.0 alpha:1.0]];
    }
    
    [modal addSubview:message];
    [modal addSubview:seperator];
    [modal addSubview:acceptButton];
    
    modal.center = _frame.center;
    
    [_frame addSubview:modal];
    
    [mainView addSubview:_frame];
   
}


- (void) hideConsentView {
    dispatch_async(dispatch_get_main_queue(), ^(void){
        [_frame setHidden:true];
        [_frame removeFromSuperview];
    });
}

- (void) presentConsentView3:(NSString*) titleText
                            :(int) delay
                            :(int) position
                            :(NSString*) descriptionText
                            :(NSString*) buttonText
                            :(NSString*) termsText
                            :(NSString*) policyText
                            :(NSString*) termsUrl
                            :(NSString*) policyUrl {
    
    mainView.userInteractionEnabled = true;
    
    CGRect screenRect = [[UIScreen mainScreen] bounds];
    float screenWidth = screenRect.size.width;
    float screenHeight = screenRect.size.height;
    
    _frame = [[UIView alloc] initWithFrame:CGRectMake(0, 0, screenWidth, screenHeight)];
    [_frame setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
    _frame.layer.zPosition = MAXFLOAT - 1;
    _frame.userInteractionEnabled = true;
    
    UIView *modal = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 250, 230)];
    if (@available(iOS 13.0, *)) {
        [modal setBackgroundColor:[[UIColor systemGray5Color] colorWithAlphaComponent:0.95f]];
    } else {
        [modal setBackgroundColor:[[UIColor systemGrayColor] colorWithAlphaComponent:0.95f]];
    }
    modal.userInteractionEnabled = true;
    
    NSMutableAttributedString *attrRight = [[NSMutableAttributedString alloc] initWithString:termsText
                                                                           attributes:@{ NSLinkAttributeName: [NSURL URLWithString:termsUrl],
                                                                                         NSUnderlineStyleAttributeName: @(NSUnderlineStyleSingle)
                                                                           }];
    NSMutableAttributedString *attrLeft = [[NSMutableAttributedString alloc] initWithString:policyText
                                                                           attributes:@{ NSLinkAttributeName: [NSURL URLWithString:policyUrl],
                                                                                         NSUnderlineStyleAttributeName: @(NSUnderlineStyleSingle) }];
    
    NSMutableAttributedString *haydaattiributtet = [[NSMutableAttributedString alloc] initWithString:titleText];
    
    NSString *displayName = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleDisplayName"];
    NSRange nameRange = [haydaattiributtet.mutableString rangeOfString:@"{{name}}"];
    [haydaattiributtet.mutableString replaceOccurrencesOfString:@"{{name}}" withString:displayName options:NSCaseInsensitiveSearch range:nameRange];
    
    NSRange termsRange = [haydaattiributtet.mutableString rangeOfString:@"{{terms}}"];
    [haydaattiributtet replaceCharactersInRange:termsRange withAttributedString:attrRight];
    
    
    NSRange privRange = [haydaattiributtet.mutableString rangeOfString:@"{{privacy}}"];
    [haydaattiributtet replaceCharactersInRange:privRange withAttributedString:attrLeft];
    
    UITextView *description = [[UITextView alloc] init];
    [description setFrame:CGRectMake(12, 12, 226, 110)];
    description.attributedText = haydaattiributtet;
    if (@available(iOS 13.0, *)) {
        description.textColor = [UIColor labelColor];
    } else {
        description.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    }
    description.backgroundColor = [UIColor clearColor];
    description.editable = false;
    if (@available(iOS 13.0, *)) {
        description.linkTextAttributes = @{NSForegroundColorAttributeName:[UIColor labelColor]};
    } else {
        description.linkTextAttributes = @{NSForegroundColorAttributeName:[UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0]};
    }
    
    description.textContainer.maximumNumberOfLines = 5;
    description.dataDetectorTypes = UIDataDetectorTypeLink;
    description.textAlignment = NSTextAlignmentCenter;
    description.font= [UIFont boldSystemFontOfSize:16];
    
    
    NSMutableAttributedString *descriptionTextAttributed = [[NSMutableAttributedString alloc] initWithString:descriptionText];
    NSRange buttonTextRange = [descriptionTextAttributed.mutableString rangeOfString:@"{{button}}"];
    [descriptionTextAttributed.mutableString replaceOccurrencesOfString:@"{{button}}" withString:buttonText options:NSCaseInsensitiveSearch range:buttonTextRange];
    
    UILabel *secondaryText = [[UILabel alloc] init];
    [secondaryText setFrame:CGRectMake(16, 122, 218, 32)];
    secondaryText.text = descriptionTextAttributed.string;
    if (@available(iOS 13.0, *)) {
        secondaryText.textColor = [UIColor labelColor];
    } else {
        secondaryText.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    }
    secondaryText.numberOfLines = 2;
    secondaryText.textAlignment = NSTextAlignmentCenter;
    secondaryText.font= [UIFont systemFontOfSize:12];
    
    
    UIButton *acceptButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [acceptButton setBackgroundColor:[UIColor clearColor]];
    [acceptButton setFrame:CGRectMake(0, 182, 250, 48)];
    [acceptButton setTitle:buttonText forState:UIControlStateNormal];
    [acceptButton.titleLabel setFont:[UIFont systemFontOfSize:16]];
    acceptButton.tag = delay;
    [acceptButton addTarget:self
                    action:@selector(acceptButtonTap:)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath2 = [UIBezierPath bezierPathWithRoundedRect:acceptButton.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer2 = [[CAShapeLayer alloc] init];
    maskLayer2.frame = acceptButton.bounds;
    maskLayer2.path  = maskPath2.CGPath;
    acceptButton.layer.mask = maskLayer2;
    
    UIBezierPath *maskModalPath = [UIBezierPath bezierPathWithRoundedRect:modal.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight | UIRectCornerTopLeft | UIRectCornerTopRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskModalLayer = [[CAShapeLayer alloc] init];
    maskModalLayer.frame = acceptButton.bounds;
    maskModalLayer.path  = maskModalPath.CGPath;
    modal.layer.mask = maskModalLayer;
    
    UIView *seperator = [[UIView alloc] initWithFrame:CGRectMake(0, 181, 250, 1)];
    if (@available(iOS 13.0, *)) {
        [seperator setBackgroundColor:[UIColor separatorColor] ];
    } else {
        [seperator setBackgroundColor:[UIColor colorWithRed:209/255.0 green:209/255.0 blue:214/255.0 alpha:1.0]];
    }
    
    
    
    [modal addSubview:description];
    [modal addSubview:secondaryText];
    [modal addSubview:seperator];
    [modal addSubview:acceptButton];
    
    modal.center = _frame.center;
    if (position == 1) {
        CGRect r = [modal frame];
        r.origin.y = r.origin.y + 20;
        [modal setFrame:r];
    }
    
    [_frame addSubview:modal];
    
    [mainView addSubview:_frame];
}

- (void) presentConsentView2 {
    mainView.userInteractionEnabled = true;
    
    CGRect screenRect = [[UIScreen mainScreen] bounds];
    float screenWidth = screenRect.size.width;
    float screenHeight = screenRect.size.height;
    
    _frame = [[UIView alloc] initWithFrame:CGRectMake(0, 0, screenWidth, screenHeight)];
    [_frame setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
    _frame.layer.zPosition = MAXFLOAT - 1;
    _frame.userInteractionEnabled = true;
    
    CFURLRef bgUrl = CFBundleCopyResourceURL(CFBundleGetMainBundle(), CFSTR("Data/Raw/idfa_4c"), CFSTR("png"), NULL);
    NSURL *bgNsUrl = (__bridge NSURL*)bgUrl;
    NSData *data = [NSData dataWithContentsOfURL:bgNsUrl];
    UIImage *img = [UIImage imageWithData:data];

    UIImageView *modalBg = [[UIImageView alloc] init];
    [modalBg setFrame:CGRectMake(0, 0, 250, 510)];
    [modalBg setImage:img];
     modalBg.userInteractionEnabled = true;
    
    UIView *modal = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 250, 510)];
    [modal setBackgroundColor:[UIColor clearColor]];
    modal.userInteractionEnabled = true;
    
            
    UIButton *rejectButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [rejectButton setBackgroundColor:[UIColor colorWithRed:228/255.0 green:232/255.0 blue:239/255.0 alpha:1.0]];
    [rejectButton setFrame:CGRectMake(15, 298, 220, 40)];
    [rejectButton setTitle:@"Ask App Not to Track" forState:UIControlStateNormal];
    [rejectButton setTitleColor:[UIColor colorWithRed:188/255.0 green:210/255.0 blue:238/255.0 alpha:1.0] forState:UIControlStateNormal];
    [rejectButton addTarget:self
                    action:@selector(rejectButtonTap)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath = [UIBezierPath bezierPathWithRoundedRect:rejectButton.bounds byRoundingCorners:(UIRectCornerTopLeft | UIRectCornerTopRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer = [[CAShapeLayer alloc] init];
    maskLayer.frame = rejectButton.bounds;
    maskLayer.path  = maskPath.CGPath;
    rejectButton.layer.mask = maskLayer;
    
            
    UIButton *acceptButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [acceptButton setBackgroundColor:[UIColor colorWithRed:219/255.0 green:219/255.0 blue:209/255.0 alpha:1.0]];
    [acceptButton setFrame:CGRectMake(15, 338, 220, 40)];
    [acceptButton setTitle:@"Allow Tracking" forState:UIControlStateNormal];
    [acceptButton addTarget:self
                    action:@selector(acceptButtonTap)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath2 = [UIBezierPath bezierPathWithRoundedRect:acceptButton.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer2 = [[CAShapeLayer alloc] init];
    maskLayer2.frame = acceptButton.bounds;
    maskLayer2.path  = maskPath2.CGPath;
    acceptButton.layer.mask = maskLayer2;
    
    
    CFURLRef arrowUrl = CFBundleCopyResourceURL(CFBundleGetMainBundle(), CFSTR("Data/Raw/arrow2"), CFSTR("png"), NULL);
    NSURL *arrowNsUrl = (__bridge NSURL*)arrowUrl;
    NSData *arrowData = [NSData dataWithContentsOfURL:arrowNsUrl];
    UIImage *arrowImg = [UIImage imageWithData:arrowData];
    
    UIImageView *arrowImageView = [[UIImageView alloc] init];
    [arrowImageView setFrame:CGRectMake(170, 285, 30, 76)];
    [arrowImageView setImage:arrowImg];
    arrowImageView.userInteractionEnabled = false;
    
    
    UILabel *description = [[UILabel alloc] init];
    [description setFrame:CGRectMake(0, 55, 250, 100)];
    description.text = @"Don't miss out on\nall the fun.";
    description.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    description.numberOfLines = 2;
    description.textAlignment = NSTextAlignmentCenter;
    description.font= [UIFont boldSystemFontOfSize:18];
    
    
    UILabel *clickPercent = [[UILabel alloc] init];
    [clickPercent setFrame:CGRectMake(0, 385, 250, 100)];
    clickPercent.text = @"If you don't want to miss out on all the\ngames you can enjoy, please select \"Allow\n Tracking\" in the next pop-up. This allows\nus to show you personalized ads. So you\ncan watch ads that are relative to your\n interests.";
    clickPercent.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    clickPercent.numberOfLines = 6;
    clickPercent.textAlignment = NSTextAlignmentCenter;
    clickPercent.font= [UIFont boldSystemFontOfSize:10];
    
    
    [modal addSubview:modalBg];
    [modal addSubview:description];
    [modal addSubview:clickPercent];
    [modal addSubview:acceptButton];
    [modal addSubview:rejectButton];
    [modal addSubview:arrowImageView];
    
    modal.center = _frame.center;
    [_frame addSubview:modal];
    
    [mainView addSubview:_frame];
}

- (void) presentConsentView {
    mainView.userInteractionEnabled = true;
    
    CGRect screenRect = [[UIScreen mainScreen] bounds];
    float screenWidth = screenRect.size.width;
    float screenHeight = screenRect.size.height;
    
    _frame = [[UIView alloc] initWithFrame:CGRectMake(0, 0, screenWidth, screenHeight)];
    [_frame setBackgroundColor:[[UIColor blackColor] colorWithAlphaComponent:0.5f]];
    _frame.layer.zPosition = MAXFLOAT - 1;
    _frame.userInteractionEnabled = true;
    
    CFURLRef bgUrl = CFBundleCopyResourceURL(CFBundleGetMainBundle(), CFSTR("Data/Raw/idfa_bg"), CFSTR("png"), NULL);
    NSURL *bgNsUrl = (__bridge NSURL*)bgUrl;
    NSData *data = [NSData dataWithContentsOfURL:bgNsUrl];
    UIImage *img = [UIImage imageWithData:data];
    
    UIImageView *modalBg = [[UIImageView alloc] init];
    [modalBg setFrame:CGRectMake(0, 0, 250, 425)];
    [modalBg setImage:img];
     modalBg.userInteractionEnabled = true;

    
    UIView *modal = [[UIView alloc] initWithFrame:CGRectMake(0, 0, 250, 425)];
    [modal setBackgroundColor:[UIColor clearColor]];
    modal.userInteractionEnabled = true;
    
            
    UIButton *rejectButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [rejectButton setBackgroundColor:[UIColor colorWithRed:228/255.0 green:232/255.0 blue:239/255.0 alpha:1.0]];
    [rejectButton setFrame:CGRectMake(15, 278, 220, 40)];
    [rejectButton setTitle:@"Ask App Not to Track" forState:UIControlStateNormal];
    [rejectButton setTitleColor:[UIColor colorWithRed:188/255.0 green:210/255.0 blue:238/255.0 alpha:1.0] forState:UIControlStateNormal];
    [rejectButton addTarget:self
                    action:@selector(rejectButtonTap)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath = [UIBezierPath bezierPathWithRoundedRect:rejectButton.bounds byRoundingCorners:(UIRectCornerTopLeft | UIRectCornerTopRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer = [[CAShapeLayer alloc] init];
    maskLayer.frame = rejectButton.bounds;
    maskLayer.path  = maskPath.CGPath;
    rejectButton.layer.mask = maskLayer;
    
            
    UIButton *acceptButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [acceptButton setBackgroundColor:[UIColor colorWithRed:219/255.0 green:219/255.0 blue:209/255.0 alpha:1.0]];
    [acceptButton setFrame:CGRectMake(15, 318, 220, 40)];
    [acceptButton setTitle:@"Allow Tracking" forState:UIControlStateNormal];
    [acceptButton addTarget:self
                    action:@selector(acceptButtonTap)
    forControlEvents:UIControlEventTouchUpInside];
    
    UIBezierPath *maskPath2 = [UIBezierPath bezierPathWithRoundedRect:acceptButton.bounds byRoundingCorners:(UIRectCornerBottomLeft | UIRectCornerBottomRight) cornerRadii:CGSizeMake(10.0, 10.0)];

    CAShapeLayer *maskLayer2 = [[CAShapeLayer alloc] init];
    maskLayer2.frame = acceptButton.bounds;
    maskLayer2.path  = maskPath2.CGPath;
    acceptButton.layer.mask = maskLayer2;
    
    
    CFURLRef arrowUrl = CFBundleCopyResourceURL(CFBundleGetMainBundle(), CFSTR("Data/Raw/arrow2"), CFSTR("png"), NULL);
    NSURL *arrowNsUrl = (__bridge NSURL*)arrowUrl;
    NSData *arrowData = [NSData dataWithContentsOfURL:arrowNsUrl];
    UIImage *arrowImg = [UIImage imageWithData:arrowData];
    
    UIImageView *arrowImageView = [[UIImageView alloc] init];
    [arrowImageView setFrame:CGRectMake(170, 260, 30, 76)];
    [arrowImageView setImage:arrowImg];
    arrowImageView.userInteractionEnabled = false;
    
    
    
    UILabel *description = [[UILabel alloc] init];
    [description setFrame:CGRectMake(0, 50, 250, 100)];
    description.text = @"To keep this game free, please\n give us your consent on the\n next screen.";
    description.textColor = [UIColor colorWithRed:62/255.0 green:33/255.0 blue:122/255.0 alpha:1.0];
    description.numberOfLines = 3;
    description.textAlignment = NSTextAlignmentCenter;
    description.font= [description.font fontWithSize:13];
    
    NSRange range1 = [description.text rangeOfString:@"To keep this game free, please\n give us your consent on the\n"];
    NSRange range2 = [description.text rangeOfString:@"next screen."];
    
    NSMutableAttributedString *attributedText = [[NSMutableAttributedString alloc] initWithString:description.text];

    [attributedText setAttributes:@{NSFontAttributeName:[UIFont systemFontOfSize:13]}
                            range:range1];
    [attributedText setAttributes:@{NSFontAttributeName:[UIFont boldSystemFontOfSize:13]}
                            range:range2];

    description.attributedText = attributedText;
    
    
    UILabel *clickPercent = [[UILabel alloc] init];
    [clickPercent setFrame:CGRectMake(0, 232, 250, 30)];
    clickPercent.text = @"94% players click";
    clickPercent.textColor = [UIColor whiteColor];
    clickPercent.numberOfLines = 1;
    clickPercent.textAlignment = NSTextAlignmentCenter;
    clickPercent.font= [UIFont boldSystemFontOfSize:14];
    
    
    [modal addSubview:modalBg];
    [modal addSubview:description];
    [modal addSubview:clickPercent];
    [modal addSubview:acceptButton];
    [modal addSubview:rejectButton];
    [modal addSubview:arrowImageView];
    
    modal.center = _frame.center;
    [_frame addSubview:modal];
    
    [mainView addSubview:_frame];
}

- (void)willStartWithViewController:(UIViewController*)controller
{
    // Set the root view to be your custom view controller's root view.
    _rootView = _rootController.view;
    
    
    unityView = UnityGetGLView();
    
    [_rootView addSubview:unityView];
    [_rootView sendSubviewToBack:unityView];
    
    mainView = _rootView;
}

- (UIView*) getRootView{
    return mainView;
}

- (void)acceptButtonTap:(id) sender {
    int delay = 0;
    if (sender != nil) {
        UIButton *clicked = (UIButton *) sender;
        delay = (int) clicked.tag;
    }
    dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delay * NSEC_PER_SEC));
    dispatch_after(popTime, dispatch_get_main_queue(), ^(void){
        UnitySendMessage("Elephant", "sendUiConsentStatus", "accepted");
        [self requstPermissionForAppTracking];
    });
    [self hideConsentView];
}

- (void)rejectButtonTap {
    UnitySendMessage("Elephant", "sendUiConsentStatus", "denied");
    [self hideConsentView];
}

- (void)dismissAlertButton {
    [self hideConsentView];
}


- (void)requstPermissionForAppTracking {
    if (@available(iOS 14.0, *)) {
        
       __block NSString *statusText;
       [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
           switch (status) {
               case ATTrackingManagerAuthorizationStatusAuthorized:
                   statusText = @"Authorized";
                   break;
               case ATTrackingManagerAuthorizationStatusDenied:
                   statusText = @"Denied";
                   break;
               case ATTrackingManagerAuthorizationStatusRestricted:
                   statusText = @"Restricted";
                   break;
               case ATTrackingManagerAuthorizationStatusNotDetermined:
                   statusText = @"NotDetermined";
                   break;
               default:
                   statusText = @"NotDetermined";
                   break;
           }

           UnitySendMessage("Elephant", "setConsentStatus", statusText.UTF8String);
           [self hideConsentView];
       }];
    }
    
}

// Delegation methods
- (void)application:(UIApplication *)app didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    @try {
        // Store the device token for later use
        self.deviceToken = deviceToken;

        // Convert NSData to a string representation of the device token
        const unsigned char* tokenBytes = (const unsigned char*)deviceToken.bytes;
        NSUInteger tokenLength = deviceToken.length;
        char deviceTokenString[tokenLength * 2 + 1];
        for (NSUInteger i = 0; i < tokenLength; i++) {
            snprintf(&deviceTokenString[i * 2], 3, "%02x", tokenBytes[i]);
        }
        deviceTokenString[tokenLength * 2] = '\0';

        // Convert the C-string to an NSString
        NSString *deviceTokenNS = [NSString stringWithUTF8String:deviceTokenString];

        NSLog(@"notifisok: %@", deviceTokenNS);

        UnitySendMessage("Elephant", "SetDeviceToken", deviceTokenNS.UTF8String);
    }
    @catch (NSException *exception) {
        NSLog(@"An exception occurred: %@", exception.reason);
    }
}

- (void)application:(UIApplication *)app didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
    @try {
        // Handle the registration error here (if needed)
        NSLog(@"Failed to register for remote notifications with error: %@", error.localizedDescription);
        
        UnitySendMessage("Elephant", "SetDeviceToken", "ERROR");
        
        // Schedule a delayed registration after 30 seconds
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(30 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
            [[UIApplication sharedApplication] registerForRemoteNotifications];
        });
        
        // Call the registered Unity method to notify the error message
        NSString *errorMessage = [NSString stringWithFormat:@"Failed to register for remote notifications with error: %@", error.localizedDescription];
        UnitySendMessage("Elephant", "HandleRegistrationError", [errorMessage UTF8String]);
    }
    @catch (NSException *exception) {
        NSLog(@"An exception occurred: %@", exception.reason);
    }
}


- (void)application:(UIApplication *)app didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult))completionHandler {
    @try {
        UnitySendMessage("Elephant", "ReceiveNotificationMessage", "testIOS");
        if(app.applicationState == UIApplicationStateInactive) {
            NSLog(@"Inactive - the user has tapped in the notification when app was closed or in background");
            completionHandler(UIBackgroundFetchResultNewData);
        }
        else if (app.applicationState == UIApplicationStateBackground) {
            NSLog(@"application Background - notification has arrived when app was in background");
            NSString* contentAvailable = [NSString stringWithFormat:@"%@", [[userInfo valueForKey:@"aps"] valueForKey:@"content-available"]];
            
            if([contentAvailable isEqualToString:@"1"]) {
                NSLog(@"content-available is equal to 1");
                completionHandler(UIBackgroundFetchResultNewData);
            }
        }
        else {
            NSLog(@"application Active - notification has arrived while app was opened");
            completionHandler(UIBackgroundFetchResultNewData);
        }
    }
    @catch (NSException *exception) {
        NSLog(@"An exception occurred: %@", exception.reason);
        completionHandler(UIBackgroundFetchResultFailed);
    }
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completion {
    @try {
        NSString *actionIdentifier = response.actionIdentifier;
        NSDictionary *userInfo = response.notification.request.content.userInfo;
        NSString *notificationId = userInfo[@"notification_id"];
        NSString *messageId = userInfo[@"message_id"];
        NSString *jobId = userInfo[@"job_id"];
        NSString *scheduledAt = userInfo[@"scheduled_at"];

        if (!notificationId || [notificationId isKindOfClass:[NSNull class]]) {
            notificationId = @"null";
        }

        if (!messageId || [messageId isKindOfClass:[NSNull class]]) {
            messageId = @"null";
        }
        
        if (!jobId || [jobId isKindOfClass:[NSNull class]]) {
            jobId = @"null";
        }
        
        if (!scheduledAt || [scheduledAt isKindOfClass:[NSNull class]]) {
            scheduledAt = @"null";
        }

        if ([actionIdentifier isEqualToString:UNNotificationDefaultActionIdentifier]) {
            NSString *combinedMessage = [NSString stringWithFormat:@"%@;%@;%@;%@", notificationId, messageId, jobId, scheduledAt];
            UnitySendMessage("Elephant", "SendPushNotificationOpenEvent", [combinedMessage UTF8String]);
        }
        
        if (completion) {
            completion();
        }
    }
    @catch (NSException *exception) {
        NSLog(@"An exception occurred: %@", exception.reason);
    }
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completion {
    @try {
        UIApplicationState appState = [UIApplication sharedApplication].applicationState;
        
        if (appState == UIApplicationStateActive) {
            NSLog(@"App is in foreground, not showing notification");
            
            UnitySendMessage("Elephant", "ReceiveNotificationMessage", "Notification received in foreground");
            
            if (completion) {
                completion(UNNotificationPresentationOptionNone);
            }
        } else {
            NSLog(@"App is in background, showing notification");
            
            if (completion) {
                if (@available(iOS 14.0, *)) {
                    completion(UNNotificationPresentationOptionList | 
                             UNNotificationPresentationOptionBanner | 
                             UNNotificationPresentationOptionSound);
                } else {
                    completion(UNNotificationPresentationOptionAlert | 
                             UNNotificationPresentationOptionSound);
                }
            }
        }
    }
    @catch (NSException *exception) {
        NSLog(@"An exception occurred: %@", exception.reason);
        if (completion) {
            completion(UNNotificationPresentationOptionNone);
        }
    }
}
	

@end


IMPL_APP_CONTROLLER_SUBCLASS(IdfaConsentViewController)
