# Make and install tzdb code and data.

# This file is in the public domain, so clarified as of
# 2009-05-17 by Arthur David Olson.

# Package name for the code distribution.
PACKAGE=	tzcode

# Version number for the distribution, overridden in the 'tarballs' rule below.
VERSION=	unknown

# Email address for bug reports.
BUGEMAIL=	tz@iana.org

DATAFORM=		main

LOCALTIME=	Factory

POSIXRULES=	-

DESTDIR =

TOPDIR =

TZDEFAULT = $(TOPDIR)/etc/localtime

USRDIR = usr
USRSHAREDIR = $(USRDIR)/share

TZDIR_BASENAME=	zoneinfo
TZDIR = $(TOPDIR)/$(USRSHAREDIR)/$(TZDIR_BASENAME)

# The "tzselect" and (if you do "make INSTALL") "date" commands go in:
BINDIR = $(TOPDIR)/$(USRDIR)/bin

# The "zdump" command goes in:
ZDUMPDIR = $(BINDIR)

# The "zic" command goes in:
ZICDIR = $(TOPDIR)/$(USRDIR)/sbin

# Manual pages go in subdirectories of. . .
MANDIR = $(TOPDIR)/$(USRSHAREDIR)/man

# Library functions are put in an archive in LIBDIR.
LIBDIR = $(TOPDIR)/$(USRDIR)/lib


# Types to try, as an alternative to time_t.
TIME_T_ALTERNATIVES = $(TIME_T_ALTERNATIVES_HEAD) $(TIME_T_ALTERNATIVES_TAIL)
TIME_T_ALTERNATIVES_HEAD = int_least64_t
TIME_T_ALTERNATIVES_TAIL = int_least32_t uint_least32_t uint_least64_t

REDO=		posix_right

EXPIRES_LINE=	0

# To install data in text form that has all the information of the TZif data,
# (optionally incorporating leap second information), use
#	TZDATA_TEXT=	tzdata.zi leapseconds
# To install text data without leap second information (e.g., because
# REDO='posix_only'), use
#	TZDATA_TEXT=	tzdata.zi
# To avoid installing text data, use
#	TZDATA_TEXT=

TZDATA_TEXT=	leapseconds tzdata.zi

# For backward-compatibility links for old zone names, use
#	BACKWARD=	backward
# To omit these links, use
#	BACKWARD=

BACKWARD=	backward

# If you want out-of-scope and often-wrong data from the file 'backzone',
# but only for entries listed in the backward-compatibility file zone.tab, use
#	PACKRATDATA=	backzone
#	PACKRATLIST=	zone.tab
# If you want all the 'backzone' data, use
#	PACKRATDATA=	backzone
#	PACKRATLIST=
# To omit this data, use
#	PACKRATDATA=
#	PACKRATLIST=

PACKRATDATA=
PACKRATLIST=

# The name of a locale using the UTF-8 encoding, used during self-tests.
# The tests are skipped if the name does not appear to work on this system.

UTF8_LOCALE=	en_US.utf8

# Non-default libraries needed to link.
# On some hosts, this should have -lintl unless CFLAGS has -DHAVE_GETTEXT=0.
LDLIBS=


# Select instrumentation via "make GCC_INSTRUMENT='whatever'".
GCC_INSTRUMENT = \
  -fsanitize=undefined -fsanitize-address-use-after-scope \
  -fsanitize-undefined-trap-on-error -fstack-protector
# Omit -fanalyzer from GCC_DEBUG_FLAGS, as it makes GCC too slow.
GCC_DEBUG_FLAGS = -DGCC_LINT -g3 -O3 -fno-common \
  $(GCC_INSTRUMENT) \
  -Wall -Wextra \
  -Walloc-size-larger-than=100000 -Warray-bounds=2 \
  -Wbad-function-cast -Wbidi-chars=any,ucn -Wcast-align=strict -Wdate-time \
  -Wdeclaration-after-statement -Wdouble-promotion \
  -Wduplicated-branches -Wduplicated-cond \
  -Wformat=2 -Wformat-overflow=2 -Wformat-signedness -Wformat-truncation \
  -Wimplicit-fallthrough=5 -Winit-self -Wlogical-op \
  -Wmissing-declarations -Wmissing-prototypes -Wnested-externs \
  -Wnull-dereference \
  -Wold-style-definition -Woverlength-strings -Wpointer-arith \
  -Wshadow -Wshift-overflow=2 -Wstrict-overflow \
  -Wstrict-prototypes -Wstringop-overflow=4 \
  -Wstringop-truncation -Wsuggest-attribute=cold \
  -Wsuggest-attribute=const -Wsuggest-attribute=format \
  -Wsuggest-attribute=malloc \
  -Wsuggest-attribute=noreturn -Wsuggest-attribute=pure \
  -Wtrampolines -Wundef -Wuninitialized -Wunused-macros -Wuse-after-free=3 \
  -Wvariadic-macros -Wvla -Wwrite-strings \
  -Wno-address -Wno-format-nonliteral -Wno-sign-compare \
  -Wno-type-limits



CFLAGS=

# Linker flags.  Default to $(LFLAGS) for backwards compatibility
# to release 2012h and earlier.

LDFLAGS=	$(LFLAGS)

# For leap seconds, this Makefile uses LEAPSECONDS='-L leapseconds' in
# submake command lines.  The default is no leap seconds.

LEAPSECONDS=

# The zic command and its arguments.

zic=		./zic
ZIC=		$(zic) $(ZFLAGS)


ZFLAGS=

