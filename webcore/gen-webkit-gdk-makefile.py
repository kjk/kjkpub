#!/usr/bin/env python
# Written by kjk (Krzysztof Kowalczyk)
#
# This script generates makefile(s) from a high-level description.
# The idea is to make it simpler to change the makefile when
# we want to change compiler flags or set of source files (simpler
# compared to updating makefile(s) manually).
# The biggest saving comes from not having to type long paths.
# Some tediousness of writing a makefile could be eliminated by
# clever use of advanced make features but my philosophy is
# to have a straitghtforward and simple makefile. It might be big
# but if it fails, it should be easy to tell where and why.
#
# Following this philosophy, each build variant gets its own
# makefile (as opposed to trying to do conditional stuff inside
# makefile). I call it XMI (eXtreme Makefile Inlining)
# 
# Makefile is a set of targets. Target is an executable or dll.
# Targets are build from C/C++ source files with a set of flags.
# Flags and set of files can be different for different targets.
#
# TODO:
# - make sure derived sources for javascript and webcore are created
# - experiment with minimal targets (not using xpath, xslt, no icu)
#
import string, sys, os, os.path

# Directory in which to create makefile, relative to top-level directory
MAKEFILE_PATH = "."

JSC_PATH = os.path.join(MAKEFILE_PATH, "JavaScriptCore")
WC_PATH = os.path.join(MAKEFILE_PATH, "WebCore")

all_source_files = {}

# TODO: consider using -D_FORTIFY_SOURCE=2 as described in http://kernelslacker.livejournal.com/54643.html

# Standard prelude present at the top of each makefile. Defines as much as possible
# of common defnitions between different makefiles
makefile_prelude_template = """# Description of major variables:
# OBJ_DIR - directory where temporary object files are put
# BIN_DIR - directory where final target (e.g. executable) is put

# JSC_DIR - directory where JavaScript sources live
JSC_DIR=%s
# WC_DIR - directory where WebCore sources live
WC_DIR=%s

# cpp definition used for all code (javascriptcore and webcore)
DEFS_COMMON=-D_THREAD_SAFE -DHAVE_FUNC_ISNAN

# include directories needed when compiling JavaScriptCore
JSC_INC=-I$(JSC_DIR) -I$(JSC_DIR)/wtf -I$(JSC_DIR)/kjs -I$(JSC_DIR)/pcre -I$(JSC_DIR)/bindings -I$(JSC_DIR)/bindings/c -I$(JSC_DIR)/DerivedSources/JavaScriptCore

# C/C++ flags common for all release targets
COMMON_REL_C_FLAGS=-g -Os -Wall -fno-strict-aliasing -fPIC -DPIC -pthread -DNDEBUG -fomit-frame-pointer
# C/C++ flags common for all debug targets
COMMON_DBG_C_FLAGS=-g -O0 -Wall -fno-strict-aliasing -fPIC -DPIC -pthread

JSC_REL_CFLAGS=$(COMMON_REL_C_FLAGS)
JSC_REL_CXXFLAGS=$(COMMON_REL_C_FLAGS) -fno-rtti -fno-exceptions

JSC_DBG_CFLAGS=$(COMMON_DBG_C_FLAGS)
JSC_DBG_CXXFLAGS=$(COMMON_DBG_C_FLAGS) -fno-rtti -fno-exceptions

WC_INC=-I$(JSC_DIR) -I$(WC_DIR) -I$(WC_DIR)/include\
 -I$(WC_DIR)/DerivedSources/WebCore -I$(WC_DIR)/bindings/js -I$(WC_DIR)/bridge\
 -I$(WC_DIR)/editing -I$(WC_DIR)/html -I$(WC_DIR)/css\
 -I$(WC_DIR)/dom -I$(WC_DIR)/loader -I$(WC_DIR)/loader/icon\
 -I$(WC_DIR)/page\
 -I$(WC_DIR)/platform\
 -I$(WC_DIR)/platform/graphics\
 -I$(WC_DIR)/platform/image-decoders -I$(WC_DIR)/platform/image-decoders/bmp\
 -I$(WC_DIR)/platform/image-decoders/gif -I$(WC_DIR)/platform/image-decoders/ico\
 -I$(WC_DIR)/platform/image-decoders/jpeg -I$(WC_DIR)/platform/image-decoders/xbm\
 -I$(WC_DIR)/platform/image-decoders/png -I$(WC_DIR)/platform/image-decoders/zlib\
 -I$(WC_DIR)/rendering -I$(WC_DIR)/xml -I$(WC_DIR)/kcanvas -I$(WC_DIR)/kcanvas/device\
 -I$(WC_DIR)/device/cairo -I$(WC_DIR)/platform/cairo\
 -I$(WC_DIR)/platform/gdk\
 -I$(WC_DIR)/platform/network -I$(WC_DIR)/platform/network/gdk

WC_COMMON_C_FLAGS=-DLINUX -DUSE_CONSERVATIVE_GC=0 -DBUILDING_CAIRO__ -DBUILDING_GDK__ %s
WC_COMMON_CXX_FLAGS=-fno-rtti -fno-exceptions `pkg-config --cflags freetype2` `pkg-config --cflags fontconfig` `pkg-config --cflags gtk+-2.0` `xml2-config --cflags` %s

WC_REL_CFLAGS=$(COMMON_REL_C_FLAGS) $(WC_COMMON_C_FLAGS)
WC_REL_CXXFLAGS=$(COMMON_REL_C_FLAGS) $(WC_COMMON_C_FLAGS) $(WC_COMMON_CXX_FLAGS)

WC_DBG_CFLAGS=$(COMMON_DBG_C_FLAGS) $(WC_COMMON_C_FLAGS)
WC_DBG_CXXFLAGS=$(COMMON_DBG_C_FLAGS) $(WC_COMMON_C_FLAGS) $(WC_COMMON_CXX_FLAGS)

# compilers, can be over-written for other targets
CXX = g++
CC = gcc

TESTKJS_LDFLAGS=`icu-config --ldflags`

WC_LDFLAGS=$(LDFLAGS)`icu-config --ldflags` `pkg-config --libs freetype2` `pkg-config --libs fontconfig` `pkg-config --libs gtk+-2.0` `curl-config --libs ` `pkg-config --libs cairo` `pkg-config --libs sqlite3` `xml2-config --libs` %s

"""

