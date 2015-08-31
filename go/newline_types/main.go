package main

import (
	"fmt"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

// Unix, new mac: lf
// Old mac: cr
// Windows: cr lf
var (
	cr             byte = 0xd // \r, 13
	lf             byte = 0xa // \n, 10
	textFileExtArr      = []string{
		".txt", ".cs", ".cpp", ".cc", ".h", ".md", ".go", ".html", ".xml", ".csproj",
		".sln", ".bat", ".sh", ".py", ".lua", ".asm", ".json", ".js", ".m", ".mm",
		".pl",
	}
	textFileExtMap = make(map[string]bool)
	results        []Result
)

// NewLineCounts counts number of newlines
type NewLineCounts struct {
	CrCount   int
	LfCount   int
	CrLfCount int
}

// Result has results of scanning one file
type Result struct {
	Path   string
	Counts NewLineCounts
}

// Mixed returns true if there are different kinds of newlines
func (c *NewLineCounts) Mixed() bool {
	n := 0
	if c.CrCount > 0 {
		n++
	}
	if c.LfCount > 0 {
		n++
	}
	if c.CrLfCount > 0 {
		n++
	}
	return n > 1
}

func init() {
	for _, ext := range textFileExtArr {
		textFileExtMap[ext] = true
	}
}

func countNewlines(d []byte) NewLineCounts {
	res := NewLineCounts{}
	n := len(d)
	for i := 0; i < n; i++ {
		c := d[i]
		if c == lf {
			res.LfCount++
			continue
		}
		if c != cr {
			continue
		}
		// it's cr, see if just cr or cr lf
		if i == n-1 {
			res.CrCount++
			continue
		}
		if d[i+1] == lf {
			res.CrLfCount++
			i++
		} else {
			res.CrCount++
		}
	}
	return res
}

func isTextFile(path string) bool {
	path = strings.ToLower(path)
	return textFileExtMap[path]
}

func main() {
	err := filepath.Walk(".", func(path string, fi os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if !fi.Mode().IsRegular() {
			return nil
		}
		ext := filepath.Ext(path)
		if !isTextFile(ext) {
			return nil
		}
		d, err := ioutil.ReadFile(path)
		if err != nil {
			return err
		}
		counts := countNewlines(d)
		res := Result{
			Path:   path,
			Counts: counts,
		}
		results = append(results, res)
		return nil
	})
	if err != nil {
		fmt.Printf("filepath.Walk() failed with %s\n", err)
		return
	}
	var nCr, nLf, nCrLf int
	var mixed []Result
	for _, r := range results {
		c := r.Counts
		if c.Mixed() {
			mixed = append(mixed, r)
		} else if c.CrCount > 0 {
			nCr++
		} else if c.LfCount > 0 {
			nLf++
		} else if c.CrLfCount > 0 {
			nCrLf++
		}
	}
	fmt.Printf("%d files\n", len(results))
	nMixed := len(mixed)
	if nMixed > 0 {
		fmt.Printf("%d mixed:\n", nMixed)
		for _, r := range mixed {
			fmt.Printf("  %s\n", r.Path)
		}
	}
	if nCr > 0 {
		fmt.Printf("%d with CR (old mac)\n", nCr)
	}
	if nLf > 0 {
		fmt.Printf("%d with LF (unix)\n", nLf)
	}
	if nCrLf > 0 {
		fmt.Printf("%d with CR LF (windows)\n", nCrLf)
	}
}
