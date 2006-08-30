; This script is mosquito lisp implementation of downloading
; a wikipedia article. However, it doesn't work, because
; http-client doesn't use any User-Agent header. Should be
; easy enough to patch

; (load "c:\\kjk\\test.ms")
(import "lib/http-client")
(define *wikipedia-site* "http://en.wikipedia.org/wiki")
(define (get-url-for-term term)
  (string-append *wikipedia-site* "/" term "?action=raw"))

(define (get-raw-article term)
  (define url (get-url-for-term term))
  (http-get url))

(define home-article-response (get-raw-article "home"))
(define home-article-body (http-response-body home-article-response))
