import sys

def usage_and_exit():
	print "Usage: create_file.py file_name file_size"

def main():
	if 3 != len(sys.argv):
		usage_and_exit()
	file_name = sys.argv[1]
	file_size = int(sys.argv[2])
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