def gen_makefile_prelude(xpath_support=True, xslt_support=True):
  c_flags = ""
  defines = ""
  libs = ""
  if xpath_support:
    defines += " -DXPATH_SUPPORT=1"  
  if xslt_support:
    c_flags = "`xslt-config --cflags`"
    libs = "`xslt-config --libs`"
    defines += " -DXSLT_SUPPORT=1"
  return makefile_prelude_template % (JSC_PATH, WC_PATH, defines, c_flags, libs)

obj_dir_rule_txt = """
all: $(OBJ_DIR)

# TODO: fully correct only when OBJ_DIR and BIN_DIR are the same
# We really need 2 rules but make complains about duplicate
# rules *if* the dirs are the same
$(OBJ_DIR):
	@mkdir -p $(OBJ_DIR)
	@mkdir -p $(BIN_DIR)

"""

common_rel_flags = """
BIN_DIR=$(OBJ_DIR)
JSC_CFLAGS=$(JSC_REL_CFLAGS)
JSC_CXXFLAGS=$(JSC_REL_CXXFLAGS)
WC_CFLAGS=$(WC_REL_CFLAGS)
WC_CXXFLAGS=$(WC_REL_CXXFLAGS)
"""

common_dbg_flags = """
BIN_DIR=$(OBJ_DIR)
JSC_CFLAGS=$(JSC_DBG_CFLAGS)
JSC_CXXFLAGS=$(JSC_DBG_CXXFLAGS)
WC_CFLAGS=$(WC_DBG_CFLAGS)
WC_CXXFLAGS=$(WC_DBG_CXXFLAGS)
"""

# Standard prelude for Gdk Linux Release build target
gdk_rel_prelude = gen_makefile_prelude() + """
OBJ_DIR=obj-gdk-rel
""" + common_rel_flags

# Standard prelude for Gdk Linux Debug build target
gdk_dbg_prelude = gen_makefile_prelude() + """
OBJ_DIR=obj-gdk-dbg
""" + common_dbg_flags

gdk_no_xslt_rel_prelude = gen_makefile_prelude(True, False) + """
OBJ_DIR=obj-gdk-no-xslt-rel
""" + common_rel_flags