# How to use zic to install TZif files.

ZIC_INSTALL=	$(ZIC) -d '$(DESTDIR)$(TZDIR)' $(LEAPSECONDS)


AWK=		awk


KSHELL=		/bin/bash


CURL=		curl


GPG=		gpg

# This expensive test requires USE_LTZ.
# To suppress it, define this macro to be empty.
CHECK_TIME_T_ALTERNATIVES = check_time_t_alternatives


TAB_CHAR=	'	'
SAFE_CHARSET1=	$(TAB_CHAR)' !\"'$$sharp'$$%&'\''()*+,./0123456789:;<=>?@'
SAFE_CHARSET2=	'ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\^_`'
SAFE_CHARSET3=	'abcdefghijklmnopqrstuvwxyz{|}~'
SAFE_CHARSET=	$(SAFE_CHARSET1)$(SAFE_CHARSET2)$(SAFE_CHARSET3)
SAFE_CHAR=	'[]'$(SAFE_CHARSET)'-]'


UNUSUAL_OK_LATIN_1 = ¡¢£¤¥¦§¨©«¬®¯°±²³´¶·¸¹»¼½¾¿×÷
# Non-ASCII non-letters that OK_CHAR allows, as these characters are
# useful in commentary.
UNUSUAL_OK_CHARSET= $(UNUSUAL_OK_LATIN_1)


s = [:space:]


OK_CHAR=	'[][:alpha:]$(UNUSUAL_OK_CHARSET)'$(SAFE_CHARSET)'-]'

# SAFE_LINE matches a line of safe characters.
# SAFE_SHARP_LINE is similar, except any OK character can follow '#';
# this is so that comments can contain non-ASCII characters.
# OK_LINE matches a line of OK characters.
SAFE_LINE=	'^'$(SAFE_CHAR)'*$$'
SAFE_SHARP_LINE='^'$(SAFE_CHAR)'*('$$sharp$(OK_CHAR)'*)?$$'
OK_LINE=	'^'$(OK_CHAR)'*$$'

# Flags to give 'tar' when making a distribution.
# Try to use flags appropriate for GNU tar.
GNUTARFLAGS= --format=pax --pax-option='delete=atime,delete=ctime' \
  --numeric-owner --owner=0 --group=0 \
  --mode=go+u,go-w --sort=name
TARFLAGS=	`if tar $(GNUTARFLAGS) --version >/dev/null 2>&1; \
		 then echo $(GNUTARFLAGS); \
		 else :; \
		 fi`

# Flags to give 'gzip' when making a distribution.
GZIPFLAGS=	-9n

# When comparing .tzs files, use GNU diff's -F'^TZ=' option if supported.
# This makes it easier to see which Zone has been affected.
DIFF_TZS=	 diff -u$$(! diff -u -F'^TZ=' - - <>/dev/null >&0 2>&1 \
			   || echo ' -F^TZ=')

###############################################################################

#MAKE=		make

cc=		cc
CC=		$(cc) -DTZDIR='"$(TZDIR)"'

AR=		ar

# ':' on typical hosts; 'ranlib' on the ancient hosts that still need ranlib.
RANLIB=		:

TZCOBJS=	zic.o
TZDOBJS=	zdump.o localtime.o asctime.o strftime.o
DATEOBJS=	date.o localtime.o strftime.o asctime.o
LIBSRCS=	localtime.c asctime.c difftime.c strftime.c
LIBOBJS=	localtime.o asctime.o difftime.o strftime.o
HEADERS=	tzfile.h private.h
NONLIBSRCS=	zic.c zdump.c
NEWUCBSRCS=	date.c
SOURCES=	$(HEADERS) $(LIBSRCS) $(NONLIBSRCS) $(NEWUCBSRCS) \
			tzselect.ksh workman.sh
MANS=		newctime.3 newstrftime.3 newtzset.3 time2posix.3 \
			tzfile.5 tzselect.8 zic.8 zdump.8
MANTXTS=	newctime.3.txt newstrftime.3.txt newtzset.3.txt \
			time2posix.3.txt \
			tzfile.5.txt tzselect.8.txt zic.8.txt zdump.8.txt \
			date.1.txt
COMMON=		calendars CONTRIBUTING LICENSE Makefile \
			NEWS README SECURITY theory.html version
WEB_PAGES=	tz-art.html tz-how-to.html tz-link.html
CHECK_WEB_PAGES=check_theory.html check_tz-art.html \
			check_tz-how-to.html check_tz-link.html
DOCS=		$(MANS) date.1 $(MANTXTS) $(WEB_PAGES)
PRIMARY_YDATA=	africa antarctica asia australasia \
		europe northamerica southamerica
YDATA=		$(PRIMARY_YDATA) etcetera
NDATA=		factory
TDATA_TO_CHECK=	$(YDATA) $(NDATA) backward
TDATA=		$(YDATA) $(NDATA) $(BACKWARD)
ZONETABLES=	zone1970.tab zone.tab
TABDATA=	iso3166.tab $(TZDATA_TEXT) $(ZONETABLES)
LEAP_DEPS=	leapseconds.awk leap-seconds.list
TZDATA_ZI_DEPS=	ziguard.awk zishrink.awk version $(TDATA) \
		  $(PACKRATDATA) $(PACKRATLIST)
DSTDATA_ZI_DEPS= ziguard.awk $(TDATA) $(PACKRATDATA) $(PACKRATLIST)
DATA=		$(TDATA_TO_CHECK) backzone iso3166.tab leap-seconds.list \
			leapseconds $(ZONETABLES)
