cmake_minimum_required(VERSION 3.20)

# KoInjector
# Copyright (c) 2026 VenoMex
project(KoInjector
    VERSION     1.0.0
    DESCRIPTION "Knight Online cross-process manual-map DLL injector"
    LANGUAGES   CXX)

if (NOT CMAKE_SIZEOF_VOID_P EQUAL 4)
    message(FATAL_ERROR
        "KoInjector must be built as PE32/x86 (32-bit). "
        "Configure with: cmake -A Win32 ..   (MSVC)   "
        "or: cmake -DCMAKE_CXX_FLAGS=-m32 -DCMAKE_C_FLAGS=-m32 ..   (GCC/Clang)")
endif()

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_executable(KoInjector
    src/main.cpp
    src/util.cpp
    src/pe.cpp
    src/process.cpp
    src/injector.cpp
    src/remote_stub.cpp
)

target_include_directories(KoInjector PRIVATE src)

if (MSVC)
    target_compile_options(KoInjector PRIVATE /W4 /permissive-)
    # /OPT:NOICF      : RemoteStubEnd ayrı bir sembol olarak kalsın ki stub boyutu ölçülebilsin
    # /INCREMENTAL:NO : incremental-link thunk'ları kapalı olsun, &Func gerçek byte'ları göstersin
    target_link_options(KoInjector PRIVATE /OPT:NOICF /INCREMENTAL:NO)
else()
    target_compile_options(KoInjector PRIVATE -Wall -Wextra -pedantic -m32)
    target_link_options(KoInjector PRIVATE -m32)
endif()
