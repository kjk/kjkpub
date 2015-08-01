package main

import (
	"fmt"
	"io/ioutil"
	"path/filepath"
	"strings"
)

type Dir struct {
	Name  string
	Files []string
}

func panicIfErr(err error) {
	if err != nil {
		panic(err.Error())
	}
}

func isSrcFile(file string) bool {
	ext := filepath.Ext(file)
	for _, s := range []string{".c", ".cpp", ".asm", ".cxx"} {
		if strings.EqualFold(ext, s) {
			return true
		}
	}
	return false
}

func findFilesInDirRecur(dir string) []*Dir {
	var res []*Dir
	dirsToVisit := []string{dir}
	for len(dirsToVisit) > 0 {
		dir = dirsToVisit[0]
		dirsToVisit = dirsToVisit[1:]

		var srcFiles []string
		items, err := ioutil.ReadDir(dir)
		panicIfErr(err)
		for _, fi := range items {
			mode := fi.Mode()
			name := fi.Name()
			if mode.IsDir() {
				dirsToVisit = append(dirsToVisit, filepath.Join(dir, name))
			} else if mode.IsRegular() {
				if isSrcFile(name) {
					srcFiles = append(srcFiles, name)
				}
			}
		}
		if len(srcFiles) > 0 {
			d := &Dir{
				Name:  dir,
				Files: srcFiles,
			}
			res = append(res, d)
		}
	}
	return res
}

func dumpDir(dir *Dir) {
	fmt.Printf("%s\n", dir.Name)
	for _, f := range dir.Files {
		fmt.Printf("  %s\n", f)
	}
}

func dumpDirCompact(dir *Dir) {
	fmt.Printf("%s (%d)", dir.Name, len(dir.Files))
	for _, f := range dir.Files {
		fmt.Printf(" %s", f)
	}
	fmt.Println()
}

func dumpDirs(dirs []*Dir) {
	for _, dir := range dirs {
		if false {
			dumpDir(dir)
		} else {
			dumpDirCompact(dir)
		}
	}
}

func main() {
	dirsToVisit := []string{"src", "mupdf", "ext"}
	for _, dir := range dirsToVisit {
		dirs := findFilesInDirRecur(dir)
		dumpDirs(dirs)
	}
}