AWK_SCRIPTS=	checklinks.awk checktab.awk leapseconds.awk \
			ziguard.awk zishrink.awk
MISC=		$(AWK_SCRIPTS)
TZS_YEAR=	2050
TZS_CUTOFF_FLAG=	-c $(TZS_YEAR)
TZS=		to$(TZS_YEAR).tzs
TZS_NEW=	to$(TZS_YEAR)new.tzs
TZS_DEPS=	$(YDATA) asctime.c localtime.c \
			private.h tzfile.h zdump.c zic.c
TZDATA_DIST = $(COMMON) $(DATA) $(MISC)
# EIGHT_YARDS is just a yard short of the whole ENCHILADA.
EIGHT_YARDS = $(TZDATA_DIST) $(DOCS) $(SOURCES) tzdata.zi
ENCHILADA = $(EIGHT_YARDS) $(TZS)

# Consult these files when deciding whether to rebuild the 'version' file.
# This list is not the same as the output of 'git ls-files', since
# .gitignore is not distributed.
VERSION_DEPS= \
		calendars CONTRIBUTING LICENSE Makefile NEWS README SECURITY \
		africa antarctica asctime.c asia australasia \
		backward backzone \
		checklinks.awk checktab.awk \
		date.1 date.c difftime.c \
		etcetera europe factory iso3166.tab \
		leap-seconds.list leapseconds.awk localtime.c \
		newctime.3 newstrftime.3 newtzset.3 northamerica \
		private.h southamerica strftime.c theory.html \
		time2posix.3 tz-art.html tz-how-to.html tz-link.html \
		tzfile.5 tzfile.h tzselect.8 tzselect.ksh \
		workman.sh zdump.8 zdump.c zic.8 zic.c \
		ziguard.awk zishrink.awk \
		zone.tab zone1970.tab

# And for the benefit of csh users on systems that assume the user
# shell should be used to handle commands in Makefiles. . .

SHELL=		/bin/sh

all:		tzselect zic zdump libtz.a $(TABDATA) \
		  vanguard.zi main.zi rearguard.zi

ALL:		all date $(ENCHILADA)

install:	all $(DATA) $(REDO) $(MANS)
		mkdir -p '$(DESTDIR)$(BINDIR)' \
			'$(DESTDIR)$(ZDUMPDIR)' '$(DESTDIR)$(ZICDIR)' \
			'$(DESTDIR)$(LIBDIR)' \
			'$(DESTDIR)$(MANDIR)/man3' '$(DESTDIR)$(MANDIR)/man5' \
			'$(DESTDIR)$(MANDIR)/man8'
		$(ZIC_INSTALL) -l $(LOCALTIME) \
			`case '$(POSIXRULES)' in ?*) echo '-p';; esac \
			` $(POSIXRULES) \
			-t '$(DESTDIR)$(TZDEFAULT)'
		cp -f $(TABDATA) '$(DESTDIR)$(TZDIR)/.'
		cp tzselect '$(DESTDIR)$(BINDIR)/.'
		cp zdump '$(DESTDIR)$(ZDUMPDIR)/.'
		cp zic '$(DESTDIR)$(ZICDIR)/.'
		cp libtz.a '$(DESTDIR)$(LIBDIR)/.'
		$(RANLIB) '$(DESTDIR)$(LIBDIR)/libtz.a'
		cp -f newctime.3 newtzset.3 '$(DESTDIR)$(MANDIR)/man3/.'
		cp -f tzfile.5 '$(DESTDIR)$(MANDIR)/man5/.'
		cp -f tzselect.8 zdump.8 zic.8 '$(DESTDIR)$(MANDIR)/man8/.'

INSTALL:	ALL install date.1
		mkdir -p '$(DESTDIR)$(BINDIR)' '$(DESTDIR)$(MANDIR)/man1'
		cp date '$(DESTDIR)$(BINDIR)/.'
		cp -f date.1 '$(DESTDIR)$(MANDIR)/man1/.'

# Calculate version number from git, if available.
# Otherwise, use $(VERSION) unless it is "unknown" and there is already
# a 'version' file, in which case reuse the existing 'version' contents
# and append "-dirty" if the contents do not already end in "-dirty".
version:	$(VERSION_DEPS)
		{ (type git) >/dev/null 2>&1 && \
		  V=`git describe --match '[0-9][0-9][0-9][0-9][a-z]*' \
				--abbrev=7 --dirty` || \
		  if test '$(VERSION)' = unknown && V=`cat $@`; then \
		    case $$V in *-dirty);; *) V=$$V-dirty;; esac; \
		  else \
		    V='$(VERSION)'; \
		  fi; } && \
		printf '%s\n' "$$V" >$@.out
		mv $@.out $@

# These files can be tailored by setting BACKWARD, PACKRATDATA, PACKRATLIST.
vanguard.zi main.zi rearguard.zi: $(DSTDATA_ZI_DEPS)
		$(AWK) \
		  -v DATAFORM=`expr $@ : '\(.*\).zi'` \
		  -v PACKRATDATA='$(PACKRATDATA)' \
		  -v PACKRATLIST='$(PACKRATLIST)' \
		  -f ziguard.awk \
		  $(TDATA) $(PACKRATDATA) >$@.out
		mv $@.out $@