gdk_small_rel_prelude = gen_makefile_prelude(False, False) + """
OBJ_DIR=obj-gdk-small-rel
""" + common_rel_flags

gdk_small_dbg_prelude = gen_makefile_prelude(False, False) + """
OBJ_DIR=obj-gdk-small-dbg
""" + common_rel_flags

# This is included verbatim at the end of makefile
makefile_common_postlude = """
clean:
	rm -rf $(OBJ_DIR) $(BIN_DIR)

.PHONY: all clean

# Dependencies tracking
-include $(OBJ_DIR)/*.d
"""

def write_to_file(file_name, data):
  fo = open(file_name, "wb")
  fo.write(data)
  fo.close()

# Given a C/C++ source file name <file_name>, replace C/C++ suffix
# with a given <suffix>
def c_source_name_transform(file_name, suffix):
  for t in [".c", ".cpp", ".cc"]:
    if file_name.endswith(t):
      file_name = file_name[:-len(t)]
      return file_name + suffix
  print "'%s' has invalid suffix (should be '.c', '.cpp' or '.cc'" % file_name
  assert 0

# Get name of object file from C/C++ source <file_name>
def get_obj_name(file_name):
  return c_source_name_transform(file_name, ".o")

# Get name of dependency file from C/C++ source <file_name>
def get_dep_name(file_name):
  return c_source_name_transform(file_name, ".d")

# return True if <file_name> is a name of C source file
# (as opposed to C++) i.e. if the name ends with ".c"
def is_c_file(file_name): return file_name.endswith(".c")

# defines customizable info for C/C++ compiler invocations. syntactic sugar over a hash
class CompFlagsInfo:
  def __init__(self, prefix="", inc_dir="", c_flags="", cxx_flags=""):
    self.prefix = prefix
    self.inc_dir = inc_dir
    self.c_flags = c_flags
    self.cxx_flags = cxx_flags

# Info about one C/C++ file needed to generate makefile rule to compile it
# This info includes file name, which prefix to add to the object/dependency
# file (to avoid conflicting names for object files in case two different
# C/C++ files from different directories have the same name (since we put
# object files in one directory and base object file name only on the source
# file name, not directory it lives in))
class CFileRule:
  def __init__(self, dir, file_name, comp_flags_info):
    self.dir = dir
    self.file_name = file_name
    self.comp_flags_info = comp_flags_info
    assert comp_flags_info.prefix != ""
    self.file_path = os.path.join(self.dir, self.file_name)
    self.obj_name = "$(OBJ_DIR)/" + self.comp_flags_info.prefix + "_" + get_obj_name(self.file_name)

  def get_rule_as_txt(self):
    dep_name = self.comp_flags_info.prefix + "_" + get_dep_name(self.file_name)
    compiler_flags = self.comp_flags_info.inc_dir
    if is_c_file(self.file_name): 
      compiler = "$(CC)"
      compiler_flags += " " + self.comp_flags_info.c_flags
    else:
      compiler = "$(CXX)"
      compiler_flags += " " + self.comp_flags_info.cxx_flags
    txt = "%s: %s\n" % (self.obj_name, self.file_path)
    return txt + "	%s -c -o $@ $(DEFS_COMMON) %s -MT$@ -MF$(OBJ_DIR)/%s -MD $<\n\n" % (compiler, compiler_flags, dep_name)

(TARGET_EXE, TARGET_DLL) = range(2)

class Target:
  def __init__(self, target_name, target_type, ld_flags):
    self.target_name = target_name
    assert target_type in (TARGET_EXE, TARGET_DLL)
    assert target_type == TARGET_EXE # TODO: only TARGET_EXE supported at this time
    self.target_type = target_type
    self.ld_flags = ld_flags
    self.file_paths = []

  def define_files(self, dir, to_exclude = []):
    global all_source_files
    files = all_source_files[dir]
    files = [file for file in files if file not in to_exclude]
    for file in files:
      path = os.path.join(dir, file)
      assert path not in self.file_paths
      self.file_paths.append(path)

def get_target_object_list_name(target_name):
  return (target_name + "_OBJECTS").upper()

