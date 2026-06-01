#import "WebViewController.h"

@interface WebViewController () <WKNavigationDelegate, WKUIDelegate>
@property (nonatomic, strong) UIProgressView *progressView;
@property (nonatomic, strong) UILabel *titleLabel;
@property (nonatomic, strong) UIView *navigationContainer;
@property (nonatomic, strong) UIVisualEffectView *blurView;
@end

@implementation WebViewController

// MARK: - Life Cycle

- (void)viewDidLoad {
    [super viewDidLoad];
    
    [[self view] setBackgroundColor:[UIColor systemBackgroundColor]];
    
    [self setupNavigationBar];
    [self setupDoneButton];
    [self setupDismissButton];
    [self setupWebView];
    [self setupProgressView];
    
    [[self webView] addObserver:self
                     forKeyPath:@"estimatedProgress"
                        options:NSKeyValueObservingOptionNew
                        context:nil];
    
    [[self webView] addObserver:self
                     forKeyPath:@"title"
                        options:NSKeyValueObservingOptionNew
                        context:nil];
    
    // Ensure proper view hierarchy - blur view should be on top of webView
    [self.view bringSubviewToFront:self.blurView];
}

- (void)dealloc {
    [[self webView] removeObserver:self forKeyPath:@"estimatedProgress"];
    [[self webView] removeObserver:self forKeyPath:@"title"];
}

- (UIStatusBarStyle)preferredStatusBarStyle {
    if (@available(iOS 13.0, *)) {
        return UIStatusBarStyleDarkContent;
    } else {
        return UIStatusBarStyleDefault;
    }
}

- (void)viewWillAppear:(BOOL)animated {
    [super viewWillAppear:animated];
    [self setModalPresentationStyle:UIModalPresentationFullScreen];
}

// MARK: - Setup

