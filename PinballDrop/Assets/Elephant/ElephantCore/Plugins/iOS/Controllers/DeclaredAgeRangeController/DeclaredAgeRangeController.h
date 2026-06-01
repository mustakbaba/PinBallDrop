#import <Foundation/Foundation.h>

@class DeclaredAgeRangeController;

NS_ASSUME_NONNULL_BEGIN

@protocol DeclaredAgeRangeControllerProtocol <NSObject>

+ (instancetype)sharedInstance;
- (void)requestAgeRangeWithCompletion:(void (^)(NSString * _Nullable ageRange, NSString * _Nullable error))completion;
- (BOOL)isAvailableSync;

@end

NS_ASSUME_NONNULL_END
