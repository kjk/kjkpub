package main

import (
	"archive/tar"
	"compress/bzip2"
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"
)

// To run: go run scripts\dlcef.go

const (
	dlURL32 = "http://opensource.spotify.com/cefbuilds/cef_binary_3.3578.1860.g36610bd_windows32.tar.bz2"
)

func must(err error) {
	if err != nil {
		panic(err.Error())
	}
}

func main() {
	var err error
	topDir := "CefTest"
	_, err = os.Stat(topDir)
	if err != nil {
		fmt.Printf("Directory CefTest doesn't exist. Most likely not running this from the right directory. Do 'go run scripts\\dlcef.go'\n")
		os.Exit(1)
	}

	dstDir := filepath.Join(topDir, "cef")
	_, err = os.Stat(dstDir)
	if err == nil {
		fmt.Printf("Cef directory '%s' already exists. To force re-download, delete the directory\n", dstDir)
		os.Exit(0)
	}
	must(os.Mkdir(dstDir, 0755))

	parts := strings.Split(dlURL32, "/")
	archiveName := parts[len(parts)-1]
	archivePath := filepath.Join(topDir, archiveName)
	_, err = os.Stat(archivePath)
	timeStart := time.Now()
	if err != nil {
		fmt.Printf("Downloading %s to %s\n", dlURL32, archivePath)
		req, err := http.NewRequest(http.MethodGet, dlURL32, nil)
		must(err)
		client := http.DefaultClient
		resp, err := client.Do(req)
		must(err)
		if resp.StatusCode != http.StatusOK {
			fmt.Printf("Unexpected http request status code: '%s'\n", resp.Status)
			os.Exit(1)
		}
		dstFile, err := os.Create(archivePath)
		must(err)
		_, err = io.Copy(dstFile, resp.Body)
		if err != nil {
			fmt.Printf("io.Copy() failed with %s\n", err)
			dstFile.Close()
			os.Remove(archivePath)
			os.Exit(1)
		}
		err = dstFile.Close()
		must(err)
		resp.Body.Close()
		fmt.Printf("Downloaded to %s in %s\n", dstFile, time.Since(timeStart))
	}

	timeStart = time.Now()
	f, err := os.Open(archivePath)
	must(err)
	defer f.Close()
	bzr := bzip2.NewReader(f)
	tar := tar.NewReader(bzr)
	nExtracted := 0
	for {
		hdr, err := tar.Next()
		if err == io.EOF {
			err = nil
			break
		}
		if err != nil {
			break
		}
		fi := hdr.FileInfo()
		if fi.IsDir() {
			fmt.Printf("Skipping directory '%s'\n", fi.Name())
			continue
		}
		if !fi.Mode().IsRegular() {
			fmt.Printf("Skipping non-regular file '%s'\n", fi.Name())
		}

		name := filepath.FromSlash(fi.Name())
		dstFilePath := filepath.Join(dstDir, name)
		fmt.Printf("Extracting %s to %s...\n", name, dstFilePath)
		dir := filepath.Dir(dstFilePath)
		err = os.MkdirAll(dir, 0755)
		must(err)
		dstFile, err := os.Create(dstFilePath)
		must(err)
		_, err = io.Copy(dstFile, tar)
		if err != nil {
			dstFile.Close()
			os.Remove(dstFilePath)
			fmt.Printf("Failed to tar extract ")
			os.Exit(1)
		}
		must(dstFile.Close())
		nExtracted++
	}
	if err != nil {
		fmt.Printf("tar extraction failed with %s\n", err)
		os.Exit(1)
	}
	fmt.Printf("Extracted %d files in %s\n", nExtracted, time.Since(timeStart))
}
