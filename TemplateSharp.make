

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(CSC)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/TemplateSharp.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

TEMPLATESHARP_DLL_MDB_SOURCE=bin/Debug/TemplateSharp.dll.mdb
TEMPLATESHARP_DLL_MDB=$(BUILD_DIR)/TemplateSharp.dll.mdb

if ENABLE_TESTS
ASSEMBLY_COMPILER_FLAGS += "-define:ENABLE_TESTS"
endif

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(CSC)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/TemplateSharp.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

TEMPLATESHARP_DLL_MDB=

if ENABLE_TESTS
ASSEMBLY_COMPILER_FLAGS += "-define:ENABLE_TESTS"
endif

endif

AL=al
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(TEMPLATESHARP_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(TEMPLATESHARP_PC)  


RESGEN=resgen2

include TemplateSharp.config
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG)

FILES = $(TEMPLATESHARP_CS) $(TEMPLATESHARP_ASSEMBLYINFO)

dist_doc_DATA = $(TEMPLATESHARP_MARKDOWNHTMLS)

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	template-sharp.pc.in \
    $(TEMPLATESHARP_MARKDOWNS)

REFERENCES = $(TEMPLATESHARP_REFS)

if ENABLE_TESTS
FILES += $(TEMPLATESHARP_TESTS)
REFERENCES += $(TEMPLATESHARP_TEST_REFS)
endif

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) $(TEMPLATESHARP_MARKDOWNHTMLS)

include $(top_srcdir)/Makefile.include

TEMPLATESHARP_PC = $(BUILD_DIR)/template-sharp.pc

$(eval $(call emit-deploy-wrapper,TEMPLATESHARP_PC,template-sharp.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

if ENABLE_TESTS
test: $(ASSEMBLY)
	nunit-console $(ASSEMBLY)
endif

.md.html:
	$(MARKDOWN) $< > $@
