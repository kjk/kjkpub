package main

import (
	"fmt"
	"os"
)

func main() {
	fmt.Printf("Starting program\n")
	var files []*os.File
	var fileNames []string
	for {
		n := len(fileNames)
		name := fmt.Sprintf("%d.test.txt", n)
		os.Remove(name)
		f, err := os.Create(name)
		if err != nil {
			fmt.Printf("opening file %s failed with %s\n", name, err)
			break
		}
		fileNames = append(fileNames, name)
		files = append(files, f)
	}
	fmt.Printf("Limit on number of opened files: %d\n", len(fileNames))
	for i := range fileNames {
		files[i].Close()
		os.Remove(fileNames[i])
	}
}
