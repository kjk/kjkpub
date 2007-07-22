#!/usr/bin/env python
#
# Copyright (c) 2006 Ricardo Garcia Gonzalez
#
# Permission is hereby granted, free of charge, to any person obtaining a
# copy of this software and associated documentation files (the "Software"),
# to deal in the Software without restriction, including without limitation
# the rights to use, copy, modify, merge, publish, distribute, sublicense,
# and/or sell copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included
# in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
# THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
# OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
# ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.
# 
# Except as contained in this notice, the name(s) of the above copyright
# holders shall not be used in advertising or otherwise to promote the
# sale, use or other dealings in this Software without prior written
# authorization.
#
import sys
import optparse
import httplib
import urllib2
import re
import string
import os

# First off, check Python and refuse to run
if sys.hexversion < 0x020400f0:
	sys.exit('Error: Python 2.4 or later needed to run the program')

# Global constants
const_video_url_str = 'http://www.youtube.com/watch?v=%s'
const_video_url_re = re.compile(r'(?:http://)?(?:www\d*\.)?youtube\.com/(?:v/|(?:watch(?:\.php)?)?\?v=)([^&]+).*')
const_login_url_str = 'http://www.youtube.com/login?next=/watch%%3Fv%%3D%s'
const_login_post_str = 'current_form=loginForm&next=%%2Fwatch%%3Fv%%3D%s&username=%s&password=%s&action_login=Log+In'
const_age_url_str = 'http://www.youtube.com/verify_age?next_url=/watch%%3Fv%%3D%s'
const_age_post_str = 'next_url=%%2Fwatch%%3Fv%%3D%s&action_confirm=Confirm'
const_video_url_params_re = re.compile(r'player2\.swf\?([^"]+)"', re.M)
const_video_url_real_str = 'http://www.youtube.com/get_video?%s'
const_video_title_re = re.compile(r'<title>YouTube - ([^<]*)</title>', re.M | re.I)
const_block_size = 10 * 1024

def error_advice(error_text):
	sys.stderr.write('Error: %s.\n' % error_text)
	sys.stderr.write('Try again several times. It may be a temporal problem.\n')
	sys.stderr.write('Other typical problems:\n\n')
	sys.stderr.write('\tVideo no longer exists.\n')
	sys.stderr.write('\tVideo requires age confirmation but you did not provide an account.\n')
	sys.stderr.write('\tYou provided the account data, but it is not valid.\n')
	sys.stderr.write('\tThe connection was cut suddenly for some reason.\n')
	sys.stderr.write('\tYouTube changed their system, and the program no longer works.\n')
	sys.stderr.write('\nTry to confirm you are able to view the video using a web browser.\n')
	sys.stderr.write('Use the same video URL and account information, if needed, with this program.\n')
	sys.stderr.write('When using a proxy, make sure http_proxy has http://host:port format.\n')
	sys.stderr.write('Try again several times and contact me if the problem persists.\n')

# Print error message, followed by standard advice information, and then exit
def error_advice_exit(error_text):
	error_advice(error_text)
	sys.exit('\n')

# Wrapper to create custom requests with typical headers
def request_create(url, data=None):
	retval = urllib2.Request(url)
	if not data is None:
		retval.add_data(data)
	# Try to mimic Firefox, at least a little bit
	retval.add_header('User-Agent', 'Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1) Gecko/20061010 Firefox/2.0')
	retval.add_header('Accept-Charset', 'ISO-8859-1,utf-8;q=0.7,*;q=0.7')
	retval.add_header('Accept', 'text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5')
	retval.add_header('Accept-Language', 'en-us,en;q=0.5')
	return retval

# Perform a request, process headers and return response
def perform_request(url, data=None):
	request = request_create(url, data)
	response = urllib2.urlopen(request)
	return response

# Convert bytes to KiB
def to_k(bytes):
	return bytes / 1024

# Conditional print
def cond_print(str):
	global cmdl_opts
	if not cmdl_opts.quiet:
		sys.stdout.write(str)
		sys.stdout.flush()

# Title string normalization
def title_string_norm(title):
	title = title.lower()
	title = map(lambda x: x in string.whitespace and '_' or x, title)
	title = filter(lambda x: x in string.lowercase or x in string.digits or x == '_', title)
	return ''.join(title)

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
    if dbgLevel > 0: print "_getHttpHelper(%s)" % url
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
    if dbgLevel > 0: print "_getHttpHandleExceptionRetry()"
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

