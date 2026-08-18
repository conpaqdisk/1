// KoInjector — remote stub (PIC) + parametre bloğu
// Copyright (c) 2026 VenoMex

#pragma once

#include <cstddef>
#include <cstdint>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#endif

namespace koi::stub {

// Remote stub'a geçilen parametre bloğu layout'u (0x18 byte):
//   +0x00 remoteImageBase
//   +0x04 remoteNtHeaders       (mutlak adres: remoteBase + e_lfanew)
//   +0x08 remoteBaseRelocDir    (mutlak: remoteBase + DataDir[5].VA, yoksa 0)
//   +0x0C remoteImportDir       (mutlak: remoteBase + DataDir[1].VA, yoksa 0)
//   +0x10 pLoadLibraryA
//   +0x14 pGetProcAddress
#pragma pack(push, 1)
struct RemoteParam {
    uint32_t remoteImageBase;
    uint32_t remoteNtHeaders;
    uint32_t remoteBaseRelocDir;
    uint32_t remoteImportDir;
    uint32_t pLoadLibraryA;
    uint32_t pGetProcAddress;
};
#pragma pack(pop)
static_assert(sizeof(RemoteParam) == 0x18, "RemoteParam must be 0x18 bytes");

#ifdef _WIN32
struct StubBlob {
    const uint8_t* bytes;
    size_t         size;
};

// Derlenmiş PIC stub byte'larına bir pointer ve boyutunu döner (boyut, runtime'da
// bitiş marker'ı eksi başlangıç olarak ölçülür). Linker marker'ları yeniden
// sıralamışsa fırlatır; /OPT:NOICF /INCREMENTAL:NO ile derleyin.
StubBlob getStubBlob();

extern "C" DWORD WINAPI RemoteStub(LPVOID lpParam);
extern "C" int          RemoteStubEnd();
#endif

} // namespace koi::stub
