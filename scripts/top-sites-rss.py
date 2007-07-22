# Written by Krzysztof Kowalczyk (http://blog.kowalczyk.info)
# 
# Purpose: get list of rss feeds from list of top sites
#
# Steps:
#  * download list of top sites (http://dt.palm.com/chrisl/Top_Sites.html)
#  * extract sites
#  * for each site extract rss feed (if exists)

import sys, os, os.path, string, random, time, re, socket, urllib, urllib2
from httplib import HTTPConnection, HTTPException

# those headers are supposed to mimic FireFox
user_agent_hdr = "User-Agent"
user_agent_val = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.7.5) Gecko/20041107 Firefox/1.0"

accept_hdr = "Accept"
accept_val = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5"

accept_lang_hdr = "Accept-Language"
accept_lang_val = "en-us,en;q=0.5"

# don't give that, not sure if python handles it
# Accept-Encoding: gzip,deflate

accept_encoding_hdr = "Accept-encoding"
accept_encoding_val = "gzip"

content_encoding_hdr = "Content-Encoding"

accept_charset_hdr = "Accept-Charset"
accept_charset_val = "ISO-8859-1,utf-8;q=0.7,*;q=0.7"

keep_alive_hdr = "Keep-Alive"
keep_alive_val = "300"

connection_hdr = "Connection"
connection_val = "keep-alive"

referer_hdr = "Referer"

location_hdr = "Location"

# a class to trick urllib2 to not handle redirects
class HTTPRedirectHandlerNoRedirect(urllib2.HTTPRedirectHandler):
    def http_error_302(self, req, fp, code, msg, headers):
        #print "******** HTTPRedirectHandlerNoRedirect() called"
        pass

def _getHttpHelper(url, postData, handleRedirect, dbgLevel, referer, cookieJar):
    import cookielib
    print "_getHttpHelper(%s)" % url
    req = urllib2.Request(url)
    req.add_header(user_agent_hdr, user_agent_val)
    req.add_header(accept_hdr, accept_val)
    req.add_header(accept_lang_hdr, accept_lang_val)
    req.add_header(accept_charset_hdr, accept_charset_val)
    req.add_header(keep_alive_hdr, keep_alive_val)
    req.add_header(connection_hdr, connection_val)
    if None != referer:
        req.add_header(referer_hdr, referer)

    if None != postData:
        req.add_data(urllib.urlencode(postData))

    httpHandler = urllib2.HTTPHandler(debuglevel=dbgLevel)
    httpsHandler = urllib2.HTTPSHandler(debuglevel=dbgLevel)
    if cookieJar is None:
        cookieJar = cookielib.CookieJar()

    cookieHandler = urllib2.HTTPCookieProcessor(cookieJar)

    if handleRedirect:
        opener = urllib2.build_opener(cookieHandler, httpHandler, httpsHandler)
    else:
        noRedirectHandler = HTTPRedirectHandlerNoRedirect()
        opener = urllib2.build_opener(cookieHandler, httpHandler, httpsHandler, noRedirectHandler)

    url_handle = opener.open(req)
    if not url_handle:
        # print "_getHttpHelper() - no url_handle"
        return None

    headers = url_handle.info()

    # TODO: is it always present? What happens if it's not?
    encoding = url_handle.headers.get(content_encoding_hdr)

    if "gzip" == encoding:
        htmlCompressed = url_handle.read()
        compressedStream = StringIO.StringIO(htmlCompressed)
        gzipper = gzip.GzipFile(fileobj=compressedStream)
        htmlTxt = gzipper.read()
    else:
        htmlTxt = url_handle.read()

    url_handle.close()
    # TODO: log somewhere how long retrieving this url took
    # print "_getHttpHelper() htmlTxt size=%d" % len(htmlTxt)
    return htmlTxt

def _getHttpHandleException(url, postData, handleRedirect, dbgLevel, referer, cookieJar):
    htmlTxt = None
    try:
        htmlTxt = _getHttpHelper(url, postData, handleRedirect, dbgLevel, referer, cookieJar)
    except Exception, ex:
        pass
    return htmlTxt