# This file has a version comment that attempts to capture any tailoring
# via BACKWARD, DATAFORM, PACKRATDATA, PACKRATLIST, and REDO.
tzdata.zi:	$(DATAFORM).zi version zishrink.awk
		version=`sed 1q version` && \
		  LC_ALL=C $(AWK) \
		    -v dataform='$(DATAFORM)' \
		    -v deps='$(DSTDATA_ZI_DEPS) zishrink.awk' \
		    -v redo='$(REDO)' \
		    -v version="$$version" \
		    -f zishrink.awk \
		    $(DATAFORM).zi >$@.out
		mv $@.out $@

version.h:	version
		VERSION=`cat version` && printf '%s\n' \
		  'static char const PKGVERSION[]="($(PACKAGE)) ";' \
		  "static char const TZVERSION[]=\"$$VERSION\";" \
		  'static char const REPORT_BUGS_TO[]="$(BUGEMAIL)";' \
		  >$@.out
		mv $@.out $@

zdump:		$(TZDOBJS)
		$(CC) -o $@ $(CFLAGS) $(LDFLAGS) $(TZDOBJS) $(LDLIBS)

zic:		$(TZCOBJS)
		$(CC) -o $@ $(CFLAGS) $(LDFLAGS) $(TZCOBJS) $(LDLIBS)

leapseconds:	$(LEAP_DEPS)
		$(AWK) -v EXPIRES_LINE=$(EXPIRES_LINE) \
		  -f leapseconds.awk leap-seconds.list >$@.out
		mv $@.out $@

# Arguments to pass to submakes of install_data.
# They can be overridden by later submake arguments.
INSTALLARGS = \
 BACKWARD='$(BACKWARD)' \
 DESTDIR='$(DESTDIR)' \
 LEAPSECONDS='$(LEAPSECONDS)' \
 PACKRATDATA='$(PACKRATDATA)' \
 PACKRATLIST='$(PACKRATLIST)' \
 TZDEFAULT='$(TZDEFAULT)' \
 TZDIR='$(TZDIR)' \
 ZIC='$(ZIC)'

INSTALL_DATA_DEPS = zic leapseconds tzdata.zi

# 'make install_data' installs one set of TZif files.
install_data: $(INSTALL_DATA_DEPS)
		$(ZIC_INSTALL) tzdata.zi

posix_only: $(INSTALL_DATA_DEPS)
		$(MAKE) $(INSTALLARGS) LEAPSECONDS= install_data

right_only: $(INSTALL_DATA_DEPS)
		$(MAKE) $(INSTALLARGS) LEAPSECONDS='-L leapseconds' \
			install_data

right_posix:	right_only
		rm -fr '$(DESTDIR)$(TZDIR)-leaps'
		ln -s '$(TZDIR_BASENAME)' '$(DESTDIR)$(TZDIR)-leaps' || \
		  $(MAKE) $(INSTALLARGS) TZDIR='$(TZDIR)-leaps' right_only
		$(MAKE) $(INSTALLARGS) TZDIR='$(TZDIR)-posix' posix_only

posix_right:	posix_only
		rm -fr '$(DESTDIR)$(TZDIR)-posix'
		ln -s '$(TZDIR_BASENAME)' '$(DESTDIR)$(TZDIR)-posix' || \
		  $(MAKE) $(INSTALLARGS) TZDIR='$(TZDIR)-posix' posix_only
		$(MAKE) $(INSTALLARGS) TZDIR='$(TZDIR)-leaps' right_only

zones:		$(REDO)

# dummy.zd is not a real file; it is mentioned here only so that the
# top-level 'make' does not have a syntax error.
ZDS = dummy.zd
# Rule used only by submakes invoked by the $(TZS_NEW) rule.
# It is separate so that GNU 'make -j' can run instances in parallel.
$(ZDS): zdump
		./zdump -i $(TZS_CUTOFF_FLAG) '$(wd)/'$$(expr $@ : '\(.*\).zd') \
		  >$@

TZS_NEW_DEPS = tzdata.zi zdump zic
$(TZS_NEW): $(TZS_NEW_DEPS)
		rm -fr tzs$(TZS_YEAR).dir
		mkdir tzs$(TZS_YEAR).dir
		$(zic) -d tzs$(TZS_YEAR).dir tzdata.zi
		$(AWK) '/^L/{print "Link\t" $$2 "\t" $$3}' \
		   tzdata.zi | LC_ALL=C sort >$@.out
		wd=`pwd` && \
		x=`$(AWK) '/^Z/{print "tzs$(TZS_YEAR).dir/" $$2 ".zd"}' \
				tzdata.zi \
			| LC_ALL=C sort -t . -k 2,2` && \
		set x $$x && \
		shift && \
		ZDS=$$* && \
		$(MAKE) wd="$$wd" TZS_CUTOFF_FLAG="$(TZS_CUTOFF_FLAG)" \
		  ZDS="$$ZDS" $$ZDS && \
		sed 's,^TZ=".*\.dir/,TZ=",' $$ZDS >>$@.out
		rm -fr tzs$(TZS_YEAR).dir
		mv $@.out $@

# If $(TZS) exists but 'make check_tzs' fails, a maintainer should inspect the
# failed output and fix the inconsistency, perhaps by running 'make force_tzs'.
$(TZS):
		touch $@

force_tzs:	$(TZS_NEW)
		cp $(TZS_NEW) $(TZS)

libtz.a:	$(LIBOBJS)
		rm -f $@
		$(AR) -rc $@ $(LIBOBJS)
		$(RANLIB) $@

date:		$(DATEOBJS)
		$(CC) -o $@ $(CFLAGS) $(LDFLAGS) $(DATEOBJS) $(LDLIBS)

