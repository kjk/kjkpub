//
//  ButtonTestAppDelegate.h
//  ButtonTest
//
//  Created by Krzysztof Kowalczyk on 3/9/10.
//  Copyright 2010 OpenDNS. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@interface ButtonTestAppDelegate : NSObject <NSApplicationDelegate> {
    NSWindow *window;
}

@property (assign) IBOutlet NSWindow *window;

@end
