cl /nologo /Ox /Zi /D "NDEBUG" test_hash.c strings.c /link "/out:th.exe" 
cl /nologo /Od /Zi test_hash.c strings.c /link "/out:thd.exe"

