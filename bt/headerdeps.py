import sys, os, os.path, string, re

src_files = [
  (r"C:\kjk\src\libtorrent\src",
  "alert.cpp",
  "allocate_resources.cpp",
  "bandwidth_manager.cpp",
  "bt_peer_connection.cpp",
  "entry.cpp",
  "escape_string.cpp",
  "file.cpp",
  "file_pool.cpp",
  "file_win.cpp",
  "http_connection.cpp",
  "http_stream.cpp",
  "http_tracker_connection.cpp",
  "identify_client.cpp",
  "instantiate_connection.cpp",
  "ip_filter.cpp",
  "logger.cpp",
  "lsd.cpp",
  "metadata_transfer.cpp",
  "natpmp.cpp",
  "peer_connection.cpp",
  "piece_picker.cpp",
  "policy.cpp",
  "session.cpp",
  "session_impl.cpp",
  "sha1.cpp",
  "socks5_stream.cpp",
  "stat.cpp",
  "storage.cpp",
  "torrent.cpp",
  "torrent_handle.cpp",
  "torrent_info.cpp",
  "tracker_manager.cpp",
  "udp_tracker_connection.cpp",
  "upnp.cpp",
  "ut_pex.cpp",
  "web_peer_connection.cpp",
  ),
  (r"C:\kjk\src\libtorrent\src\kademlia",
  "closest_nodes.cpp",
  "dht_tracker.cpp",
  "find_data.cpp",
  "node.cpp",
  "node_id.cpp",
  "refresh.cpp",
  "routing_table.cpp",
  "rpc_manager.cpp",
  "traversal_algorithm.cpp",
  ),
]

def find_include_path(name):
    include_paths = [
#       r"C:\cygwin\home\user\libtorrent\include"
#      ,r"C:\cygwin\home\user\libtorrent\zlib"
       r"C:\kjk\src\libtorrent\include"
      ,r"C:\kjk\src\libtorrent\zlib"
      ,r"C:\cygwin\home\user\boost_1_33_1"
      ,r"C:\cygwin\home\user\asio\include"
    ]
    for include_path in include_paths:
        path = os.path.join(include_path, name)
        if os.path.exists(path):
            return include_path
    return None

include_re = r'#\s?include\s?["<"]([^">]+)[">]'
include_rec = re.compile(include_re)

class include_file:
    def __init__(self,name):
        self.name = name
        self.dir = None
        self.includers = []
    def path(self):
        if self.dir:
            return os.path.join(self.dir, self.name)
        else:
            return self.name

class source_file:
    def __init__(self,path):
        self.path = path
        self.includes = []
        self.recursive_includes = {}

def convert_path_name(name):
    if os.name is "nt":
        return name.replace(r"/", "\\")
    return name

g_includes = {}
def find_include(name):
    global g_includes
    if not name in g_includes:
        inc = include_file(name)
        inc.dir = find_include_path(name)
        g_includes[name] = inc        
    return g_includes[name]

# extract includes from a source (.c* or .h*)
def includes_from_source_file(file_path):
    global include_rec
    lines = file(file_path).readlines()
    includes = []
    for line in lines:
        line = line.strip()
        m = include_rec.search(line)
        if m:
            inc = convert_path_name(m.group(1))
            includes.append(inc)
    return includes

def main():
    global g_includes
    src_to_includes = {}
    # extract includes from source files, recursively parse include files
    to_check = []
    for src_file_dsc in src_files:
        dir_path = src_file_dsc[0]
        files = src_file_dsc[1:]
        for file in files:
            file_path = os.path.join(dir_path, file)
            assert file_path not in to_check # warning: expensive if lots of files
            to_check.append(file_path)
            #print "check * '%s'" % file_path
        
    while len(to_check) > 0:
        file_path = to_check[0]
        to_check = to_check[1:]
        assert file_path not in src_to_includes
        src = source_file(file_path)
        includes = includes_from_source_file(file_path)
        src.includes = includes
        src_to_includes[file_path] = src
        for inc_name in includes: 
            inc = find_include(inc_name)
            if inc.dir: # we found this file in filesystem
                if inc.path() not in src_to_includes and inc.path() not in to_check:
                    to_check.append(inc.path())
                    #print "check # '%s'" % inc.path()

    inc_paths = [inc.path() for inc in g_includes.values()]
    inc_paths.sort()
    for inc in inc_paths:
        print inc

if __name__ == "__main__":
	main()
