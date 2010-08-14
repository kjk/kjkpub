# Microsoft Developer Studio Project File - Name="TidyMyLib" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Static Library" 0x0104

CFG=TidyMyLib - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "TidyMyLib.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "TidyMyLib.mak" CFG="TidyMyLib - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "TidyMyLib - Win32 Release" (based on "Win32 (x86) Static Library")
!MESSAGE "TidyMyLib - Win32 Debug" (based on "Win32 (x86) Static Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
RSC=rc.exe

!IF  "$(CFG)" == "TidyMyLib - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "Release"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_MBCS" /D "_LIB" /YX /FD /c
# ADD CPP /nologo /MT /W4 /GX /O1 /D "WIN32" /D "NDEBUG" /D "_MBCS" /D "_LIB" /YX /FD /c
# ADD BASE RSC /l 0x1409 /d "NDEBUG"
# ADD RSC /l 0x1409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LIB32=link.exe -lib
# ADD BASE LIB32 /nologo
# ADD LIB32 /nologo /out:"..\output\TidyMyLib.lib"

!ELSEIF  "$(CFG)" == "TidyMyLib - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "Debug"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_MBCS" /D "_LIB" /YX /FD /GZ /c
# ADD CPP /nologo /W4 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_MBCS" /D "_LIB" /FR /YX /FD /GZ /c
# ADD BASE RSC /l 0x1409 /d "_DEBUG"
# ADD RSC /l 0x1409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LIB32=link.exe -lib
# ADD BASE LIB32 /nologo
# ADD LIB32 /nologo

!ENDIF 

# Begin Target

# Name "TidyMyLib - Win32 Release"
# Name "TidyMyLib - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\access.c
# End Source File
# Begin Source File

SOURCE=.\alloc.c
# End Source File
# Begin Source File

SOURCE=.\attrask.c
# End Source File
# Begin Source File

SOURCE=.\attrdict.c
# End Source File
# Begin Source File

SOURCE=.\attrget.c
# End Source File
# Begin Source File

SOURCE=.\attrs.c
# End Source File
# Begin Source File

SOURCE=.\buffio.c
# End Source File
# Begin Source File

SOURCE=.\charsets.c
# End Source File
# Begin Source File

SOURCE=.\clean.c
# End Source File
# Begin Source File

SOURCE=.\config.c
# End Source File
# Begin Source File

SOURCE=.\entities.c
# End Source File
# Begin Source File

SOURCE=.\fileio.c
# End Source File
# Begin Source File

SOURCE=.\iconvtc.c
# End Source File
# Begin Source File

SOURCE=.\istack.c
# End Source File
# Begin Source File

SOURCE=.\lexer.c
# End Source File
# Begin Source File

SOURCE=.\localize.c
# End Source File
# Begin Source File

SOURCE=.\parser.c
# End Source File
# Begin Source File

SOURCE=.\pprint.c
# End Source File
# Begin Source File

SOURCE=.\streamio.c
# End Source File
# Begin Source File

SOURCE=.\tagask.c
# End Source File
# Begin Source File

SOURCE=.\tags.c
# End Source File
# Begin Source File

SOURCE=.\tidylib.c
# End Source File
# Begin Source File

SOURCE=.\tmbstr.c
# End Source File
# Begin Source File

SOURCE=.\utf8.c
# End Source File
# Begin Source File

SOURCE=.\win32tc.c
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\access.h
# End Source File
# Begin Source File

SOURCE=.\attrdict.h
# End Source File
# Begin Source File

SOURCE=.\attrs.h
# End Source File
# Begin Source File

SOURCE=.\buffio.h
# End Source File
# Begin Source File

SOURCE=.\charsets.h
# End Source File
# Begin Source File

SOURCE=.\clean.h
# End Source File
# Begin Source File

SOURCE=.\config.h
# End Source File
# Begin Source File

SOURCE=.\entities.h
# End Source File
# Begin Source File

SOURCE=.\fileio.h
# End Source File
# Begin Source File

SOURCE=.\forward.h
# End Source File
# Begin Source File

SOURCE=.\iconvtc.h
# End Source File
# Begin Source File

SOURCE=.\lexer.h
# End Source File
# Begin Source File

SOURCE=.\message.h
# End Source File
# Begin Source File

SOURCE=.\parser.h
# End Source File
# Begin Source File

SOURCE=.\platform.h
# End Source File
# Begin Source File

SOURCE=.\pprint.h
# End Source File
# Begin Source File

SOURCE=.\streamio.h
# End Source File
# Begin Source File

SOURCE=.\tags.h
# End Source File
# Begin Source File

SOURCE=".\tidy-int.h"
# End Source File
# Begin Source File

SOURCE=.\tidy.h
# End Source File
# Begin Source File

SOURCE=.\tidyenum.h
# End Source File
# Begin Source File

SOURCE=.\tmbstr.h
# End Source File
# Begin Source File

SOURCE=.\utf8.h
# End Source File
# Begin Source File

SOURCE=.\win32tc.h
# End Source File
# End Group
# End Target
# End Project
