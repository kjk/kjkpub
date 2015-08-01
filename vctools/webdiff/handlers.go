package main

import (
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"os"
	"path/filepath"
	"strings"

	"github.com/kjk/u"
)

// /static/$rest
func handleStatic(w http.ResponseWriter, r *http.Request) {
	fileName := r.URL.Path[len("/static/"):]
	path := filepath.Join("www", fileName)
	if u.PathExists(path) {
		LogInfof("%s\n", path)
		http.ServeFile(w, r, path)
	} else {
		LogInfof("file %q doesn't exist, referer: %q\n", path, getReferer(r))
		http.NotFound(w, r)
	}
}

// /
func handleIndex(w http.ResponseWriter, r *http.Request) {
	path := filepath.Join("www", "index.html")
	http.ServeFile(w, r, path)
}

func handleFavicon(w http.ResponseWriter, r *http.Request) {
	http.NotFound(w, r)
}

func gitItemJSONType(typ int) string {
	switch typ {
	case NotVersioned:
		return "u"
	case Modified:
		return "m"
	case Deleted:
		return "d"
	case Added:
		return "a"
	default:
		LogFatalf("unknown type: %d\n", typ)
		return "INVALID"
	}

}

// /api/getfilelist.json
// Arguments:
//  - jsonp : jsonp wrapper, optional
func handleAPIGetFileList(w http.ResponseWriter, r *http.Request) {
	jsonp := strings.TrimSpace(r.FormValue("jsonp"))
	LogInfof("jsonp: '%s'\n", jsonp)
	var v [][]string
	status := gGitInfo.GitStatusItems
	for i := 0; i < len(status); i++ {
		var el []string
		gi := status[i]
		typeStr := gitItemJSONType(gi.Type)
		el = append(el, typeStr)
		// TODO: set other fields depending on the type
	}
	httpOkWithJSON(w, nil, v)
}

// /api/kill
func handleAPIKill(w http.ResponseWriter, r *http.Request) {
	//LogInfof("Exiting because /api/kill\n")
	os.Exit(1)
}

func registerHTTPHandlers() {
	http.HandleFunc("/", handleIndex)
	http.HandleFunc("/static/", handleStatic)
	http.HandleFunc("/favicon.ico", handleFavicon)
	http.HandleFunc("/api/getfilelist.json", handleAPIGetFileList)
	http.HandleFunc("/api/kill", handleAPIKill)
}

func startWebServerAsync() int {
	registerHTTPHandlers()

	// generate random
	httpAddr := rand.Intn(10000) + 50000
	httpAddrStr := fmt.Sprintf(":%d", httpAddr)
	fmt.Printf("Started runing on %s\n", httpAddrStr)
	go func() {
		if err := http.ListenAndServe(httpAddrStr, nil); err != nil {
			log.Fatalf("http.ListendAndServer() failed with %s\n", err)
		}
	}()
	fmt.Printf("Exited\n")
	return httpAddr
}