# Gathers information needed to generate a makefile i.e. rule for each
# source file and targets
class MakefileInfo:
  def __init__(self):
    self.all_c_files = {} # key is file path, value is CFileRule
    self.targets = {} # key is target name, value is Target

  def define_files(self, dir, comp_flags_info):
    global all_source_files
    all_files = all_source_files[dir]
    for file in all_files:
      rule = CFileRule(dir, file, comp_flags_info)
      path = rule.file_path
      assert path not in self.targets
      self.all_c_files[path] = rule

  def define_files_multiple(self, dir_list, comp_flags_info):
    for dir in dir_list:
      self.define_files(dir, comp_flags_info)

  def define_target(self, target_name, target_type=TARGET_EXE, ld_flags = ""):
    assert target_name not in self.targets
    self.targets[target_name] = Target(target_name, target_type, ld_flags)

  def target_add_files(self, target_name, dir, to_exclude = []):
    target = self.targets[target_name]
    target.define_files(dir, to_exclude)

  def target_add_files_multiple(self, target_name, files):
    for (dir, to_exclude) in files:
      target = self.targets[target_name]
      target.define_files(dir, to_exclude)

  # get compilation rules for all files that have been used in at least one target
  def get_rules_as_txt(self):
    files_used = {} # key is file path
    assert len(self.targets) > 0
    for target in self.targets.values():
      for path in target.file_paths:
        files_used[path] = 1
    file_paths = files_used.keys()
    # TODO: sort by directory/file name
    rules = [self.all_c_files[path].get_rule_as_txt() for path in file_paths]
    return string.join(rules, "")

  def get_target_as_txt(self, target_name):
    target = self.targets[target_name]
    obj_list_name = get_target_object_list_name(target_name)
    txt = "$(BIN_DIR)/%s: $(%s)\n" % (target_name, obj_list_name)
    txt += "	$(CXX) -o $@ $(%s) %s -pthread\n" % (obj_list_name, target.ld_flags)
    return txt

  def get_target_object_list(self, target_name):
    target = self.targets[target_name]
    obj_list_name = get_target_object_list_name(target_name)
    txt = ["%s =" % obj_list_name]
    for file_path in target.file_paths:
      rule = self.all_c_files[file_path]
      txt.append("	%s" % rule.obj_name)
    return string.join(txt, " \\\n") + "\n\n"

  def get_makefile_as_txt(self):
    targets_obj_lists_txt = ""
    # first generate object lists for each target
    for target in self.targets.keys():
      targets_obj_lists_txt += self.get_target_object_list(target)
    rules_txt = self.get_rules_as_txt()
    targets_txt = ""
    for target_name in self.targets.keys():
      targets_txt += self.get_target_as_txt(target_name) + "\n"
    target_names = ["$(BIN_DIR)/" + target_name for target_name in self.targets.keys()]
    all_target_txt = "all: $(OBJ_DIR) $(BIN_DIR) " + string.join(target_names, " ") + "\n\n"
    return targets_obj_lists_txt + obj_dir_rule_txt + all_target_txt + targets_txt + rules_txt

def define_jsc_files(mfInfo):
  jsc_comp_flags_info = CompFlagsInfo(prefix="jsc", inc_dir="$(JSC_INC)", c_flags="$(JSC_CFLAGS)", cxx_flags="$(JSC_CXXFLAGS)")
  jsc_dirs = [
    './JavaScriptCore/wtf', 
    './JavaScriptCore/kjs',
    './JavaScriptCore/pcre',
    './JavaScriptCore/DerivedSources/JavaScriptCore',
    './JavaScriptCore/bindings',
    './JavaScriptCore/bindings/c']
  mfInfo.define_files_multiple(jsc_dirs, jsc_comp_flags_info)

# testkjs target is made of these:
testkjs_files = [
  ['./JavaScriptCore/wtf', []],
  ['./JavaScriptCore/kjs', []],
  ['./JavaScriptCore/pcre', ['ucptable.c', 'dftables.c']],
  ['./JavaScriptCore/DerivedSources/JavaScriptCore', ['chartables.c']],
  ['./JavaScriptCore/bindings', ['testbindings.cpp']],
  ['./JavaScriptCore/bindings/c', []]]

testkjs = "testkjs"
def define_testkjs_target(mfInfo):
  mfInfo.define_target(testkjs, ld_flags="$(TESTKJS_LDFLAGS)")
  mfInfo.target_add_files_multiple(testkjs, testkjs_files)

