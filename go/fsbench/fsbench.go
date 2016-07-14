package main

import (
	"fmt"
	"io/ioutil"
	"os"
	"strconv"
	"sync"
	"time"
)

const (
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

// MbsPerSec returns number of megabytes per second given number of bytes and
// duration
func MbsPerSec(nBytes int64, dur time.Duration) float64 {
	mbs := float64(nBytes) / (1024 * 1024)
	return (mbs * float64(dur)) / float64(time.Second)
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
	nBytes := int64(len(d))
	mbsPerSec := MbsPerSec(nBytes, dur)
	fmt.Printf("Files: %4d, workers: %2d, time: %s, %.2f MBs/sec\n", parts, workers, dur, mbsPerSec)
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
