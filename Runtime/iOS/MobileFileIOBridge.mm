#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <UniformTypeIdentifiers/UniformTypeIdentifiers.h>

// Unity callback object name
static NSString* _callbackObjectName = nil;
static BOOL _allowMultipleSelection = NO;

// UIDocumentPickerViewController Delegate
@interface MobileFileIOPickerDelegate : NSObject <UIDocumentPickerDelegate>
@end

@implementation MobileFileIOPickerDelegate

static MobileFileIOPickerDelegate* _pickerDelegate = nil;

+ (MobileFileIOPickerDelegate*)sharedInstance {
    if (_pickerDelegate == nil) {
        _pickerDelegate = [[MobileFileIOPickerDelegate alloc] init];
    }
    return _pickerDelegate;
}

// iOS 14+
- (void)documentPicker:(UIDocumentPickerViewController *)controller
didPickDocumentsAtURLs:(NSArray<NSURL *> *)urls {

    NSMutableArray<NSString*>* copiedPaths = [[NSMutableArray alloc] init];

    for (NSURL* url in urls) {
        // Security scoped resource access
        BOOL accessed = [url startAccessingSecurityScopedResource];

        @try {
            // Copy to temp directory
            NSString* tempDir = NSTemporaryDirectory();
            NSString* fileName = [url lastPathComponent];
            NSString* destPath = [tempDir stringByAppendingPathComponent:fileName];

            // Remove if exists
            NSFileManager* fileManager = [NSFileManager defaultManager];
            if ([fileManager fileExistsAtPath:destPath]) {
                [fileManager removeItemAtPath:destPath error:nil];
            }

            NSError* error = nil;
            if ([fileManager copyItemAtURL:url toURL:[NSURL fileURLWithPath:destPath] error:&error]) {
                [copiedPaths addObject:destPath];
                NSLog(@"[MobileFileIO] Copied file to: %@", destPath);
            } else {
                NSLog(@"[MobileFileIO] Copy error: %@", error.localizedDescription);
            }
        }
        @finally {
            if (accessed) {
                [url stopAccessingSecurityScopedResource];
            }
        }
    }

    // Send callback to Unity
    if (_callbackObjectName != nil) {
        if (_allowMultipleSelection) {
            NSString* joinedPaths = [copiedPaths componentsJoinedByString:@"|"];
            UnitySendMessage([_callbackObjectName UTF8String],
                           "OnMultipleFilesPicked",
                           [joinedPaths UTF8String]);
        } else {
            NSString* path = copiedPaths.count > 0 ? copiedPaths[0] : @"";
            UnitySendMessage([_callbackObjectName UTF8String],
                           "OnFilePicked",
                           [path UTF8String]);
        }
    }

    [controller dismissViewControllerAnimated:YES completion:nil];
}

// Deprecated but needed for iOS 13 and below
- (void)documentPicker:(UIDocumentPickerViewController *)controller
didPickDocumentAtURL:(NSURL *)url {
    [self documentPicker:controller didPickDocumentsAtURLs:@[url]];
}

- (void)documentPickerWasCancelled:(UIDocumentPickerViewController *)controller {
    if (_callbackObjectName != nil) {
        UnitySendMessage([_callbackObjectName UTF8String],
                       "OnPickerCancelled",
                       "");
    }
    [controller dismissViewControllerAnimated:YES completion:nil];
}

@end

// Helper to get Unity's root view controller
extern UIViewController* UnityGetGLViewController(void);

extern "C" {

    void _MobileFileIO_PickFile(const char** utis, int utiCount, const char* callbackObject) {
        _callbackObjectName = [NSString stringWithUTF8String:callbackObject];
        _allowMultipleSelection = NO;

        dispatch_async(dispatch_get_main_queue(), ^{
            NSMutableArray<UTType*>* contentTypes = [[NSMutableArray alloc] init];

            for (int i = 0; i < utiCount; i++) {
                NSString* utiString = [NSString stringWithUTF8String:utis[i]];
                UTType* type = [UTType typeWithIdentifier:utiString];
                if (type != nil) {
                    [contentTypes addObject:type];
                }
            }

            // Fallback to data type if no valid types
            if (contentTypes.count == 0) {
                [contentTypes addObject:UTTypeData];
            }

            UIDocumentPickerViewController* picker;

            if (@available(iOS 14.0, *)) {
                picker = [[UIDocumentPickerViewController alloc]
                         initForOpeningContentTypes:contentTypes];
            } else {
                // Fallback for iOS 13 and below
                NSMutableArray<NSString*>* utiStrings = [[NSMutableArray alloc] init];
                for (int i = 0; i < utiCount; i++) {
                    [utiStrings addObject:[NSString stringWithUTF8String:utis[i]]];
                }
                picker = [[UIDocumentPickerViewController alloc]
                         initWithDocumentTypes:utiStrings
                         inMode:UIDocumentPickerModeImport];
            }

            picker.delegate = [MobileFileIOPickerDelegate sharedInstance];
            picker.allowsMultipleSelection = NO;

            if (@available(iOS 13.0, *)) {
                picker.shouldShowFileExtensions = YES;
            }

            UIViewController* rootVC = UnityGetGLViewController();
            [rootVC presentViewController:picker animated:YES completion:nil];
        });
    }

    void _MobileFileIO_PickMultipleFiles(const char** utis, int utiCount, const char* callbackObject) {
        _callbackObjectName = [NSString stringWithUTF8String:callbackObject];
        _allowMultipleSelection = YES;

        dispatch_async(dispatch_get_main_queue(), ^{
            NSMutableArray<UTType*>* contentTypes = [[NSMutableArray alloc] init];

            for (int i = 0; i < utiCount; i++) {
                NSString* utiString = [NSString stringWithUTF8String:utis[i]];
                UTType* type = [UTType typeWithIdentifier:utiString];
                if (type != nil) {
                    [contentTypes addObject:type];
                }
            }

            if (contentTypes.count == 0) {
                [contentTypes addObject:UTTypeData];
            }

            UIDocumentPickerViewController* picker;

            if (@available(iOS 14.0, *)) {
                picker = [[UIDocumentPickerViewController alloc]
                         initForOpeningContentTypes:contentTypes];
            } else {
                NSMutableArray<NSString*>* utiStrings = [[NSMutableArray alloc] init];
                for (int i = 0; i < utiCount; i++) {
                    [utiStrings addObject:[NSString stringWithUTF8String:utis[i]]];
                }
                picker = [[UIDocumentPickerViewController alloc]
                         initWithDocumentTypes:utiStrings
                         inMode:UIDocumentPickerModeImport];
            }

            picker.delegate = [MobileFileIOPickerDelegate sharedInstance];

            if (@available(iOS 11.0, *)) {
                picker.allowsMultipleSelection = YES;
            }

            if (@available(iOS 13.0, *)) {
                picker.shouldShowFileExtensions = YES;
            }

            UIViewController* rootVC = UnityGetGLViewController();
            [rootVC presentViewController:picker animated:YES completion:nil];
        });
    }
}