- (void)setupNavigationBar {
    UIBlurEffect *blurEffect = [UIBlurEffect effectWithStyle:UIBlurEffectStyleSystemChromeMaterial];
    [self setBlurView:[[UIVisualEffectView alloc] initWithEffect:blurEffect]];
    
    [[self view] addSubview:[self blurView]];
    [[self blurView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    [self setNavigationContainer:[UIView new]];
    [[self navigationContainer] setBackgroundColor:[UIColor clearColor]];
    
    [self setNavigationBarView:[self navigationContainer]];
    
    [[self blurView].contentView addSubview:[self navigationContainer]];
    [[self navigationContainer] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    // Setup blur view constraints (but don't reference webView yet)
    [[[[self blurView] topAnchor] constraintEqualToAnchor:[[self view] topAnchor]] setActive:YES];
    [[[[self blurView] leadingAnchor] constraintEqualToAnchor:[[self view] leadingAnchor]] setActive:YES];
    [[[[self blurView] trailingAnchor] constraintEqualToAnchor:[[self view] trailingAnchor]] setActive:YES];
    [[[[self blurView] bottomAnchor] constraintEqualToAnchor:[[self navigationContainer] bottomAnchor]] setActive:YES];
    
    [[[[self navigationContainer] topAnchor] constraintEqualToAnchor:[[self view] safeAreaLayoutGuide].topAnchor] setActive:YES];
    [[[[self navigationContainer] leadingAnchor] constraintEqualToAnchor:[[self blurView] leadingAnchor]] setActive:YES];
    [[[[self navigationContainer] trailingAnchor] constraintEqualToAnchor:[[self blurView] trailingAnchor]] setActive:YES];
    [[[[self navigationContainer] heightAnchor] constraintEqualToConstant:56.0] setActive:YES];
    
    [self setupTitleLabel];
    [self addSeparatorLine];
}

- (void)setupTitleLabel {
    [self setTitleLabel:[UILabel new]];
    [[self titleLabel] setText:@""];
    [[self titleLabel] setFont:[UIFont systemFontOfSize:17.0 weight:UIFontWeightSemibold]];
    [[self titleLabel] setTextColor:[UIColor labelColor]];
    [[self titleLabel] setTextAlignment:NSTextAlignmentCenter];
    [[self titleLabel] setLineBreakMode:NSLineBreakByTruncatingMiddle];
    
    [[self navigationContainer] addSubview:[self titleLabel]];
    [[self titleLabel] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    [[[[self titleLabel] centerYAnchor] constraintEqualToAnchor:[[self navigationContainer] centerYAnchor]] setActive:YES];
    [[[[self titleLabel] leadingAnchor] constraintEqualToAnchor:[[self navigationContainer] leadingAnchor] constant:60.0] setActive:YES];
    [[[[self titleLabel] trailingAnchor] constraintEqualToAnchor:[[self navigationContainer] trailingAnchor] constant:-60.0] setActive:YES];
}

- (void)setupDismissButton {
    [self setDismissButton:[UIButton buttonWithType:UIButtonTypeSystem]];
    
    if (@available(iOS 13.0, *)) {
        UIImage *xImage = [UIImage systemImageNamed:@"xmark"];
        UIImageSymbolConfiguration *config = [UIImageSymbolConfiguration configurationWithPointSize:17.0 weight:UIImageSymbolWeightMedium];
        xImage = [xImage imageWithConfiguration:config];
        [[self dismissButton] setImage:xImage forState:UIControlStateNormal];
    } else {
        [[self dismissButton] setTitle:@"✕" forState:UIControlStateNormal];
        [[self dismissButton] titleLabel].font = [UIFont systemFontOfSize:20.0];
    }
    
    [[self dismissButton] setTintColor:[UIColor labelColor]];
    [[self dismissButton] addTarget:self action:@selector(dismissButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    
    [[self navigationContainer] addSubview:[self dismissButton]];
    [[self dismissButton] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    [[[[self dismissButton] centerYAnchor] constraintEqualToAnchor:[[self navigationContainer] centerYAnchor]] setActive:YES];
    [[[[self dismissButton] leadingAnchor] constraintEqualToAnchor:[[self navigationContainer] leadingAnchor] constant:16.0] setActive:YES];
    [[[[self dismissButton] widthAnchor] constraintEqualToConstant:44.0] setActive:YES];
    [[[[self dismissButton] heightAnchor] constraintEqualToConstant:44.0] setActive:YES];
}

- (void)setupDoneButton {
    [self setDoneButton:[UIButton buttonWithType:UIButtonTypeSystem]];
    [[self doneButton] setHidden:YES];
}

- (void)addSeparatorLine {
    UIView *separator = [UIView new];
    [separator setBackgroundColor:[[UIColor separatorColor] colorWithAlphaComponent:0.3]];
    
    [[self blurView].contentView addSubview:separator];
    [separator setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    [[[separator heightAnchor] constraintEqualToConstant:0.5] setActive:YES];
    [[[separator leadingAnchor] constraintEqualToAnchor:[[self blurView] leadingAnchor]] setActive:YES];
    [[[separator trailingAnchor] constraintEqualToAnchor:[[self blurView] trailingAnchor]] setActive:YES];
    [[[separator bottomAnchor] constraintEqualToAnchor:[[self blurView] bottomAnchor]] setActive:YES];
}

- (void)setupProgressView {
    [self setProgressView:[[UIProgressView alloc] initWithProgressViewStyle:UIProgressViewStyleBar]];
    [[self progressView] setProgressTintColor:[UIColor systemBlueColor]];
    [[self progressView] setTrackTintColor:[UIColor clearColor]];
    [[self progressView] setAlpha:0.0];
    
    [[self view] addSubview:[self progressView]];
    [[self progressView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    [[[[self progressView] topAnchor] constraintEqualToAnchor:[[self blurView] bottomAnchor]] setActive:YES];
    [[[[self progressView] leadingAnchor] constraintEqualToAnchor:[[self view] leadingAnchor]] setActive:YES];
    [[[[self progressView] trailingAnchor] constraintEqualToAnchor:[[self view] trailingAnchor]] setActive:YES];
    [[[[self progressView] heightAnchor] constraintEqualToConstant:2.0] setActive:YES];
}

- (void)setupWebView {
    WKWebViewConfiguration *configuration = [[WKWebViewConfiguration alloc] init];
    [configuration setAllowsInlineMediaPlayback:YES];
    
    if (@available(iOS 10.0, *)) {
        configuration.mediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypeNone;
    } else {
        if (@available(iOS 9.0, *)) {
            configuration.requiresUserActionForMediaPlayback = NO;
        } else {
            configuration.mediaPlaybackRequiresUserAction = NO;
        }
    }
    
    [self setWebView:[[WKWebView alloc] initWithFrame:CGRectZero configuration:configuration]];
    
    [[self webView] setAllowsBackForwardNavigationGestures:YES];
    [[self webView] setNavigationDelegate:self];
    [[self webView] setUIDelegate:self];
    [[self webView] setBackgroundColor:[UIColor systemBackgroundColor]];
    [[self webView] setOpaque:YES];
    
    NSString *jScript = @"var meta = document.createElement('meta'); meta.setAttribute('name', 'viewport'); meta.setAttribute('content', 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'); document.getElementsByTagName('head')[0].appendChild(meta);";
    WKUserScript *wkUScript = [[WKUserScript alloc] initWithSource:jScript injectionTime:WKUserScriptInjectionTimeAtDocumentEnd forMainFrameOnly:YES];
    [[[self webView] configuration].userContentController addUserScript:wkUScript];
    
    if (self.url) {
        [[self webView] loadRequest:[[NSURLRequest alloc] initWithURL:self.url]];
    }
    
    // Add webView BEHIND the blur view
    [[self view] insertSubview:[self webView] belowSubview:[self blurView]];
    [[self webView] setTranslatesAutoresizingMaskIntoConstraints:NO];
    
    // Correct constraints - webView starts below the navigation bar
    [[[[self webView] topAnchor] constraintEqualToAnchor:[[self blurView] bottomAnchor]] setActive:YES];
    [[[[self webView] leadingAnchor] constraintEqualToAnchor:[[self view] leadingAnchor]] setActive:YES];
    [[[[self webView] trailingAnchor] constraintEqualToAnchor:[[self view] trailingAnchor]] setActive:YES];
    [[[[self webView] bottomAnchor] constraintEqualToAnchor:[[self view] bottomAnchor]] setActive:YES];
    
    [[self webView] scrollView].contentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentNever;
}

// MARK: - KVO

- (void)observeValueForKeyPath:(NSString *)keyPath
                      ofObject:(id)object
                        change:(NSDictionary<NSKeyValueChangeKey,id> *)change
                       context:(void *)context {
    
    if ([keyPath isEqualToString:@"estimatedProgress"]) {
        float progress = [[self webView] estimatedProgress];
        [[self progressView] setProgress:progress animated:YES];
        
        if (progress > 0.0 && [[self progressView] alpha] == 0.0) {
            [[self progressView] setAlpha:1.0];
        }
        
        if (progress >= 1.0) {
            [UIView animateWithDuration:0.3 delay:0.3 options:UIViewAnimationOptionCurveEaseOut animations:^{
                [[self progressView] setAlpha:0.0];
            } completion:^(BOOL finished) {
                [[self progressView] setProgress:0.0 animated:NO];
            }];
        }
    } else if ([keyPath isEqualToString:@"title"]) {
        NSString *title = [[self webView] title];
        if (title && title.length > 0) {
            [[self titleLabel] setText:title];
        }
    }
}

// MARK: - Helper Methods for Navigation

- (BOOL)isSetupedCustomHeader:(NSURLRequest *)targetRequest {
    for (NSString *key in [self.customRequestHeader allKeys]) {
        if (![[[targetRequest allHTTPHeaderFields] objectForKey:key] isEqualToString:[self.customRequestHeader objectForKey:key]]) {
            return NO;
        }
    }
    return YES;
}

- (NSURLRequest *)constructionCustomHeader:(NSURLRequest *)originalRequest {
    NSMutableURLRequest *convertedRequest = originalRequest.mutableCopy;
    for (NSString *key in [self.customRequestHeader allKeys]) {
        [convertedRequest setValue:self.customRequestHeader[key] forHTTPHeaderField:key];
    }
    return (NSURLRequest *)[convertedRequest copy];
}

// MARK: - WKNavigationDelegate

- (void)webView:(WKWebView *)webView didStartProvisionalNavigation:(WKNavigation *)navigation {
    [[self progressView] setAlpha:1.0];
    [[self progressView] setProgress:0.1 animated:YES];
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    // Content inset adjustment removed since webView now starts below navigation bar
}

- (void)webView:(WKWebView *)wkWebView
decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction
decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler {
    
    NSURL *nsurl = [navigationAction.request URL];
    NSString *url = [nsurl absoluteString];
    NSString *path = nsurl.path;
    
    if ([path isEqualToString:@"/checkout-completed-successfully"]) {
        NSLog(@"Checkout completed successfully - closing webview");
        decisionHandler(WKNavigationActionPolicyCancel);
        [self dismissViewControllerAnimated:YES completion:^{
            UnitySendMessage("Elephant", "OnWebViewClosed", "checkout-completed-successfully");
        }];
        return;
    }
    
    if ([url rangeOfString:@"//itunes.apple.com/"].location != NSNotFound) {
        if (@available(iOS 10.0, *)) {
            [[UIApplication sharedApplication] openURL:nsurl options:@{} completionHandler:nil];
        } else {
            [[UIApplication sharedApplication] openURL:nsurl];
        }
        decisionHandler(WKNavigationActionPolicyCancel);
        return;
    } else if (![url hasPrefix:@"about:blank"]  // for loadHTML()
               && ![url hasPrefix:@"about:srcdoc"] // for iframe srcdoc attribute
               && ![url hasPrefix:@"file:"]
               && ![url hasPrefix:@"http:"]
               && ![url hasPrefix:@"https:"]) {
        if([[UIApplication sharedApplication] canOpenURL:nsurl]) {
            if (@available(iOS 10.0, *)) {
                [[UIApplication sharedApplication] openURL:nsurl options:@{} completionHandler:nil];
            } else {
                [[UIApplication sharedApplication] openURL:nsurl];
            }
        }
        decisionHandler(WKNavigationActionPolicyCancel);
        return;
    } else if (navigationAction.navigationType == WKNavigationTypeLinkActivated
               && (!navigationAction.targetFrame || !navigationAction.targetFrame.isMainFrame)) {
        // cf. for target="_blank", cf. http://qiita.com/ShingoFukuyama/items/b3a1441025a36ab7659c
        [wkWebView loadRequest:navigationAction.request];
        decisionHandler(WKNavigationActionPolicyCancel);
        return;
    }
    
    decisionHandler(WKNavigationActionPolicyAllow);
}

- (void)webView:(WKWebView *)webView
decidePolicyForNavigationResponse:(WKNavigationResponse *)navigationResponse
decisionHandler:(void (^)(WKNavigationResponsePolicy))decisionHandler {
    
    if ([navigationResponse.response isKindOfClass:[NSHTTPURLResponse class]]) {
        NSHTTPURLResponse *response = (NSHTTPURLResponse *)navigationResponse.response;
        
        if (response.statusCode >= 400) {
            NSLog(@"WebView HTTP Error: %ld", (long)response.statusCode);
        }
    }
    
    decisionHandler(WKNavigationResponsePolicyAllow);
}

- (void)webViewWebContentProcessDidTerminate:(WKWebView *)webView {
    NSLog(@"WebView process terminated - reloading");
    [webView reload];
}

- (WKWebView *)webView:(WKWebView *)webView
createWebViewWithConfiguration:(WKWebViewConfiguration *)configuration
   forNavigationAction:(WKNavigationAction *)navigationAction
        windowFeatures:(WKWindowFeatures *)windowFeatures {
    
    if (!navigationAction.targetFrame.isMainFrame) {
        [webView loadRequest:navigationAction.request];
    }
    return nil;
}

// MARK: - Configure

- (void)configureWithURL:(NSURL *)url {
    [self setUrl:url];
    if ([self webView]) {
        [[self webView] loadRequest:[[NSURLRequest alloc] initWithURL:url]];
    }
}

// MARK: - Actions

- (void)dismissButtonTapped:(UIButton *)sender {
    NSLog(@"Dismiss tapped");
    
    if (self.presentingViewController) {
        [self dismissViewControllerAnimated:YES completion:^{
            UnitySendMessage("Elephant", "OnWebViewClosed", "user_dismiss");
        }];
    } else if (self.navigationController) {
        [self.navigationController popViewControllerAnimated:YES];
        UnitySendMessage("Elephant", "OnWebViewClosed", "user_dismiss");
    } else {
        UnitySendMessage("Elephant", "OnWebViewClosed", "user_dismiss");
    }
}

- (void)doneButtonTapped:(UIButton *)sender {
    [self dismissViewControllerAnimated:YES completion:nil];
    UnitySendMessage("Elephant", "OnWebViewClosed", "user_dismiss");
}

@end