tzselect:	tzselect.ksh version
		VERSION=`cat version` && sed \
			-e 's|#!/bin/bash|#!$(KSHELL)|g' \
			-e 's|AWK=[^}]*|AWK='\''$(AWK)'\''|g' \
			-e 's|\(PKGVERSION\)=.*|\1='\''($(PACKAGE)) '\''|' \
			-e 's|\(REPORT_BUGS_TO\)=.*|\1=$(BUGEMAIL)|' \
			-e 's|TZDIR=[^}]*|TZDIR=$(TZDIR)|' \
			-e 's|\(TZVERSION\)=.*|\1='"$$VERSION"'|' \
			<$@.ksh >$@.out
		chmod +x $@.out
		mv $@.out $@

check: check_back check_mild
check_mild:	check_character_set check_white_space check_links \
		  check_name_lengths check_slashed_abbrs check_sorted \
		  check_tables check_web check_ziguard check_zishrink check_tzs

check_character_set: $(ENCHILADA)
	test ! '$(UTF8_LOCALE)' || \
	! printf 'A\304\200B\n' | \
	  LC_ALL='$(UTF8_LOCALE)' grep -q '^A.B$$' >/dev/null 2>&1 || { \
		LC_ALL='$(UTF8_LOCALE)' && export LC_ALL && \
		sharp='#' && \
		! grep -Env $(SAFE_LINE) $(MANS) date.1 $(MANTXTS) \
			$(MISC) $(SOURCES) $(WEB_PAGES) \
			CONTRIBUTING LICENSE README SECURITY \
			version tzdata.zi && \
		! grep -Env $(SAFE_LINE)'|^UNUSUAL_OK_'$(OK_CHAR)'*$$' \
			Makefile && \
		! grep -Env $(SAFE_SHARP_LINE) $(TDATA_TO_CHECK) backzone \
			leapseconds zone.tab && \
		! grep -Env $(OK_LINE) $(ENCHILADA); \
	}
	touch $@

check_white_space: $(ENCHILADA)
		patfmt=' \t|[\f\r\v]' && pat=`printf "$$patfmt\\n"` && \
		! grep -En "$$pat" \
			$$(ls $(ENCHILADA) | grep -Fvx leap-seconds.list)
		! grep -n '[$s]$$' \
			$$(ls $(ENCHILADA) | grep -Fvx leap-seconds.list)
		touch $@

PRECEDES_FILE_NAME = ^(Zone|Link[$s]+[^$s]+)[$s]+
FILE_NAME_COMPONENT_TOO_LONG = $(PRECEDES_FILE_NAME)[^$s]*[^/$s]{15}

check_name_lengths: $(TDATA_TO_CHECK) backzone
		! grep -En '$(FILE_NAME_COMPONENT_TOO_LONG)' \
			$(TDATA_TO_CHECK) backzone
		touch $@

PRECEDES_STDOFF = ^(Zone[$s]+[^$s]+)?[$s]+
STDOFF = [-+]?[0-9:.]+
RULELESS_SAVE = (-|$(STDOFF)[sd]?)
RULELESS_SLASHED_ABBRS = \
  $(PRECEDES_STDOFF)$(STDOFF)[$s]+$(RULELESS_SAVE)[$s]+[^$s]*/

check_slashed_abbrs: $(TDATA_TO_CHECK)
		! grep -En '$(RULELESS_SLASHED_ABBRS)' $(TDATA_TO_CHECK)
		touch $@

CHECK_CC_LIST = { n = split($$1,a,/,/); for (i=2; i<=n; i++) print a[1], a[i]; }

check_sorted: backward backzone
		$(AWK) '/^Link/ {printf "%.5d %s\n", g, $$3} !/./ {g++}' \
		  backward | LC_ALL=C sort -cu
		$(AWK) '/^Zone/ {print $$2}' backzone | LC_ALL=C sort -cu
		touch $@

check_back:	checklinks.awk $(TDATA_TO_CHECK)
		$(AWK) \
		  -v DATAFORM=$(DATAFORM) \
		  -v backcheck=backward \
		  -f checklinks.awk $(TDATA_TO_CHECK)
		touch $@

check_links:	checklinks.awk tzdata.zi
		$(AWK) \
		  -v DATAFORM=$(DATAFORM) \
		  -f checklinks.awk tzdata.zi
		touch $@

check_tables:	checktab.awk $(YDATA) backward $(ZONETABLES)
		for tab in $(ZONETABLES); do \
		  test "$$tab" = zone.tab && links='$(BACKWARD)' || links=''; \
		  $(AWK) -f checktab.awk -v zone_table=$$tab $(YDATA) $$links \
		    || exit; \
		done
		touch $@

check_tzs:	$(TZS) $(TZS_NEW)
		if test -s $(TZS); then \
		  $(DIFF_TZS) $(TZS) $(TZS_NEW); \
		else \
		  cp $(TZS_NEW) $(TZS); \
		fi
		touch $@

check_web:	$(CHECK_WEB_PAGES)
check_theory.html: theory.html
check_tz-art.html: tz-art.html
check_tz-how-to.html: tz-how-to.html
check_tz-link.html: tz-link.html
check_theory.html check_tz-art.html check_tz-how-to.html check_tz-link.html:
		$(CURL) -sS --url https://validator.w3.org/nu/ -F out=gnu \
		    -F file=@$$(expr $@ : 'check_\(.*\)') -o $@.out && \
		  test ! -s $@.out || { cat $@.out; exit 1; }
		mv $@.out $@

