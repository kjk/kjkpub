# TARGET can  be: dbg, rel-o2 (synonim for rel), rel-os, rel (like rel-o2)
TARGET?=dbg

MAKEFILE=Makefile.tmpl

CFLAGS=-Wall -g

ifeq (dbg, ${TARGET})
OUTDIR=obj-dbg
CFLAGS+=-O0
TARGET_SET=yes
endif

ifeq (rel-o2, ${TARGET})
OUTDIR=obj-rel-o2
CFLAGS+=-O2 -DNDEBUG
TARGET_SET=yes
endif

ifeq (relo2, ${TARGET})
OUTDIR=obj-rel-o2
CFLAGS+=-O2 -DNDEBUG
TARGET_SET=yes
endif

ifeq (rel-os, ${TARGET})
OUTDIR=obj-rel-os
CFLAGS=-Os -DNDEBUG
TARGET_SET=yes
endif

ifeq (relos, ${TARGET})
OUTDIR=obj-rel-os
CFLAGS+=-Os -DNDEBUG
TARGET_SET=yes
endif

# TODO: if TARGET_SET is not yes, print an error message and exit

export CFLAGS

.PHONY: all clean

all: $(OUTDIR)/Makefile
	cd $(OUTDIR); $(MAKE) all

$(OUTDIR)/Makefile: $(OUTDIR) force
	cp $(MAKEFILE) $(OUTDIR)/Makefile

$(OUTDIR):
	@mkdir -p $(OUTDIR)

clean: $(OUTDIR)/Makefile
	cd $(OUTDIR); make clean

force: ;
