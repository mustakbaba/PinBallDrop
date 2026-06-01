#import <Foundation/Foundation.h>

extern void UnitySendMessage(const char* className, const char* methodName, const char* param);

extern "C" {

static const char* GetCString(NSString* _Nullable string) {
    if (!string) {
        return "unknown";
    }
    return [string UTF8String];
}

void RequestDeclaredAgeRange() {
    Class controllerClass = NSClassFromString(@"DeclaredAgeRangeController");
    if (!controllerClass) {
        UnitySendMessage("Elephant", "OnAgeRangeResult", "{\"error\":\"DeclaredAgeRangeController class not found\"}");
        return;
    }
    
    SEL sharedSel = NSSelectorFromString(@"sharedInstance");
    if (![controllerClass respondsToSelector:sharedSel]) {
        UnitySendMessage("Elephant", "OnAgeRangeResult", "{\"error\":\"sharedInstance method not found\"}");
        return;
    }
    
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    id controller = [controllerClass performSelector:sharedSel];
#pragma clang diagnostic pop
    
    if (!controller) {
        UnitySendMessage("Elephant", "OnAgeRangeResult", "{\"error\":\"Failed to get shared instance\"}");
        return;
    }
    
    SEL requestSel = NSSelectorFromString(@"requestAgeRangeWithCompletion:");
    if (![controller respondsToSelector:requestSel]) {
        UnitySendMessage("Elephant", "OnAgeRangeResult", "{\"error\":\"requestAgeRangeWithCompletion: not found\"}");
        return;
    }
    
    void (^completionBlock)(NSString*, NSString*) = ^(NSString* _Nullable ageRange, NSString* _Nullable error) {
        NSString* jsonString = nil;
        
        if (error) {
            NSMutableDictionary* errorResult = [NSMutableDictionary dictionary];
            [errorResult setObject:error forKey:@"error"];
            NSError* jsonError = nil;
            NSData* jsonData = [NSJSONSerialization dataWithJSONObject:errorResult options:0 error:&jsonError];
            jsonString = jsonData ? [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding]
            : @"{\"error\":\"Failed to serialize error\"}";
        } else if (ageRange) {
            jsonString = ageRange;
        } else {
            jsonString = @"{\"error\":\"Unknown error: no data received\"}";
        }
        
        UnitySendMessage("Elephant", "OnAgeRangeResult", [jsonString UTF8String]);
    };
    
    NSMethodSignature* signature = [controller methodSignatureForSelector:requestSel];
    NSInvocation* invocation = [NSInvocation invocationWithMethodSignature:signature];
    [invocation setTarget:controller];
    [invocation setSelector:requestSel];
    [invocation setArgument:&completionBlock atIndex:2];
    [invocation retainArguments];
    [invocation invoke];
}

bool IsDeclaredAgeRangeAvailable() {
    Class controllerClass = NSClassFromString(@"DeclaredAgeRangeController");
    if (!controllerClass) {
        return false;
    }
    
    SEL sharedSel = NSSelectorFromString(@"sharedInstance");
    if (![controllerClass respondsToSelector:sharedSel]) {
        return false;
    }
    
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    id controller = [controllerClass performSelector:sharedSel];
#pragma clang diagnostic pop
    
    if (!controller) {
        return false;
    }
    
    SEL isAvailableSel = NSSelectorFromString(@"isAvailableSync");
    if (![controller respondsToSelector:isAvailableSel]) {
        return false;
    }
    
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Warc-performSelector-leaks"
    BOOL result = NO;
    NSMethodSignature* sig = [controller methodSignatureForSelector:isAvailableSel];
    NSInvocation* inv = [NSInvocation invocationWithMethodSignature:sig];
    [inv setTarget:controller];
    [inv setSelector:isAvailableSel];
    [inv invoke];
    [inv getReturnValue:&result];
#pragma clang diagnostic pop
    
    return result;
}

const char* GetDeclaredAgeRangeStatus() {
    return IsDeclaredAgeRangeAvailable() ? "available" : "unavailable";
}
}
