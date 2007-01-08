#!/bin/sh
./regenerate-makefiles
cd ../../JavaScriptCore
make clean
make
cd ../WebCore/Projects/gdk
make clean
make .DerivedSources
make
cd ../../../WebKitTools/GdkLauncher
make clean
make