check_ziguard: rearguard.zi vanguard.zi ziguard.awk
		$(AWK) -v DATAFORM=rearguard -f ziguard.awk vanguard.zi | \
		  diff -u rearguard.zi -
		$(AWK) -v DATAFORM=vanguard -f ziguard.awk rearguard.zi | \
		  diff -u vanguard.zi -
		touch $@

# Check that zishrink.awk does not alter the data, and that ziguard.awk
# preserves main-format data.
check_zishrink: check_zishrink_posix check_zishrink_right
check_zishrink_posix check_zishrink_right: \
  zic leapseconds $(PACKRATDATA) $(PACKRATLIST) \
  $(TDATA) $(DATAFORM).zi tzdata.zi
		rm -fr $@.dir $@-t.dir $@-shrunk.dir
		mkdir $@.dir $@-t.dir $@-shrunk.dir
		case $@ in \
		  *_right) leap='-L leapseconds';; \
		  *) leap=;; \
		esac && \
		  $(ZIC) $$leap -d $@.dir $(DATAFORM).zi && \
		  $(ZIC) $$leap -d $@-shrunk.dir tzdata.zi && \
		  case $(DATAFORM),$(PACKRATLIST) in \
		    main,) \
		      $(ZIC) $$leap -d $@-t.dir $(TDATA) && \
		      $(AWK) '/^Rule/' $(TDATA) | \
			$(ZIC) $$leap -d $@-t.dir - $(PACKRATDATA) && \
		      diff -r $@.dir $@-t.dir;; \
		  esac
		diff -r $@.dir $@-shrunk.dir
		rm -fr $@.dir $@-t.dir $@-shrunk.dir
		touch $@

clean_misc:
		rm -fr check_*.dir
		rm -f *.o *.out $(TIME_T_ALTERNATIVES) \
		  check_* core typecheck_* \
		  date tzselect version.h zdump zic libtz.a
clean:		clean_misc
		rm -fr *.dir tzdb-*/
		rm -f *.zi $(TZS_NEW)

maintainer-clean: clean
		@echo 'This command is intended for maintainers to use; it'
		@echo 'deletes files that may need special tools to rebuild.'
		rm -f leapseconds version $(MANTXTS) $(TZS) *.asc *.tar.*

names:
		@echo $(ENCHILADA)

public:		check check_public $(CHECK_TIME_T_ALTERNATIVES) \
		tarballs signatures

date.1.txt:	date.1
newctime.3.txt:	newctime.3
newstrftime.3.txt: newstrftime.3
newtzset.3.txt:	newtzset.3
time2posix.3.txt: time2posix.3
tzfile.5.txt:	tzfile.5
tzselect.8.txt:	tzselect.8
zdump.8.txt:	zdump.8
zic.8.txt:	zic.8

$(MANTXTS):	workman.sh
		LC_ALL=C sh workman.sh `expr $@ : '\(.*\)\.txt$$'` >$@.out
		mv $@.out $@

# Set file timestamps deterministically if possible,
# so that tarballs containing the timestamps are reproducible.
#
# '$(SET_TIMESTAMP_N) N DEST A B C ...' sets the timestamp of the
# file DEST to the maximum of the timestamps of the files A B C ...,
# plus N if GNU ls and touch are available.
SET_TIMESTAMP_N = sh -c '\
  n=$$0 dest=$$1; shift; \
  touch -cmr `ls -t "$$@" | sed 1q` "$$dest" && \
  if test $$n != 0 && \
     lsout=`ls -n --time-style="+%s" "$$dest" 2>/dev/null`; then \
    set x $$lsout && \
    touch -cmd @`expr $$7 + $$n` "$$dest"; \
  else :; fi'

SET_TIMESTAMP = $(SET_TIMESTAMP_N) 0
SET_TIMESTAMP_DEP = $(SET_TIMESTAMP_N) 1

set-timestamps.out: $(EIGHT_YARDS)
		rm -f $@
		if (type git) >/dev/null 2>&1 && \
		   files=`git ls-files $(EIGHT_YARDS)` && \
		   touch -md @1 test.out; then \
		  rm -f test.out && \
		  for file in $$files; do \
		    if git diff --quiet $$file; then \
		      time=`git log -1 --format='tformat:%ct' $$file` && \
		      touch -cmd @$$time $$file; \
		    else \
		      echo >&2 "$$file: warning: does not match repository"; \
		    fi || exit; \
		  done; \
		fi
		$(SET_TIMESTAMP_DEP) leapseconds $(LEAP_DEPS)
		for file in `ls $(MANTXTS) | sed 's/\.txt$$//'`; do \
		  $(SET_TIMESTAMP_DEP) $$file.txt $$file workman.sh || \
		    exit; \
		done
		$(SET_TIMESTAMP_DEP) version $(VERSION_DEPS)
		$(SET_TIMESTAMP_DEP) tzdata.zi $(TZDATA_ZI_DEPS)
		touch $@
set-tzs-timestamp.out: $(TZS)
		$(SET_TIMESTAMP_DEP) $(TZS) $(TZS_DEPS)
		touch $@

# The zics below ensure that each data file can stand on its own.
# We also do an all-files run to catch links to links.

