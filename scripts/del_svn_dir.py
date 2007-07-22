import sys, string, os, os.path

def DFS(root, skip_symlinks = 1):
    """Depth first search traversal of directory structure.  Children
    are visited in alphabetical order."""
    stack = [root]
    visited = {}
    while stack:
        d = stack.pop()
        if d not in visited:
            visited[d] = 1
            yield d
        stack.extend(subdirs(d, skip_symlinks))

def subdirs(root, skip_symlinks = 1):
    """Given a root directory, returns the first-level subdirectories."""
    try:
        dirs = [os.path.join(root, x)
                for x in os.listdir(root)]
        dirs = filter(os.path.isdir, dirs)
        if skip_symlinks:
            dirs = filter(lambda x: not os.path.islink(x), dirs)
        dirs.sort()
        return dirs
    except OSError, IOError: return []

def removedirrecursive(top):
  for root, dirs, files in os.walk(top, topdown=False):
    for name in files:
      os.remove(os.path.join(root, name))
    for name in dirs:
      os.rmdir(os.path.join(root, name))

#DFS() returns an iteratable object that marches through all the
#subdirectories of a given root.  Your code for going through all the .html
#files, then, might look something like:

#    for subdir in DFS(root):
#        htmls = glob.glob(os.path.join(subdir, "*.html"))
#        ...

def main():
  dirs_to_remove = []
  for subdir in DFS("."):
    if subdir.endswith(".svn"):
      dirs_to_remove.append(subdir)
  for d in dirs_to_remove:
    print "removing directory " + d
    removedirrecursive(d)

if __name__ == "__main__":
  main()
