
EXTRA_DIST =  expansions.m4

#Warning: This is an automatically generated file, do not edit!
if ENABLE_DEBUG
 SUBDIRS =  . 
endif
if ENABLE_RELEASE
 SUBDIRS =  . 
endif

# Include project specific makefile
include TemplateSharp.make

# Ubuntu packaging

PACKAGE_UBUNTU_DIR=$(srcdir)/packaging/Ubuntu
PACKAGE_UBUNTU_ORIG_TAR_NAME=@PACKAGE@_@VERSION@.orig.tar.gz
PACKAGE_UBUNTU_ORIG_TAR=$(PACKAGE_UBUNTU_DIR)/$(PACKAGE_UBUNTU_ORIG_TAR_NAME)

.PHONY: package-ubuntu
package-ubuntu: dist
	cd $(PACKAGE_UBUNTU_DIR)/debian && debchange --check-dirname-level 0 -v @VERSION@-0ubuntu1
	cp $(distdir).tar.gz $(PACKAGE_UBUNTU_ORIG_TAR)
	tar -xzf $(PACKAGE_UBUNTU_ORIG_TAR) -C $(PACKAGE_UBUNTU_DIR)
	cp -R $(PACKAGE_UBUNTU_DIR)/debian $(PACKAGE_UBUNTU_DIR)/$(distdir)
	cd $(PACKAGE_UBUNTU_DIR)/$(distdir) && debuild

CLEANFILES += $(PACKAGE_UBUNTU_ORIG_TAR)

clean-local:
	rm -Rf $(PACKAGE_UBUNTU_DIR)/@PACKAGE@{-,_}*
