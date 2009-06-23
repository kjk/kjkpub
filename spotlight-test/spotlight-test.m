#import <Foundation/Foundation.h>

int main (int argc, const char * argv[]) {
    MDQueryRef mdQuery;
    NSAutoreleasePool * pool = [[NSAutoreleasePool alloc] init];

    //mdQuery = MDQueryCreate(kCFAllocatorDefault,  CFSTR("kMDItemKeywords == '*.m'"), NULL, NULL);
    //mdQuery = MDQueryCreate(kCFAllocatorDefault,  CFSTR("kMDItemDisplayName == \"*.m\""), NULL, NULL);
    mdQuery = MDQueryCreate(kCFAllocatorDefault,  CFSTR("kMDItemDisplayName == \"osxctrls.m\""), NULL, NULL);

    if (MDQueryExecute(mdQuery, kMDQuerySynchronous)) {
        CFIndex count = MDQueryGetResultCount(mdQuery);
        for (CFIndex i = 0; i < count; i++) {
            MDItemRef item = (MDItemRef)MDQueryGetResultAtIndex(mdQuery, i);
            CFStringRef displayName = (CFStringRef)MDItemCopyAttribute(item, CFSTR("kMDItemDisplayName"));
            CFStringRef path = (CFStringRef)MDItemCopyAttribute(item, CFSTR("kMDItemPath"));
            NSLog(@"Result %d, display=%@, path=%@", i, (NSString*)displayName, (NSString*)path);
        }
    } else {
        NSLog(@"Failed execute");
    }
    [pool drain];
    return 0;
}
