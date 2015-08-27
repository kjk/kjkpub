workspace "chm"
  configurations { "Debug", "Release" }
  platforms { "x32", "x64" }
  startproject "test_chm"
  includedirs { "CHMLib/src"}

  project "test_chm"
    kind "ConsoleApp"
    language "C"
    files {
      "test_chm.c",
      "CHMLib/src/*"
    }
