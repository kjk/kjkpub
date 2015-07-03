package main

import (
	"bytes"
	"fmt"
	"log"
	"runtime"
)

var (
	dot       = []byte(".")
	centerDot = []byte("·")
)

func FunctionFromPc(pc uintptr) string {
	fn := runtime.FuncForPC(pc)
	if fn == nil {
		return ""
	}
	name := []byte(fn.Name())
	// The name includes the path name to the package, which is unnecessary
	// since the file name is already included.  Plus, it has center dots.
	// That is, we see
	//      runtime/debug.*T·ptrmethod
	// and want
	//      *T.ptrmethod
	if period := bytes.Index(name, dot); period >= 0 {
		name = name[period+1:]
	}
	name = bytes.Replace(name, centerDot, dot, -1)
	return string(name)
}

// For logging of misc non-error things
func LogInfof(format string, arg ...interface{}) {
	s := fmt.Sprintf(format, arg...)
	if pc, _, _, ok := runtime.Caller(1); ok {
		s = FunctionFromPc(pc) + ": " + s
	}
	fmt.Print(s)
}

// like log.Fatalf() but also pre-pends name of the caller, so that we don't
// have to do that manually in every log statement
func LogFatalf(format string, arg ...interface{}) {
	s := fmt.Sprintf(format, arg...)
	if pc, _, _, ok := runtime.Caller(1); ok {
		s = FunctionFromPc(pc) + ": " + s
	}
	fmt.Print(s)
	log.Fatal(s)
}

// For logging things that are unexpected but not fatal
// Automatically pre-pends name of the function calling the log function
func LogErrorf(format string, arg ...interface{}) {
	s := fmt.Sprintf(format, arg...)
	if pc, _, _, ok := runtime.Caller(1); ok {
		s = FunctionFromPc(pc) + ": " + s
	}
	fmt.Print(s)
}