def define_wc_files(mfInfo):
  wc_comp_flags_info = CompFlagsInfo(prefix="wc", inc_dir="$(WC_INC)", c_flags="$(WC_CFLAGS)", cxx_flags="$(WC_CXXFLAGS)")
  wc_dirs = [
    './WebCore/bindings/js',
#    './WebCore/bindings/objc',
#    './WebCore/bridge/win',
    './WebCore/css',
    './WebCore/DerivedSources/WebCore', 
    './WebCore/dom', 
    './WebCore/editing',
    './WebCore/history',
    './WebCore/html',
    './WebCore/ksvg2/css', 
    './WebCore/ksvg2/events',
    './WebCore/ksvg2/misc',
    './WebCore/ksvg2/svg',
    './WebCore/loader',
    './WebCore/loader/gdk',
    './WebCore/loader/icon', 
    './WebCore/page',
    './WebCore/page/gdk',
    './WebCore/platform',
    './WebCore/platform/gdk',
    './WebCore/platform/graphics',
    './WebCore/platform/graphics/cairo',
    './WebCore/platform/graphics/gdk',
    './WebCore/platform/image-decoders/bmp',
    './WebCore/platform/image-decoders/gif',
    './WebCore/platform/image-decoders/ico',
    './WebCore/platform/image-decoders/jpeg',
    './WebCore/platform/image-decoders/png',
    './WebCore/platform/image-decoders/xbm',
    './WebCore/platform/image-decoders/zlib',
    './WebCore/platform/network',
    './WebCore/platform/network/gdk',
    './WebCore/rendering',
    './WebCore/xml',
    './WebKitTools/GdkLauncher']
  mfInfo.define_files_multiple(wc_dirs, wc_comp_flags_info)

