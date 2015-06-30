package main

import (
	"bufio"
	"bytes"
	"errors"
	"fmt"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"strings"

	"github.com/kr/pretty"
)

var (
	errDirectoryNotUnderScm = errors.New("directory not in git repository")
	errUnexpectedStatusLine = errors.New("unexpected line in git status output")
)

func isDir(path string) bool {
	s, err := os.Stat(path)
	return err == nil && s.IsDir()
}

func isDirScmRoot(path string) bool {
	if isDir(filepath.Join(path, ".git")) {
		return true
	}
	if isDir(filepath.Join(path, ".GIT")) {
		return true
	}
	return false
}

func findScmRoot() (string, error) {
	d, err := os.Getwd()
	if err != nil {
		return "", err
	}
	for {
		fmt.Printf("current dir: %s\n", d)
		isScm := isDirScmRoot(d)
		if isScm {
			return d, nil
		}
		nd := filepath.Dir(d)
		if d == nd {
			return "", errDirectoryNotUnderScm
		}
		d = nd
	}
}

// note: it's important that Removed and Added are > than Modified
const (
	NotVersioned = 1
	Modified     = 2
	Deleted      = 3
	Added        = 4
)

type GitItem struct {
	FullPath     string
	RelativePath string
	Type         int
}

func bytesToLines(d []byte) []string {
	r := bytes.NewBuffer(d)
	scanner := bufio.NewScanner(r)
	var lines []string
	for scanner.Scan() {
		line := scanner.Text()
		lines = append(lines, line)
	}
	// note: not checking for scanner.Err() because it shouldn't fail
	return lines
}

/*
Parse:
?? ../scdiff/bin/
A  main.go
AM main.go
D  main.go
*/
func parseGitStatusOutputLine(s string) (GitItem, error) {
	var res GitItem
	parts := strings.SplitN(s, " ", 2)
	if len(parts) != 2 {
		return res, errUnexpectedStatusLine
	}
	typeStr := parts[0]
	res.RelativePath = parts[1]
	if false && strings.HasSuffix(parts[1], "main.go") {
		fmt.Printf("s: '%s', parts[0]: '%s', parts[1]: '%s'\n", s, parts[0], parts[1])
	}
	if typeStr == "??" {
		res.Type = NotVersioned
		return res, nil
	}
	// a file can be both A(dded) and M(odified). In such case we pick only one: A(dded)
	for i := 0; i < len(typeStr); i++ {
		switch typeStr[i] {
		case 'M':
			if Modified > res.Type {
				res.Type = Modified
			}
		case 'A':
			if Added > res.Type {
				res.Type = Added
			}
		case 'D':
			if Deleted > res.Type {
				res.Type = Deleted
			}
		default:
			log.Fatalf("invalid git status line: '%s'\n", s) // TODO: for now
			return res, errUnexpectedStatusLine
		}
	}
	return res, nil
}

func parseGitStatusOutput(d []byte) ([]*GitItem, error) {
	lines := bytesToLines(d)
	var res []*GitItem
	for _, l := range lines {
		gi, err := parseGitStatusOutputLine(l)
		if err != nil {
			return nil, err
		}
		fmt.Printf("%# v\n", pretty.Formatter(&gi))
		res = append(res, &gi)
	}
	return res, nil
}

func gitStatus(rootDir string) ([]*GitItem, error) {
	cmd := exec.Command("git", "status", "-s")
	cmd.Dir = rootDir
	out, err := cmd.CombinedOutput()
	if err != nil {
		return nil, err
	}
	return parseGitStatusOutput(out)
}

func main() {
	d, err := findScmRoot()
	if err != nil {
		fmt.Fprintf(os.Stderr, "findScmRoot() failed with %s\n", err)
		os.Exit(1)
	}
	fmt.Printf("dir: %s\n", d)
	gitStatus(d)
}
