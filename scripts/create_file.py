import sys

def ends_with_no_case_and_strip(s, suff_arr):
	s = s.lower()
	for suff in suff_arr:
		suff = suff.lower()
		if s.endswith(suff):
			s = s[:-len(suff)]
			return (s, True)
	return (s, False)
		
def file_size_human_decode(s):
	(s, is_kb) = ends_with_no_case_and_strip(s, ["k", "kb"])
	(s, is_mb) = ends_with_no_case_and_strip(s, ["m", "mb"])
	(s, is_gb) = ends_with_no_case_and_strip(s, ["g", "gb"])
	val = float(s)
	if is_kb: val = val * 1024
	if is_mb: val = val * 1024 * 1024
	if is_gb: val = val * 1024 * 1024 * 1024
	return int(val)

def usage_and_exit():
	print "Usage: create_file.py file_name file_size"

def main():
	if 3 != len(sys.argv):
		usage_and_exit()
	file_name = sys.argv[1]
	file_size = file_size_human_decode(sys.argv[2])
	fo = open(file_name, "wb")
	data = "a" * 4096
	data_size = len(data)
	while file_size > 0:
		to_write = min(file_size, data_size)
		fo.write(data[:to_write])
		file_size -= to_write
	fo.close()

if __name__ == "__main__":
	main()