# gdklauncher target is made of these:
gdklauncher_files = [
  ['./JavaScriptCore/DerivedSources/JavaScriptCore', ['chartables.c']],
  ['./JavaScriptCore/bindings', ['testbindings.cpp']],
  ['./JavaScriptCore/bindings/c', []],
  ['./JavaScriptCore/kjs', ['testkjs.cpp']],
  ['./JavaScriptCore/pcre', ['ucptable.c', 'dftables.c']],
  ['./JavaScriptCore/wtf', []],
  ['./WebCore/DerivedSources/WebCore', ['CSSPropertyNames.c', 'CSSValueKeywords.c', 'CharsetData.cpp', 'DocTypeStrings.cpp', 'JSHTMLInputElementBaseTable.cpp', 'JSSVGZoomEvent.cpp', 'JSSVGAElement.cpp', 'JSSVGAngle.cpp', 'JSSVGAnimatedAngle.cpp', 'JSSVGAnimateColorElement.cpp', 'JSSVGAnimateElement.cpp', 'JSSVGAnimateTransformElement.cpp', 'JSSVGAnimatedBoolean.cpp', 'JSSVGAnimatedEnumeration.cpp', 'JSSVGAnimatedInteger.cpp', 'JSSVGAnimatedLength.cpp', 'JSSVGAnimatedLengthList.cpp', 'JSSVGAnimatedNumber.cpp', 'JSSVGAnimatedNumberList.cpp', 'JSSVGAnimatedPoints.cpp', 'JSSVGAnimatedPreserveAspectRatio.cpp', 'JSSVGAnimatedRect.cpp', 'JSSVGAnimatedString.cpp', 'JSSVGAnimatedTransformList.cpp', 'JSSVGAnimationElement.cpp', 'JSSVGColor.cpp', 'JSSVGCircleElement.cpp', 'JSSVGClipPathElement.cpp', 'JSSVGComponentTransferFunctionElement.cpp', 'JSSVGCursorElement.cpp', 'JSSVGDefsElement.cpp', 'JSSVGDescElement.cpp', 'JSSVGDocument.cpp', 'JSSVGLength.cpp', 'JSSVGMatrix.cpp', 'JSSVGMetadataElement.cpp', 'JSSVGPathElement.cpp', 'JSSVGPathSeg.cpp', 'JSSVGPathSegArcAbs.cpp', 'JSSVGPathSegArcRel.cpp', 'JSSVGPathSegClosePath.cpp', 'JSSVGPathSegCurvetoCubicAbs.cpp', 'JSSVGPathSegCurvetoCubicRel.cpp', 'JSSVGPathSegCurvetoCubicSmoothAbs.cpp', 'JSSVGPathSegCurvetoCubicSmoothRel.cpp', 'JSSVGPathSegCurvetoQuadraticAbs.cpp', 'JSSVGPathSegCurvetoQuadraticRel.cpp', 'JSSVGPathSegCurvetoQuadraticSmoothAbs.cpp', 'JSSVGPathSegCurvetoQuadraticSmoothRel.cpp', 'JSSVGPathSegLinetoAbs.cpp', 'JSSVGPathSegLinetoHorizontalAbs.cpp', 'JSSVGPathSegLinetoHorizontalRel.cpp', 'JSSVGPathSegLinetoRel.cpp', 'JSSVGPathSegLinetoVerticalAbs.cpp', 'JSSVGPathSegLinetoVerticalRel.cpp', 'JSSVGPathSegMovetoAbs.cpp', 'JSSVGPathSegMovetoRel.cpp', 'JSSVGNumberList.cpp', 'JSSVGPaint.cpp', 'JSSVGPathSegList.cpp', 'JSSVGPatternElement.cpp', 'JSSVGPointList.cpp', 'JSSVGPolygonElement.cpp', 'JSSVGPolylineElement.cpp', 'JSSVGRadialGradientElement.cpp', 'JSSVGRectElement.cpp', 'JSSVGRenderingIntent.cpp', 'JSSVGSetElement.cpp', 'JSSVGScriptElement.cpp', 'JSSVGStyleElement.cpp', 'JSSVGSwitchElement.cpp', 'JSSVGStopElement.cpp', 'JSSVGStringList.cpp', 'JSSVGSymbolElement.cpp', 'JSSVGTRefElement.cpp', 'JSSVGTSpanElement.cpp', 'JSSVGTextElement.cpp', 'JSSVGTextContentElement.cpp', 'JSSVGTextPositioningElement.cpp', 'JSSVGTitleElement.cpp', 'JSSVGTransform.cpp', 'JSSVGTransformList.cpp', 'JSSVGUnitTypes.cpp', 'JSSVGUseElement.cpp', 'JSSVGViewElement.cpp', 'JSSVGPointTable.cpp', 'JSSVGPreserveAspectRatio.cpp', 'JSSVGRectTable.cpp', 'JSSVGElement.cpp', 'JSSVGSVGElement.cpp', 'JSSVGEllipseElement.cpp', 'JSSVGFEBlendElement.cpp', 'JSSVGFEColorMatrixElement.cpp', 'JSSVGFEComponentTransferElement.cpp', 'JSSVGFECompositeElement.cpp', 'JSSVGFEDiffuseLightingElement.cpp', 'JSSVGFEDisplacementMapElement.cpp', 'JSSVGFEDistantLightElement.cpp', 'JSSVGFEFloodElement.cpp', 'JSSVGFEFuncAElement.cpp', 'JSSVGFEFuncBElement.cpp', 'JSSVGFEFuncGElement.cpp', 'JSSVGFEFuncRElement.cpp', 'JSSVGFEGaussianBlurElement.cpp', 'JSSVGFEImageElement.cpp', 'JSSVGFEMergeElement.cpp', 'JSSVGFEMergeNodeElement.cpp', 'JSSVGFEOffsetElement.cpp', 'JSSVGFEPointLightElement.cpp', 'JSSVGFESpecularLightingElement.cpp', 'JSSVGFESpotLightElement.cpp', 'JSSVGFETileElement.cpp', 'JSSVGFETurbulenceElement.cpp', 'JSSVGFilterElement.cpp', 'JSSVGForeignObjectElement.cpp', 'JSSVGGElement.cpp', 'JSSVGGradientElement.cpp', 'JSSVGImageElement.cpp', 'JSSVGLengthList.cpp', 'JSSVGLineElement.cpp', 'JSSVGLinearGradientElement.cpp', 'JSSVGMaskElement.cpp', 'JSSVGMarkerElement.cpp', 'SVGElementFactory.cpp', 'SVGNames.cpp', 'tokenizer.cpp']],
  ['./WebCore/bindings/js', []],
  ['./WebCore/css', []],
  ['./WebCore/dom', []],
  ['./WebCore/editing', []],
  ['./WebCore/history', []],
  ['./WebCore/html', []],
  ['./WebCore/loader', ['CachedXBLDocument.cpp', 'DocumentLoader.cpp']],
  ['./WebCore/loader/gdk', []],
  ['./WebCore/loader/icon', []],
  ['./WebCore/page', []],
  ['./WebCore/page/gdk', []],
  ['./WebCore/platform', ['CharsetNames.cpp']],
  ['./WebCore/platform/gdk', []],
  ['./WebCore/platform/graphics', []],
  ['./WebCore/platform/graphics/cairo', []],
  ['./WebCore/platform/graphics/gdk', []],
  ['./WebCore/platform/network', []],
  ['./WebCore/platform/network/gdk', []],
  ['./WebCore/platform/image-decoders/bmp', []],
  ['./WebCore/platform/image-decoders/gif', []],
  ['./WebCore/platform/image-decoders/ico', []],
  ['./WebCore/platform/image-decoders/jpeg', []],
  ['./WebCore/platform/image-decoders/png', []],
  ['./WebCore/platform/image-decoders/xbm', []],
  ['./WebCore/platform/image-decoders/zlib', []],
  ['./WebCore/rendering', ['RenderFileButton.cpp', 'RenderThemeWin.cpp']],
  ['./WebCore/xml', []],
  ['./WebKitTools/GdkLauncher', []],
]

