#import <Foundation/Foundation.h>
#import <Security/Security.h>

BOOL keyExistsInKeyChain(const char* key) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSString *bundleID = [[NSBundle mainBundle] bundleIdentifier];
    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: bundleID,
        (__bridge id)kSecAttrAccount: nsKey,
        (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne,
        (__bridge id)kSecReturnAttributes: @YES
    };

    CFDictionaryRef attributes = NULL;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, (CFTypeRef *)&attributes);

    if (status == errSecSuccess) {
        if (attributes) CFRelease(attributes);
        return YES;
    }
    return NO;
}

const char* getValueForKey(const char* key) {
    NSString *bundleID = [[NSBundle mainBundle] bundleIdentifier];
    if (keyExistsInKeyChain(key)) {
        NSString *nsKey = [NSString stringWithUTF8String:key];
        NSDictionary *query = @{
            (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
            (__bridge id)kSecAttrService: bundleID,
            (__bridge id)kSecAttrAccount: nsKey,
            (__bridge id)kSecReturnData: @YES,
            (__bridge id)kSecMatchLimit: (__bridge id)kSecMatchLimitOne,
        };

        CFDataRef result = NULL;
        OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, (CFTypeRef *)&result);

        if (status == errSecSuccess) {
            NSString *value = [[NSString alloc] initWithData:(__bridge_transfer NSData *)result encoding:NSUTF8StringEncoding];
            return strdup([value UTF8String]);
        }
    }

    return NULL;
}

void saveValueForKey(const char* key, const char* value) {
    NSString *nsKey = [NSString stringWithUTF8String:key];
    NSString *nsValue = [NSString stringWithUTF8String:value];
    NSData *valueData = [nsValue dataUsingEncoding:NSUTF8StringEncoding];
    NSString *bundleID = [[NSBundle mainBundle] bundleIdentifier];

    NSDictionary *query = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: bundleID,
        (__bridge id)kSecAttrAccount: nsKey,
    };

    SecItemDelete((__bridge CFDictionaryRef)query);

    NSDictionary *attributes = @{
        (__bridge id)kSecClass: (__bridge id)kSecClassGenericPassword,
        (__bridge id)kSecAttrService: bundleID,
        (__bridge id)kSecAttrAccount: nsKey,
        (__bridge id)kSecValueData: valueData,
    };

    SecItemAdd((__bridge CFDictionaryRef)attributes, NULL);
}