def _getHttpHandleExceptionRetry(url, postData, handleRedirect, dbgLevel, referer, retryCount, cookieJar):
    assert retryCount > 0
    print "_getHttpHandleExceptionRetry()"
    while True:
        htmlTxt = None
        try:
            htmlTxt = _getHttpHelper(url, postData, handleRedirect, dbgLevel, referer, cookieJar)
        except socket.error, (err,txt):
            retryCount -= 1
            if retryCount < 0:
                return None
        return htmlTxt

# do an http request to retrieve html data for url. By default we use GET request
# if postData (a dictionary) is given we do POST request
# set handleRedirect to False to disable automatic handling of HTTP redirect (302 etc.)
# set dbgLevel to 1 to have HTTP request and response headers dumped to stdio
# to set HTTP header referer, use referer
# TODO: add support for cookies
def getHttp(url, postData = None, handleRedirect=True, dbgLevel=0, referer=None, retryCount=0, handleException=True, cookieJar = None):
    htmlTxt = None
    if handleException:
        if retryCount > 0:
            htmlTxt = _getHttpHandleExceptionRetry(url, postData, handleRedirect, dbgLevel, referer, retryCount, cookieJar)
        else:
            htmlTxt = _getHttpHandleException(url, postData, handleRedirect, dbgLevel, referer, cookieJar)
    else:
        htmlTxt = _getHttpHelper(url, postData, handleRedirect, dbgLevel, referer, cookieJar)
    return htmlTxt

SITES_FILE_NAME = "top-sites.html"
SITES_URL = "http://dt.palm.com/chrisl/Top_Sites.html"

RSS_TEST_DATA = """
<meta name="Keywords" content="Apple Computer">
<link rel="home" href="http://www.apple.com/">
<link rel="alternate" type="application/rss+xml" title="RSS" href="http://wdirect.apple.com/home/2006/ticker.rss">
<link rel="index" href="http://www.apple.com/find/sitemap.html">
<link rel="stylesheet" type="text/css" href="/main/css/global.css" media="all">
<style type="text/css" media="all">
body {text-align: center; background-color: #fff;}
"""

def get_text_between(txt, start, end):
  start_pos = txt.find(start)
  if -1 == start_pos: return None
  start_pos += len(start)
  end_pos = txt.find(end, start_pos)
  if -1 == end_pos: return None
  return txt[start_pos:end_pos]

def extract_rss_feed_from_html(htmlTxt):
  app_rss_pos = htmlTxt.find("application/rss+xml")
  if -1 == app_rss_pos: return None
  htmlRest = htmlTxt[app_rss_pos:]
  rss_feed = get_text_between(htmlRest, 'href="', '"')
  print "FOUND: %s" % rss_feed
  return rss_feed

def test_extract_rss_feed():
  rss_feed = extract_rss_feed_from_html(RSS_TEST_DATA)
  #print rss_feed
  assert rss_feed == "http://wdirect.apple.com/home/2006/ticker.rss"

def get_rss_feed_from_url(url):
  #print "started downloading '%s'" % url
  htmlTxt = getHttp(url, dbgLevel=0)
  if not htmlTxt: return None
  #print "finished downloading '%s'" % url
  return extract_rss_feed_from_html(htmlTxt)

def download_top_sites(file_name):
  #print "started downloading sites"
  htmlTxt = getHttp(SITES_URL, dbgLevel=0, retryCount=4)
  #print "finished downloading sites"
  fo = open(file_name, "wb")
  fo.write(htmlTxt)
  fo.close()  
  
def parse_top_sites(file_name):
  fo = open(file_name, "rb")
  htmlTxt = fo.read()
  fo.close()
  urls = []
  for l in htmlTxt.split("\n"):
    #print l
    url = get_text_between(l, '<a href="', '"')
    if None == url: continue
    #print url
    urls.append(url)
  return urls

RSS_OUT = "top-sites-found-rss.txt"
  
def main():
  if not os.path.exists(SITES_FILE_NAME):
    download_top_sites(SITES_FILE_NAME)
  sites = parse_top_sites(SITES_FILE_NAME)
  rss_feeds = []
  for site in sites:
    feed = get_rss_feed_from_url(site)
    if feed: rss_feeds.append(feed)
  fo = open(RSS_OUT, "w")
  for feed in rss_feeds:
    fo.write(feed + "\n")
  fo.close()

if __name__ == "__main__":
  #test_extract_rss_feed()
  #print get_rss_feed_from_url("http://www.apple.com")
  main()
