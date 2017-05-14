package main

import (
	"bufio"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"os/exec"
	"os/user"
	"path/filepath"
	"strings"
)

func fatalIfErr(err error) {
	if err != nil {
		panic(err.Error())
	}
}

// userHomeDir returns $HOME diretory of the user
func userHomeDir() string {
	// user.Current() returns nil if cross-compiled e.g. on mac for linux
	if usr, _ := user.Current(); usr != nil {
		return usr.HomeDir
	}
	return os.Getenv("HOME")
}

// expandTildeInPath converts ~ to $HOME
func expandTildeInPath(s string) string {
	if strings.HasPrefix(s, "~/") {
		return userHomeDir() + s[1:]
	}
	return s
}

func dirExists(dir string) bool {
	s, err := os.Stat(dir)
	if err != nil {
		return false
	}
	return s.Mode().IsDir()
}

func runCmdMust(name string, args ...string) {
	cmd := exec.Command(name, args...)
	err := cmd.Run()
	fatalIfErr(err)
}

func readLinesMust(path string) []string {
	f, err := os.Open(path)
	fatalIfErr(err)
	defer f.Close()
	scanner := bufio.NewScanner(f)
	var lines []string
	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}
	fatalIfErr(scanner.Err())
	return lines
}

// remove #include "foo.h" lines and #pragma once lines
func filterLines(a []string) []string {
	var res []string
	for _, s := range a {
		if strings.HasPrefix(s, "#pragma once") {
			continue
		}
		if strings.HasPrefix(s, `#include "`) {
			continue
		}
		res = append(res, s)
	}
	return res
}

func readFilteredLinesMust(path string) []string {
	lines := readLinesMust(path)
	nLines := len(lines)
	lines = filterLines(lines)
	fmt.Printf("%s: %d lines, %d filtered lines\n", path, nLines, len(lines))
	return lines
}

func writeLinesMust(dstPath string, args ...interface{}) {
	var lines []string
	for _, arg := range args {
		a := arg.([]string)
		lines = append(lines, a...)
	}
	s := strings.Join(lines, "\n")
	err := ioutil.WriteFile(dstPath, []byte(s), 0644)
	fatalIfErr(err)
	fmt.Printf("Wrote %s (%d lines)\n", dstPath, len(lines))
}

func main() {
	dir := expandTildeInPath("~/src/yoga")
	if !dirExists(dir) {
		log.Fatalf("dir '%s' doesn't exist\n", dir)
	}
	runCmdMust("git", "pull")
	srcDir := filepath.Join(dir, "yoga")

	{
		lines1 := readFilteredLinesMust(filepath.Join(srcDir, "YGMacros.h"))
		lines2 := readFilteredLinesMust(filepath.Join(srcDir, "YGEnums.h"))
		lines3 := readFilteredLinesMust(filepath.Join(srcDir, "Yoga.h"))
		lines4 := readFilteredLinesMust(filepath.Join(srcDir, "YGNodeList.h"))
		writeLinesMust("yoga.h", lines1, lines2, lines3, lines4)
	}

	{
		lines1 := readFilteredLinesMust(filepath.Join(srcDir, "YGEnums.c"))
		lines2 := readFilteredLinesMust(filepath.Join(srcDir, "YGNodeList.c"))
		lines3 := readFilteredLinesMust(filepath.Join(srcDir, "Yoga.c"))
		lines := []string{`#include "yoga.h"`}
		writeLinesMust("yoga.c", lines, lines1, lines2, lines3)
	}

}
