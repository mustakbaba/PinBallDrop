#import <WebKit/WebKit.h>
#import <UIKit/UIKit.h>

@interface WebViewController : UIViewController <UIWebViewDelegate, WKNavigationDelegate, WKUIDelegate>

// MARK: - Properties

@property(nonatomic, strong) UIView *navigationBarView;
@property(nonatomic, strong) UIButton *doneButton;
@property(nonatomic, strong) UIButton *dismissButton;
@property(nonatomic, strong) WKWebView *webView;
@property(nonatomic, strong) NSURL *url;

// Navigation improvements
@property(nonatomic, strong) NSMutableDictionary *customRequestHeader;
@property(nonatomic) BOOL googleAppRedirectionEnabled;

// MARK: - Setup

- (void)setupNavigationBar;
- (void)setupDoneButton;
- (void)setupDismissButton;
- (void)setupWebView;

// MARK: - Configure

- (void)configureWithURL:(NSURL *)url;
- (void)doneButtonTapped:(UIButton *)sender;

@end