check_public: $(VERSION_DEPS)
		rm -fr public.dir
		mkdir public.dir
		ln $(VERSION_DEPS) public.dir
		cd public.dir && $(MAKE) CFLAGS='$(GCC_DEBUG_FLAGS)' ALL
		for i in $(TDATA_TO_CHECK) public.dir/tzdata.zi \
		    public.dir/vanguard.zi public.dir/main.zi \
		    public.dir/rearguard.zi; \
		do \
		  public.dir/zic -v -d public.dir/zoneinfo $$i 2>&1 || exit; \
		done
		public.dir/zic -v -d public.dir/zoneinfo-all $(TDATA_TO_CHECK)
		:
		: Also check 'backzone' syntax.
		rm public.dir/main.zi
		cd public.dir && $(MAKE) PACKRATDATA=backzone main.zi
		public.dir/zic -d public.dir/zoneinfo main.zi
		rm public.dir/main.zi
		cd public.dir && \
		  $(MAKE) PACKRATDATA=backzone PACKRATLIST=zone.tab main.zi
		public.dir/zic -d public.dir/zoneinfo main.zi
		:
		rm -fr public.dir
		touch $@

# Check that the code works under various alternative
# implementations of time_t.
check_time_t_alternatives: $(TIME_T_ALTERNATIVES)
$(TIME_T_ALTERNATIVES_TAIL): $(TIME_T_ALTERNATIVES_HEAD)
$(TIME_T_ALTERNATIVES): $(VERSION_DEPS)
		rm -fr $@.dir
		mkdir $@.dir
		ln $(VERSION_DEPS) $@.dir
		case $@ in \
		  int*32_t) range=-2147483648,2147483648;; \
		  u*) range=0,4294967296;; \
		  *) range=-4294967296,4294967296;; \
		esac && \
		wd=`pwd` && \
		zones=`$(AWK) '/^[^#]/ { print $$3 }' <zone1970.tab` && \
		if test $@ = $(TIME_T_ALTERNATIVES_HEAD); then \
		  range_target=; \
		else \
		  range_target=to$$range.tzs; \
		fi && \
		(cd $@.dir && \
		  $(MAKE) TOPDIR="$$wd/$@.dir" \
		    CFLAGS='$(CFLAGS) -Dtime_tz='"'$@'" \
		    REDO='$(REDO)' \
			D=$$wd/$@.dir \
		    TZS_YEAR="$$range" TZS_CUTOFF_FLAG="-t $$range" \
		    install $$range_target) && \
		test $@ = $(TIME_T_ALTERNATIVES_HEAD) || { \
		  (cd $(TIME_T_ALTERNATIVES_HEAD).dir && \
		    $(MAKE) TOPDIR="$$wd/$@.dir" \
		      TZS_YEAR="$$range" TZS_CUTOFF_FLAG="-t $$range" \
			D=$$wd/$@.dir \
		      to$$range.tzs) && \
		  $(DIFF_TZS) $(TIME_T_ALTERNATIVES_HEAD).dir/to$$range.tzs \
			  $@.dir/to$$range.tzs && \
		  if diff -q Makefile Makefile 2>/dev/null; then \
		    quiet_option='-q'; \
		  else \
		    quiet_option=''; \
		  fi && \
		    diff $$quiet_option -r $(TIME_T_ALTERNATIVES_HEAD).dir/etc \
					   $@.dir/etc && \
		    diff $$quiet_option -r \
		      $(TIME_T_ALTERNATIVES_HEAD).dir/usr/share \
		      $@.dir/usr/share; \
		}
		touch $@

TRADITIONAL_ASC = \
  tzcode$(VERSION).tar.gz.asc \
  tzdata$(VERSION).tar.gz.asc
REARGUARD_ASC = \
  tzdata$(VERSION)-rearguard.tar.gz.asc
ALL_ASC = $(TRADITIONAL_ASC) $(REARGUARD_ASC) \
  tzdb-$(VERSION).tar.lz.asc

tarballs rearguard_tarballs tailored_tarballs traditional_tarballs \
signatures rearguard_signatures traditional_signatures: \
  version set-timestamps.out rearguard.zi vanguard.zi
		VERSION=`cat version` && \
		$(MAKE) AWK='$(AWK)' VERSION="$$VERSION" $@_version

# These *_version rules are intended for use if VERSION is set by some
# other means.  Ordinarily these rules are used only by the above
# non-_version rules, which set VERSION on the 'make' command line.
tarballs_version: traditional_tarballs_version rearguard_tarballs_version \
  tzdb-$(VERSION).tar.lz
rearguard_tarballs_version: \
  tzdata$(VERSION)-rearguard.tar.gz
traditional_tarballs_version: \
  tzcode$(VERSION).tar.gz tzdata$(VERSION).tar.gz
tailored_tarballs_version: \
  tzdata$(VERSION)-tailored.tar.gz
signatures_version: $(ALL_ASC)
rearguard_signatures_version: $(REARGUARD_ASC)
traditional_signatures_version: $(TRADITIONAL_ASC)

tzcode$(VERSION).tar.gz: set-timestamps.out
		LC_ALL=C && export LC_ALL && \
		tar $(TARFLAGS) -cf - \
		    $(COMMON) $(DOCS) $(SOURCES) | \
		  gzip $(GZIPFLAGS) >$@.out
		mv $@.out $@

tzdata$(VERSION).tar.gz: set-timestamps.out
		LC_ALL=C && export LC_ALL && \
		tar $(TARFLAGS) -cf - $(TZDATA_DIST) | \
		  gzip $(GZIPFLAGS) >$@.out
		mv $@.out $@

# Create empty files with a reproducible timestamp.
CREATE_EMPTY = TZ=UTC0 touch -mt 202010122253.00