gdklauncher = "gdklauncher"

def define_gdklauncher_target(mfInfo):
  mfInfo.define_target(gdklauncher, ld_flags="$(WC_LDFLAGS)")
  mfInfo.target_add_files_multiple(gdklauncher, gdklauncher_files)

def gen_makefile(prelude_txt, makefile_name):
  mfInfo = MakefileInfo()
  define_jsc_files(mfInfo)
  define_wc_files(mfInfo)
  define_testkjs_target(mfInfo)
  define_gdklauncher_target(mfInfo)
  txt = mfInfo.get_makefile_as_txt()
  makefile_txt = prelude_txt + txt + makefile_common_postlude
  #print makefile_txt
  write_to_file(os.path.join(MAKEFILE_PATH, makefile_name), makefile_txt)

# Generate gdk release target
def gen_gdk_rel():
  gen_makefile(gdk_rel_prelude, "Makefile.gdk.rel")

# Generate gdk debug target
def gen_gdk_dbg():
  gen_makefile(gdk_dbg_prelude, "Makefile.gdk.dbg")

# Generate gdk small (no xpath and no xslt) release target
def gen_gdk_no_xslt_rel():
  gen_makefile(gdk_no_xslt_rel_prelude, "Makefile.gdk.no_xslt.rel")

# Generate gdk small (no xpath and no xslt) debug target
def gen_gdk_small_dbg():
  gen_makefile(gdk_small_dbg_prelude, "Makefile.gdk.small.dbg")

# Find the name of top-level WebCore directory, relative to
# current working directory. Only works if current directory
# is top-level or below
def find_toplevel_dir():
  cur_dir = "."
  depth = 0
  while depth < 5:
    if os.path.isdir(os.path.join(cur_dir, "WebCore")):
      return cur_dir
    cur_dir = os.path.join(cur_dir, "..")
  return None

def is_source_file(file_name):
  for ext in [".c", ".cpp", ".cc"]:
    if file_name.endswith(ext): return True
  return False

def find_c_source_files(start_dir):
  dirs = {}
  dirs_to_visit = [start_dir]
  while len(dirs_to_visit) > 0:
    files_in_dir = []
    dir = dirs_to_visit[0]
    dirs_to_visit = dirs_to_visit[1:]
    entries = os.listdir(dir)
    for entry in entries:
      path = os.path.join(dir, entry)
      if os.path.isdir(path):
        dirs_to_visit.append(path)
      elif os.path.isfile(path):
        files_in_dir.append(entry)
      else:
        # don't know what else could it be
        assert 0
    source_files = [file for file in files_in_dir if is_source_file(file)]
    if len(source_files) > 0:
      dirs[dir] = source_files
  return dirs

def main():
  global all_source_files
  toplevel_dir = find_toplevel_dir()
  os.chdir(toplevel_dir)
  all_source_files = find_c_source_files(".")
  #dirs = all_source_files.keys().sort()
  #print string.join(dirs, "\n")

  gen_gdk_rel()
  gen_gdk_dbg()
  gen_gdk_no_xslt_rel()
  #gen_gdk_small_dbg()

if __name__ == "__main__":
  main()