def getHttpCached(url, file_name):
  if os.path.exists(file_name):
  	return
  htmlTxt = getHttp(url, dbgLevel=0, retryCount=4)
  fo = open(file_name, "wb")
  fo.write(htmlTxt)
  fo.close()  

# Generic download step
def download_step(return_data_flag, step_title, step_error, url, post_data=None):
	try:
		cond_print('%s... ' % step_title)
		data = perform_request(url, post_data).read()
		cond_print('done.\n')
		if return_data_flag:
			return data
		return None

	except (urllib2.URLError, ValueError, httplib.HTTPException, TypeError):
		cond_print('failed.\n')
		error_advice(step_error)

	except KeyboardInterrupt:
		sys.exit('\n')

# Generic extract step
def extract_step(step_title, step_error, regexp, data):
	try:
		#cond_print('%s... ' % step_title)
		match = regexp.search(data)
		
		if match is None:
			cond_print('failed.\n')
			error_advice(step_error)
			return None
		
		extracted_data = match.group(1)
		#cond_print('done.\n')
		return extracted_data
	
	except KeyboardInterrupt:
		sys.exit('\n')

def dl_youtube_video(video_url_id, video_filename, cmdl_opts, dl_if_exists = False):
	video_url = const_video_url_str % video_url_id

	final_filename = None
	# Check name
	if not video_filename.lower().endswith('.flv'):
		sys.stderr.write('Warning: video file name does not end in .flv\n')
	
	# Test writable file
	if not cmdl_opts.simulate:
		try:
			disk_test = open(video_filename, 'wb')
			disk_test.close()
	
		except (OSError, IOError):
			sys.exit('Error: unable to open %s for writing.' % video_filename)	

	# Log in and confirm age if needed
	if not cmdl_opts.username is None:
		url = const_login_url_str % video_url_id
		post = const_login_post_str % (video_url_id, cmdl_opts.username, cmdl_opts.password)
		download_step(False, 'Logging in', 'unable to log in', url, post)
	
		url = const_age_url_str % video_url_id
		post = const_age_post_str % video_url_id
		download_step(False, 'Confirming age', 'unable to confirm age', url, post)
	
	# Retrieve video webpage
	video_webpage = download_step(True, 'Retrieving video webpage', 'unable to retrieve video webpage', video_url)
	
	# Extract video title if needed
	if cmdl_opts.use_title:
		video_title = extract_step('Extracting video title', 'unable to extract video title', const_video_title_re, video_webpage)
		final_filename = '%s-%s.flv' % (title_string_norm(video_title), video_url_id)
		if not dl_if_exists and os.path.exists(final_filename):
			print "%s already exists" % final_filename
			return
			
	# Extract needed video URL parameters
	video_url_params = extract_step('Extracting video URL parameters', 'unable to extract URL parameters', const_video_url_params_re, video_webpage)
	video_url_real = const_video_url_real_str % video_url_params
	
	# Retrieve video data
	try:
		video_data = perform_request(video_url_real)
		if final_filename:
			cond_print('Video data found at %s for %s\n' % (video_data.geturl(), final_filename))
		else:
			cond_print('Video data found at %s\n' % video_data.geturl())
	
		# Abort here if in simulate mode
		if cmdl_opts.simulate:
			return
	
		cond_print('Retrieving video data... ')
		video_file = open(video_filename, 'wb')
		try:
			video_len_str = '%sk' % to_k(long(video_data.info()['Content-length']))
		except KeyError:
			video_len_str = '(unknown)'
	
		byte_counter = 0
		video_block = video_data.read(const_block_size)
		while len(video_block) != 0:
			byte_counter += len(video_block)
			video_file.write(video_block)
			cond_print('\rRetrieving video data... %sk of %s ' % (to_k(byte_counter), video_len_str))
			video_block = video_data.read(const_block_size)
	
		video_file.close()
		cond_print('done.\n')
		cond_print('Video data saved to %s\n' % video_filename)
	
	except (urllib2.URLError, ValueError, httplib.HTTPException, TypeError):
		cond_print('failed.\n')
		error_advice('unable to download video data')
		return
	
	except KeyboardInterrupt:
		sys.exit('\n')

	# Rename video file if needed
	if final_filename:
		try:
			os.rename(video_filename, final_filename)
			cond_print('Video file renamed to %s\n' % final_filename)
		except OSError:
			sys.stderr.write('Warning: unable to rename file.\n')
	
