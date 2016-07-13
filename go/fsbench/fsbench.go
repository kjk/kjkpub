package main

import (
	"fmt"
	"io/ioutil"
	"os"
	"strconv"
	"sync"
	"time"
)

var (
	gb = 1024 * 1024 * 1024
)

func genFileData(fileSize int) []byte {
	file := make([]byte, fileSize)
	for i := 0; i < fileSize; i++ {
		file[i] = byte(i % 256)
	}
	return file
}

// FileData describes a file data
type FileData struct {
	Data []byte
	Path string
}

func test(pathBase string, d []byte, parts int, workers int) {
	var fileDatas []FileData
	partSize := len(d) / parts
	for i := 0; i < parts; i++ {
		start := i * partSize
		var data []byte
		if i == parts-1 {
			data = d[start:]
		} else {
			end := start + partSize
			data = d[start:end]
		}
		fd := FileData{
			Data: data,
			Path: pathBase + "." + strconv.Itoa(i),
		}
		fileDatas = append(fileDatas, fd)
	}

	timeStart := time.Now()
	var wg sync.WaitGroup
	c := make(chan FileData)
	for i := 0; i < workers; i++ {
		wg.Add(1)
		go func() {
			for fd := range c {
				err := ioutil.WriteFile(fd.Path, fd.Data, 0644)
				if err != nil {
					panic(err.Error())
				}
			}
			wg.Done()
		}()
	}
	for _, fd := range fileDatas {
		c <- fd
	}
	close(c)
	wg.Wait()
	dur := time.Since(timeStart)
	fmt.Printf("Parts: %4d, workers: %2d, time: %s\n", parts, workers, dur)
	for _, fd := range fileDatas {
		os.Remove(fd.Path)
	}
}

func main() {
	d := genFileData(1 * gb)
	test("foo.txt.1", d, 1, 1)
	test("foo.txt.2", d, 1024, 1)
	test("foo.txt.3", d, 1024, 16)
	test("foo.txt.4", d, 1024, 64)
}
