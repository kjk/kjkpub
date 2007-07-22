#!/bin/env python
"""Parses the output of 'readelf -e' and displays it in more human-readable form.
   If given 2 file names as arguments, also displays a diff of section sizes
   between the two"""

import os, sys, re

def usage_and_exit():
    print "usage readelfcmp.py file1 [file2]"
    sys.exit(1)

def err_file_not_exists(file_name):
    print "file '%s' doesn't exists" % file_name
    sys.exit(1)

def ensure_file_exists(file_name):
    if not os.path.exists(file_name):
        err_file_not_exists(file_name)

class SectionInfo:
    def __init__(self, number, name, size):
        self.number = number
        self.name = name
        self.size = size
    def unique_name(self):
        # it's important they sort by section number naturally, so we put number as integer
        # at the beginning
        return "%02d-%s" % (self.number, self.name) # assumes no more than 99 sections

def parse_readelf_output(file_name):
    ensure_file_exists(file_name)
    in_section_headers = False
    section_line_no = 0
    sections_info = {}
    for line in file(file_name):
        if not in_section_headers:
            if line.startswith("Section Headers:"):
                in_section_headers = True
            continue
        section_line_no = section_line_no + 1
        if section_line_no in (1,2):
            continue # skip first two lines as they contain header and some crap
        items = line.split()
        if not items[0].startswith("["):
            break
        no_txt = items[0]
        if no_txt == "[":
            items = items[1:]
            no_txt = items[0][:1]
        else:
            no_txt = no_txt[1:-1]
        assert len(items) in (10,11)  # 10 when flags are missing
        section_number = int(no_txt)
        assert section_number+2 == section_line_no
        section_name = items[1]
        section_size_hex_txt = items[5]
        section_size = int(section_size_hex_txt, 16)
        si = SectionInfo(section_number, section_name, section_size)
        sections_info[si.unique_name()] = si
    return sections_info

def number_readable(num):
    # TODO: clearly
    return "%d" % num

def print_elf_data(sections_info):
    max_section_name_len = max([len(section_info.name) for section_info in sections_info.values()])
    max_size_len = max([len(number_readable(section_info.size)) for section_info in sections_info.values()])
    #print "max_section_name_len: %d" % max_section_name_len
    #print "max_size_len:         %d" % max_size_len
    keys = sections_info.keys()
    keys.sort()
    for key in keys:
        section_info = sections_info[key]
        name = section_info.name.rjust(max_section_name_len)
        size_txt = number_readable(section_info.size).rjust(max_size_len)
        print "%2d %s %s" % (section_info.number, name, size_txt)

def print_cmp_elf_data(sections_info_1, sections_info_2):
    keys1 = sections_info_1.keys()
    keys1.sort()
    keys2 = sections_info_2.keys()
    keys2.sort()
    if keys1 != keys2:
        print_elf_data(sections_info_1)
        print_elf_data(sections_info_2)
        print "Error: names of sections don't match - those don't appear to be the same executables"
        sys.exit(1)
    max_section_name_len = max([len(section_info.name) for section_info in sections_info_1.values()])
    max_size_1_len = max([len(number_readable(section_info.size)) for section_info in sections_info_1.values()])
    max_size_2_len = max([len(number_readable(section_info.size)) for section_info in sections_info_2.values()])
    total_size_delta = 0
    for key in keys1:
        si1 = sections_info_1[key]
        si2 = sections_info_2[key]
        name = si1.name.rjust(max_section_name_len)
        size1_txt = number_readable(si1.size).rjust(max_size_1_len)
        size2_txt = number_readable(si2.size).rjust(max_size_2_len)
        size_delta = si2.size - si1.size
        total_size_delta += size_delta
        print "%2d %s %s %s %7d" % (si1.number, name, size1_txt, size2_txt, size_delta)
    print "total delta: %d" % total_size_delta

def main():
    if len(sys.argv) not in (2,3):
        usage_and_exit()
    file_name_1 = sys.argv[1]
    elf_data_1 = parse_readelf_output(file_name_1)
    if 2 == len(sys.argv):
        print_elf_data(elf_data_1)
        return
    file_name_2 = sys.argv[2]
    elf_data_2 = parse_readelf_output(file_name_2)
    print_cmp_elf_data(elf_data_1, elf_data_2)

if __name__=="__main__":
    main()