def main():
	global cmdl_opts
	# Create the command line options parser and parse command line
	cmdl_usage = 'usage: %prog [options] video_url'
	cmdl_version = '2006.12.07'
	cmdl_parser = optparse.OptionParser(usage=cmdl_usage, version=cmdl_version, conflict_handler='resolve')
	cmdl_parser.add_option('-h', '--help', action='help', help='print this help text and exit')
	cmdl_parser.add_option('-v', '--version', action='version', help='print program version and exit')
	cmdl_parser.add_option('-u', '--username', dest='username', metavar='USERNAME', help='account username')
	cmdl_parser.add_option('-p', '--password', dest='password', metavar='PASSWORD', help='account password')
	cmdl_parser.add_option('-o', '--output', dest='outfile', metavar='FILE', help='output video file name')
	cmdl_parser.add_option('-q', '--quiet', action='store_true', dest='quiet', help='activates quiet mode')
	cmdl_parser.add_option('-s', '--simulate', action='store_true', dest='simulate', help='do not download video')
	cmdl_parser.add_option('-t', '--title', action='store_true', dest='use_title', help='use title in file name')
	(cmdl_opts, cmdl_args) = cmdl_parser.parse_args()
	
	# Get video URL
	if len(cmdl_args) != 1:
		cmdl_parser.print_help()
		sys.exit('\n')
	video_url_cmdl = cmdl_args[0]
	
	# Verify video URL format and convert to "standard" format
	video_url_mo = const_video_url_re.match(video_url_cmdl)
	if video_url_mo is None:
		sys.exit('Error: URL does not seem to be a youtube video URL. If it is, report a bug.')
	video_url_id = video_url_mo.group(1)

	# Check conflicting options
	if not cmdl_opts.outfile is None and cmdl_opts.simulate:
		sys.stderr.write('Warning: video file name given but will not be used.\n')
	
	if not cmdl_opts.outfile is None and cmdl_opts.use_title:
		sys.exit('Error: using the video title conflicts with using a given file name.')
	
	# Verify both or none present
	if ((cmdl_opts.username is None and not cmdl_opts.password is None) or
	    (not cmdl_opts.username is None and cmdl_opts.password is None)):
		sys.exit('Error: both username and password must be given, or none.')
	
	# Get output file name 
	if cmdl_opts.outfile is None:
		video_filename = '%s.flv' % video_url_id
	else:
		video_filename = cmdl_opts.outfile
	
	# Install cookie and proxy handlers
	urllib2.install_opener(urllib2.build_opener(urllib2.ProxyHandler()))
	urllib2.install_opener(urllib2.build_opener(urllib2.HTTPCookieProcessor()))
	
	dl_youtube_video(video_url_id, video_filename, cmdl_opts)

class DefCmdlOpts:
	def __init__(self):
		self.simulate = False
		self.username = None
		self.password = None
		self.use_title = True
		self.quiet = False

def dl_one(youtube_id):
	global cmdl_opts
	cmdl_opts = DefCmdlOpts()
	dl_youtube_video(youtube_id, youtube_id + ".flv", cmdl_opts)

def main2():
	dl_one("7hywqbPAz0A")

def parse_videos_list(file_name):
	fo = open(file_name, "rb")
	txt = fo.read()
	fo.close()
	video_ids = []
	i = 0
	els = txt.split("video.php?video=")
	els = els[1:]
	video_ids = {}
	for el in els:
		end = el.find("'")
		if end == 11:
			video_id = el[:end]
			video_ids[video_id] = 1
	return video_ids.keys()

# TODO: remember which were already downloaded in a hash pickled to disk, to
# avoid downloading and parsing html page
def main3():
	list_file_name = "youtube-videos-list.html"
	getHttpCached("http://cyber-knowledge.net/videos/videos.php", list_file_name)
	video_ids = parse_videos_list(list_file_name)
	print "videos: %d" % len(video_ids)
	for video_id in video_ids:
		print "downloading %s" % video_id
		dl_one(video_id)

if __name__ == "__main__":
	main3()
