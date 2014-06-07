import Foundation

extension NSInputStream {
    func readUInt8() -> Int? {
        var n = sizeof(UInt8)
        var buf = Array<UInt8>(count:n, repeatedValue: 0)
        var nRead = self.read(&buf, maxLength: n)
        return nRead == n ? Int(buf[0]) : nil
    }

    func readUInt16() -> Int? {
        var n = 2*sizeof(UInt8)
        var buf = Array<UInt8>(count:n, repeatedValue: 0)
        var nRead = self.read(&buf, maxLength: n)
        if nRead != n {
            return nil
        }
        return Int(buf[1]) << 8 | Int(buf[0])
    }
}

class BinaryReader : NSInputStream {
    var ok : Bool = false
}

func usageAndExit() {
    print("Usage: parseico fileName\n")
    exit(1)
}

if C_ARGC != 2 {
    usageAndExit()
}

func getErr(e : NSError?) -> NSError {
    if let e2 = e {
        return e2
    }
    return NSError(domain: "unknown error", code: 0, userInfo: nil)
}

let fileName = String.fromCString(C_ARGV[1])
var err : NSError? = nil
var d = NSData.dataWithContentsOfFile(fileName, options: nil, error: &err)
if !d {
    let e = getErr(err)
    print("failed to read '\(fileName)'. Error: '\(e.localizedDescription)'\n")
    exit(1)
}

print("size of \(fileName) is \(d.length) bytes\n")

// TODO: could read directly from a file
let stream = NSInputStream(data: d)
stream.open()
struct ICONDIR {
    var reserved : Int = 0 // off: 0,  len: 2
    var type : Int = 0 // off: 2, len: 2; 1 : .ico, 2 : .cur
    var count : Int = 0 // off: 4, len: 2; number of images in the file
}

func errfatal(msg : String) {
    print(msg)
    exit(1)
}
var iconDir = ICONDIR()

func validIcoType(n : Int) -> Bool {
    switch n {
    case 1,2:
        return true
    default:
        return false
    }
}

var ok = false
if let reserved = stream.readUInt16() {
    if let type = stream.readUInt16() {
        if let count = stream.readUInt16() {
            iconDir.reserved = reserved
            iconDir.type = type
            iconDir.count = count
            ok = true
            if reserved != 0 {
                ok = false
            }
            if !validIcoType(type) {
                ok = false
            }
        }
    }
}
if !ok {
    errfatal("failed to read ICONDIR")
}
print("type: \(iconDir.type)\ncount: \(iconDir.count)\n")
