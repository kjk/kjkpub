package main

import (
	"encoding/xml"
	"fmt"
	"io"
	"log"
	"os"
	"runtime"
	"strings"

	"code.google.com/p/go-charset/charset"
	_ "code.google.com/p/go-charset/data"
)

func fatalIfErr(err error) {
	if err == nil {
		return
	}
	var buf [2048]byte
	n := runtime.Stack(buf[:], false)
	fmt.Printf("%s\n", string(buf[:n]))
	log.Fatalf("failed with '%s'", err)
}

const (
	spacesString = "                                                                "
)

func spaces(n int) string {
	s := spacesString
	for n > len(s) {
		s = s + s
	}
	return s[:n]
}

func indentStr(depth int) string {
	return spaces(depth * 2)
}

func iStrEq(s1, s2 string) bool {
	return strings.EqualFold(s1, s2)
}

func escapeIfNeeded(s string) string {
	if -1 == strings.Index(s, " ") {
		return s
	}
	return "\"" + s + "\""
}

func printStartElement(depth int, v xml.StartElement) {
	name := v.Name.Local
	s := indentStr(depth) + name
	attrStart := len(s)
	maxLineLen := 80
	attributesOnLine := 0
	lineStart := 0
	for _, attr := range v.Attr {
		name := attr.Name.Local
		val := attr.Value
		lineLen := len(s) - lineStart
		// TODO: doesn't take account potential "" for val escaping
		if attributesOnLine > 0 && lineLen+1+len(name)+len(val) > maxLineLen {
			lineStart = len(s) + 1
			s = s + "\n" + spaces(attrStart)
			attributesOnLine = 0
		}
		s += fmt.Sprintf(" %s=%s", name, escapeIfNeeded(val))
		attributesOnLine++
	}
	fmt.Println(s)
}

func parseFile(file string) {
	r, err := os.Open(file)
	fatalIfErr(err)
	defer r.Close()
	dec := xml.NewDecoder(r)
	dec.CharsetReader = charset.NewReader
	var elementStack []string
	for {
		t, err := dec.Token()
		if err == io.EOF {
			break
		}
		if err != nil {
			fatalIfErr(err)
		}
		switch v := t.(type) {
		case xml.StartElement:
			name := v.Name.Local
			depth := len(elementStack)
			elementStack = append(elementStack, name)
			printStartElement(depth, v)

		case xml.EndElement:
			name := v.Name.Local
			depth := len(elementStack)
			lastName := elementStack[depth-1] // TODO: should check for 0
			if !iStrEq(name, lastName) {
				err = fmt.Errorf("'%s' != '%s' (name != lastName)\n", name, lastName)
				fatalIfErr(err)
			}
			elementStack = elementStack[:depth-1]
		}
	}
}

func main() {
	//parseFile("mudraw.vcproj")
	parseFile("libmupdf.vcproj")
}
