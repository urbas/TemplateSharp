dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([TemplateSharp], [0.1])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(DMCS, dmcs, no)
if test "x$DMCS" = "xno"; then
        AC_MSG_ERROR([dmcs Not found])
fi


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_DEBUG, true)
	enable_debug=yes
fi

dnl nunit checking
AC_DEFUN([SHAMROCK_CHECK_NUNIT],
[
	NUNIT_REQUIRED=2.4.7

	AC_ARG_ENABLE(tests, AC_HELP_STRING([--enable-tests], [Enable NUnit tests]),
		enable_tests=$enableval, enable_tests="no")

	if test "x$enable_tests" = "xno"; then
		do_tests=no
		AM_CONDITIONAL(ENABLE_TESTS, false)
	else
		PKG_CHECK_MODULES(NUNIT, nunit >= $NUNIT_REQUIRED,
			do_tests="yes", do_tests="no")

		AC_SUBST(NUNIT_LIBS)
		AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")

		if test "x$do_tests" = "xno"; then
			PKG_CHECK_MODULES(NUNIT, mono-nunit >= 2.4,
				do_tests="yes", do_tests="no")

			AC_SUBST(NUNIT_LIBS)
			AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")

			if test "x$do_tests" = "xno"; then
				AC_MSG_WARN([Could not find nunit: tests will not be available])
			fi
		fi
	fi
])


dnl package checks, common for all configs
SHAMROCK_CHECK_NUNIT

dnl package checks, per config


AC_CONFIG_FILES([
templatesharp.pc
Makefile

])

AC_OUTPUT
