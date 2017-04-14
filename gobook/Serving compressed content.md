
# Summary
Relevant request headers: `accept-encoding`.
Relevant response headers: `content-encoding`.

How to serve compressed content: 
* check if client support compression by checking `accept-encoding` header
* if compression is supported, send compressed version of html/css/js data and set `content-encoding` to tell the client which compression format you used 
* for static content, pre-generate compressed versions at build time to avoid burning cpu compressing the same data over and over again
# The basics
It’s better to send less data over the network that more data.
To send less data we can send them in compressed format.
Modern browsers understand several compressed formats and they tell the web server which formats they can decode using `accept-encoding` request header.
Here’s what Chrome 57 tells the web server: `accept-encoding:gzip, deflate, sdch, br`
`gzip` and `deflate` (zlib) are supported by every browser and have very similar compression ratios.
Don’t worry about `sdch`. It’s a minor improvement over gzip/deflate and is [not supported by most browsers][1].
`br` is a latest compression format. It compresses better than gzip/deflate and is [supported by most modern browsers][2] (except Safari, because Safari is the new IE).
To use compression the server has to check `accept-encoding` to see if the client supports given compression scheme, send data in compressed format and set `content-encoding` header to tell the client which compression scheme was used, e.g.: `content-encoding: deflate`.
Go has native support for gzip and deflate compression, so I recommend to support at least those.
Go doesn’t have native brotli compressor and brotli compression is much slower than deflate, so I recommend to support it for static content, where compression can be done during build step, when compression time is not critical and command-line brotli encoder can be used.
# Example of serving compressed data
	// TODO: write code
TODO: pre-generating multiple compressed versions during build
# Links
Relevant links:
* [brotli benchmarks][3] (better compression ratio, slower compression speed)
* gopkg.in/kothar/brotli-go.v0/dec : cgo bindings for Brotli C implementation (github repo is at [https://github.com/kothar/brotli-go][4])
* [https://github.com/dsnet/compress][5] : pure Go Brotli decoder


[1]:	http://caniuse.com/#feat=sdch
[2]:	http://caniuse.com/#feat=brotli
[3]:	https://www.opencpu.org/posts/brotli-benchmarks/
[4]:	https://github.com/kothar/brotli-go
[5]:	https://github.com/dsnet/compress