tzdata$(VERSION)-rearguard.tar.gz: rearguard.zi set-timestamps.out
		rm -fr $@.dir
		mkdir $@.dir
		ln $(TZDATA_DIST) $@.dir
		cd $@.dir && rm -f $(TDATA) $(PACKRATDATA) version
		for f in $(TDATA) $(PACKRATDATA); do \
		  rearf=$@.dir/$$f; \
		  $(AWK) -v DATAFORM=rearguard -f ziguard.awk $$f >$$rearf && \
		  $(SET_TIMESTAMP_DEP) $$rearf ziguard.awk $$f || exit; \
		done
		sed '1s/$$/-rearguard/' <version >$@.dir/version
		: The dummy pacificnew pacifies TZUpdater 2.3.1 and earlier.
		$(CREATE_EMPTY) $@.dir/pacificnew
		touch -cmr version $@.dir/version
		LC_ALL=C && export LC_ALL && \
		  (cd $@.dir && \
		   tar $(TARFLAGS) -cf - \
			$(TZDATA_DIST) pacificnew | \
		     gzip $(GZIPFLAGS)) >$@.out
		mv $@.out $@

tzdata$(VERSION)-tailored.tar.gz: set-timestamps.out
		rm -fr $@.dir
		mkdir $@.dir
		: The dummy pacificnew pacifies TZUpdater 2.3.1 and earlier.
		cd $@.dir && \
		  $(CREATE_EMPTY) $(PRIMARY_YDATA) $(NDATA) backward \
		  `test $(DATAFORM) = vanguard || echo pacificnew`
		(grep '^#' tzdata.zi && echo && cat $(DATAFORM).zi) \
		  >$@.dir/etcetera
		touch -cmr tzdata.zi $@.dir/etcetera
		sed -n \
		  -e '/^# *version  *\(.*\)/h' \
		  -e '/^# *ddeps  */H' \
		  -e '$$!d' \
		  -e 'g' \
		  -e 's/^# *version  *//' \
		  -e 's/\n# *ddeps  */-/' \
		  -e 's/ /-/g' \
		  -e 'p' \
		  <tzdata.zi >$@.dir/version
		touch -cmr version $@.dir/version
		links= && \
		  for file in $(TZDATA_DIST); do \
		    test -f $@.dir/$$file || links="$$links $$file"; \
		  done && \
		  ln $$links $@.dir
		LC_ALL=C && export LC_ALL && \
		  (cd $@.dir && \
		   tar $(TARFLAGS) -cf - * | gzip $(GZIPFLAGS)) >$@.out
		mv $@.out $@

tzdb-$(VERSION).tar.lz: set-timestamps.out set-tzs-timestamp.out
		rm -fr tzdb-$(VERSION)
		mkdir tzdb-$(VERSION)
		ln $(ENCHILADA) tzdb-$(VERSION)
		$(SET_TIMESTAMP) tzdb-$(VERSION) tzdb-$(VERSION)/*
		LC_ALL=C && export LC_ALL && \
		tar $(TARFLAGS) -cf - tzdb-$(VERSION) | lzip -9 >$@.out
		mv $@.out $@

tzcode$(VERSION).tar.gz.asc: tzcode$(VERSION).tar.gz
tzdata$(VERSION).tar.gz.asc: tzdata$(VERSION).tar.gz
tzdata$(VERSION)-rearguard.tar.gz.asc: tzdata$(VERSION)-rearguard.tar.gz
tzdb-$(VERSION).tar.lz.asc: tzdb-$(VERSION).tar.lz
$(ALL_ASC):
		$(GPG) --armor --detach-sign $?

TYPECHECK_CFLAGS = $(CFLAGS) -DTYPECHECK -D__time_t_defined -D_TIME_T
typecheck: typecheck_long_long typecheck_unsigned
typecheck_long_long typecheck_unsigned: $(VERSION_DEPS)
		rm -fr $@.dir
		mkdir $@.dir
		ln $(VERSION_DEPS) $@.dir
		cd $@.dir && \
		  case $@ in \
		    *_long_long) i="long long";; \
		    *_unsigned ) i="unsigned" ;; \
		  esac && \
		  typecheck_cflags='' && \
		  $(MAKE) \
		    CFLAGS="$(TYPECHECK_CFLAGS) \"-Dtime_t=$$i\"" \
		    TOPDIR="`pwd`" \
		    install
		$@.dir/zdump -i -c 1970,1971 Europe/Rome
		touch $@

zonenames:	tzdata.zi
		@$(AWK) '/^Z/ { print $$2 } /^L/ { print $$3 }' tzdata.zi

asctime.o:	private.h tzfile.h
date.o:		private.h
difftime.o:	private.h
localtime.o:	private.h tzfile.h
strftime.o:	private.h tzfile.h
zdump.o:	version.h
zic.o:		private.h tzfile.h version.h

.PHONY: ALL INSTALL all
.PHONY: check check_mild check_time_t_alternatives
.PHONY: check_web check_zishrink
.PHONY: clean clean_misc dummy.zd force_tzs
.PHONY: install install_data maintainer-clean names
.PHONY: posix_only posix_right public
.PHONY: rearguard_signatures rearguard_signatures_version
.PHONY: rearguard_tarballs rearguard_tarballs_version
.PHONY: right_only right_posix signatures signatures_version
.PHONY: tarballs tarballs_version
.PHONY: traditional_signatures traditional_signatures_version
.PHONY: traditional_tarballs traditional_tarballs_version
.PHONY: tailored_tarballs tailored_tarballs_version
.PHONY: typecheck
.PHONY: zonenames zones
.PHONY: $(ZDS)
