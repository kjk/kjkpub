import sys, os, string, fnmatch

g_langs = [
  ["C", ".h", ".c"],
  ["C++", ".cpp", ".hpp", ".C++", ".h++", ".cxx", ".cc"],
  ["Java", ".java"],
  ["Python", ".py"],
  ["Perl", ".pl"],
  ["Assembly", ".s", ".asm"],
  ["Factor", ".factor"],
  ["PHP", ".php", ".inc"],
  ["TCL", ".tcl"],
  ["Ruby", ".rb"],
  ["C#", ".cs"],
  ["Slate", ".slate"],
  ["Scheme", ".scm" ,".ss"],
  ["Lisp", ".lisp", ".lsp"],
  ["Smalltalk", ".st"],
  ["Html", ".htm", "html", ".css"],
  ["Text", ".txt"],
  ["Shell", ".sh"],
  ["Batch", ".bat"],
  ["JavaScript", ".js"],
  ["OCaml", ".mli", ".ml", ".mlp"],
  ["Dylan", ".dylan", ".lid"], 
  ["Objective-C", ".m", ".mm"]
  ]

# given a number num, returns an easier to read representation e.g.
# 10023 => "10.023"
def numPretty(num):
    txt = "%d" % num
    txtLen = len(txt)
    loops = txtLen / 3
    if 0 == loops:
        return txt
    outArr = []
    for i in range(loops):
        outArr.append(txt[txtLen-(3*(i+1)):txtLen-3*i])
    if 0 != txtLen % 3:
        outArr.append(txt[0:txtLen % 3])
    outArr.reverse()
    txt = string.join(outArr, ".")
    return txt

# directory walk
def Walk(root, recurse=0, pattern='*', return_folders=0):
    # initialize
    result = []

    # must have at least root folder
    try:
        names = os.listdir(root)
    except os.error:
        return result

    # expand pattern
    pattern = pattern or '*'
    pat_list = string.splitfields( pattern , ';' )

    # check each file
    for name in names:
        fullname = os.path.normpath(os.path.join(root, name))

        # grab if it matches our pattern and entry type
        for pat in pat_list:
            if fnmatch.fnmatch(name, pat):
                if os.path.isfile(fullname) or (return_folders and os.path.isdir(fullname)):
                    result.append(fullname)
                continue

        # recursively scan other folders, appending results
        if recurse:
            if os.path.isdir(fullname) and not os.path.islink(fullname):
                result = result + Walk( fullname, recurse, pattern, return_folders )

    return result

def langNameFromFileName(fileName):
    global g_langs
    fileNameLen = len(fileName)
    for lang in g_langs:
        langName = lang[0]
        postfixList = lang[1:]
        for postfix in postfixList:
            postfixLen = len(postfix)
            if fileNameLen >= postfixLen:
                fileNamePostfix = string.lower(fileName[fileNameLen-postfixLen:])
                if postfix == fileNamePostfix:
                    return langName
    return None

def test_langNameFromFileName():
    fileTypeTable = (("hello.c", "C"), ("hello.h","C"), ("hello.py","Python"), ("hello.pl", "Perl"), ("hello.java", "Java"), ("hello.hpp", "C++"), ("foo", None))
    for fileType in fileTypeTable:
        fileName = fileType[0]
        expectedLangName = fileType[1]
        realLangName = langNameFromFileName(fileName)
        if realLangName != expectedLangName:
            print "real: %s, expected: %s" % (realLangName, expectedLangName)
        assert realLangName == expectedLangName

def calcLocForFile(file):
    fo = open(file, "rb")
    linesCount = 0
    for line in fo:
        linesCount += 1
    fo.close()
    return linesCount

def txtPadRight(txt, totalLen):
    spaces = "                                   "
    if len(txt) < totalLen:
        txt += spaces[:totalLen-len(txt)]
    return txt

def txtPadLeft(txt, totalLen):
    spaces = "                                   "
    if len(txt) < totalLen:
        txt = spaces[:totalLen-len(txt)] + txt
    return txt

def sortBySizePred(el1, el2):
    return cmp(el2[1], el1[1])

def dumpStats(locStats, sortBySize=False):
    totalLineCountAllLangs = 0
    locArr = locStats.items()
    locTotals = []
    for (langName, stats) in locArr:
        fileCount = len(stats)
        totalLineCount = 0
        for stat in stats:
            lineCount = stat[0]
            totalLineCount += lineCount
        totalLineCountAllLangs += totalLineCount
        locTotals.append((langName, totalLineCount))
    if sortBySize:
        locTotals.sort(sortBySizePred)
    for (langName, totalLineCount) in locTotals:
        print "%s: %s" % (txtPadRight(langName,len("Total lines")), txtPadLeft(numPretty(totalLineCount),10))
    print
    print "Total lines: %s" % numPretty(totalLineCountAllLangs)

def main():
    test_langNameFromFileName()
    if len(sys.argv) < 2:
        print "Usage: loccount.py dirname1 [dirname2 ..]"
        sys.exit(0)

    locStats = {}
    for dirName in sys.argv[1:]:
        files = Walk(dirName, 1, '*', 0)
        #print 'There are %s files below current location:' % len(files)
        for file in files:
            #print file
            langName = langNameFromFileName(file)
            if langName != None:
                loc = calcLocForFile(file)
                if locStats.has_key(langName):
                    locStats[langName].append((loc, file))
                else:
                    locStats[langName] = [(loc, file)]
    print 
    dumpStats(locStats, True)

def main2():
    print numPretty(1)
    print numPretty(10)
    print numPretty(100)
    print numPretty(10000)
    print numPretty(100000)
    print numPretty(1000000)

if __name__ == "__main__":
    main()
