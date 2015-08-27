/***************************************************************************
 *   author:     Jed Wing <jedwin@ugcs.caltech.edu>                        *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU Lesser General Public License as        *
 *   published by the Free Software Foundation; either version 2.1 of the  *
 *   License, or (at your option) any later version.                       *
 *                                                                         *
 ***************************************************************************/

#include "chm_lib.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/types.h>

int _print_ui(struct chmFile *h, struct chmUnitInfo *ui, void *context) {
    static char szBuf[128];
    memset(szBuf, 0, 128);
    if (ui->flags & CHM_ENUMERATE_NORMAL)
        strcpy(szBuf, "normal ");
    else if (ui->flags & CHM_ENUMERATE_SPECIAL)
        strcpy(szBuf, "special ");
    else if (ui->flags & CHM_ENUMERATE_META)
        strcpy(szBuf, "meta ");

    if (ui->flags & CHM_ENUMERATE_DIRS)
        strcat(szBuf, "dir");
    else if (ui->flags & CHM_ENUMERATE_FILES)
        strcat(szBuf, "file");

    printf("   %1d %8d %8d   %s\t\t%s\n", (int)ui->space, (int)ui->start, (int)ui->length, szBuf,
           ui->path);
    return CHM_ENUMERATOR_CONTINUE;
}

// https://code.google.com/p/address-sanitizer/wiki/ExampleUseAfterFree
int trigger_use_after_free(int i) {
    int *arr = (int *)malloc(sizeof(int) * 100);
    free(arr);
    return arr[i]; // BOOM
}

// TODO: doesn't seem to be detected by /Applications/Xcode-beta.app//Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang from XCode 7.0 b3
// https://code.google.com/p/address-sanitizer/wiki/ExampleStackOutOfBounds
int trigger_stack_out_of_bounds(int i) {
    int stack_array[100];
    stack_array[1] = 0;
    return stack_array[i + 100]; // BOOM
}

// https://code.google.com/p/address-sanitizer/wiki/ExampleHeapOutOfBounds
int trigger_heap_out_of_bounds(int i) {
    int *arr = (int *)malloc(sizeof(int) * 100);
    arr[0] = 0;
    int res = arr[i + 100];  // BOOM
    free(arr);
    return res;
}

int main(int argc, char **argv) {
    struct chmFile *h;
    char *filePath;

    //trigger_use_after_free(5);
    //trigger_stack_out_of_bounds(5);
    //trigger_heap_out_of_bounds(1);

    if (argc != 2) {
        fprintf(stderr, "usage: %s <chmfile>\n", argv[0]);
        exit(1);
    }

    filePath = argv[1];
    h = chm_open(filePath);
    if (h == NULL) {
        fprintf(stderr, "failed to open %s\n", filePath);
        exit(1);
    }

    printf("%s:\n", filePath);
    int res = chm_enumerate(h, CHM_ENUMERATE_ALL, _print_ui, NULL);

    if (CHM_ENUMERATOR_FAILURE == res) {
        printf("   *** ERROR *** %d: \n", res);
    }

    chm_close(h);
    return 0;
}
