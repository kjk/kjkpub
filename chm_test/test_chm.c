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

#if 0
// https://code.google.com/p/address-sanitizer/wiki/ExampleUseAfterFree
int trigger_use_after_free(int i) {
    int *arr = (int *)malloc(sizeof(int) * 100);
    free(arr);
    return arr[i]; // BOOM
}

// TODO: doesn't seem to be detected by
// /Applications/Xcode-beta.app//Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang
// from XCode 7.0 b3
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
    int res = arr[i + 100]; // BOOM
    free(arr);
    return res;
}
#endif

static int _extract(struct chmFile *h, struct chmUnitInfo *ui) {
    int ui_path_len;
    char buffer[32768];

    if (ui->flags & CHM_ENUMERATE_DIRS) {
        return CHM_ENUMERATOR_CONTINUE;
    }

    if (ui->path[0] != '/') {
        return CHM_ENUMERATOR_CONTINUE;
    }

    /* quick hack for security hole mentioned by Sven Tantau */
    if (strstr(ui->path, "/../") != NULL) {
        return CHM_ENUMERATOR_CONTINUE;
    }

    /* Get the length of the path */
    ui_path_len = strlen(ui->path) - 1;

    /* Distinguish between files and dirs */
    if (ui->path[ui_path_len] == '/') {
        return CHM_ENUMERATOR_CONTINUE;
    }

    LONGINT64 len, remain = ui->length;
    LONGUINT64 offset = 0;

    printf(", extracting %s", ui->path);

    while (remain != 0) {
        len = chm_retrieve_object(h, ui, (unsigned char *)buffer, offset, 32768);
        if (len > 0) {
            offset += len;
            remain -= len;
        } else {
            fprintf(stderr, "incomplete file: %s\n", ui->path);
            return CHM_ENUMERATOR_FAILURE;
        }
    }

    return CHM_ENUMERATOR_CONTINUE;
}

static int _print_ui(struct chmFile *h, struct chmUnitInfo *ui, void *context) {
    char buf[32 * 1024] = { 0 };
    int res;

    if (ui->flags & CHM_ENUMERATE_NORMAL)
        strcpy(buf, "normal ");
    else if (ui->flags & CHM_ENUMERATE_SPECIAL)
        strcpy(buf, "special ");
    else if (ui->flags & CHM_ENUMERATE_META)
        strcpy(buf, "meta ");

    if (ui->flags & CHM_ENUMERATE_DIRS)
        strcat(buf, "dir");
    else if (ui->flags & CHM_ENUMERATE_FILES)
        strcat(buf, "file");

    printf("   %1d %8d %8d   %s\t\t%s", (int)ui->space, (int)ui->start, (int)ui->length, buf,
           ui->path);
    res = _extract(h, ui);
    printf("\n");
    return res;
}

int main(int argc, char **argv) {
    struct chmFile *h;
    char *filePath;

    // trigger_use_after_free(5);
    // trigger_stack_out_of_bounds(5);
    // trigger_heap_out_of_bounds(1);

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
