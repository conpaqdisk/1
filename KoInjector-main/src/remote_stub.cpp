// KoInjector — remote stub (PIC) + parametre bloğu
// Copyright (c) 2026 VenoMex

#include "remote_stub.hpp"

#include <stdexcept>

#ifdef _WIN32

namespace koi::stub {

// --------------------------------------------------------------------------
// Remote stub — inject edilen hedef process'in içinde çalışır. Üç iş yapar:
//   1. delta = remoteImageBase - OptHdr.ImageBase ile base relocation'ları uygular.
//   2. Import descriptor'ları gezer, her biri için LoadLibraryA / GetProcAddress
//      çağırır, IAT slot'larını doldurur.
//   3. DllMain(remoteBase, DLL_PROCESS_ATTACH, 0) çağrısını yapar.
// Position-independent olmak zorundadır: global yok, string literal yok, CRT yok.
// --------------------------------------------------------------------------
#pragma code_seg(push, ".rstub")
#pragma runtime_checks("", off)
#pragma check_stack(off)

__declspec(safebuffers) __declspec(noinline)
extern "C" DWORD WINAPI RemoteStub(LPVOID lpParam) {
    typedef HMODULE (WINAPI *LoadLibFn)(LPCSTR);
    typedef FARPROC (WINAPI *GetProcFn)(HMODULE, LPCSTR);
    typedef BOOL    (WINAPI *DllMainFn)(HINSTANCE, DWORD, LPVOID);

    struct Imp   { uint32_t OFT, TDS, FC, Name, FT; };
    struct Reloc { uint32_t VirtualAddress; uint32_t SizeOfBlock; };

    const RemoteParam* p = (const RemoteParam*)lpParam;
    BYTE* base = (BYTE*)(uintptr_t)p->remoteImageBase;
    BYTE* nt   = (BYTE*)(uintptr_t)p->remoteNtHeaders;
    LoadLibFn ll = (LoadLibFn)(uintptr_t)p->pLoadLibraryA;
    GetProcFn gp = (GetProcFn)(uintptr_t)p->pGetProcAddress;

    // OptHdr layout'u: ImageBase NT+52'de, AddressOfEntryPoint NT+40'ta.
    uint32_t origImageBase = *(uint32_t*)(nt + 52);
    uint32_t delta = (uint32_t)(uintptr_t)base - origImageBase;
    if (delta != 0 && p->remoteBaseRelocDir != 0) {
        Reloc* block = (Reloc*)(uintptr_t)p->remoteBaseRelocDir;
        while (block->VirtualAddress != 0 && block->SizeOfBlock >= 8) {
            uint32_t n = (block->SizeOfBlock - 8) / 2;
            uint16_t* entries = (uint16_t*)(block + 1);
            for (uint32_t i = 0; i < n; ++i) {
                uint16_t e = entries[i];
                if ((e >> 12) == 3) { // IMAGE_REL_BASED_HIGHLOW
                    uint32_t* t = (uint32_t*)(base + block->VirtualAddress + (e & 0x0FFFu));
                    *t += delta;
                }
            }
            block = (Reloc*)((BYTE*)block + block->SizeOfBlock);
        }
    }

    if (p->remoteImportDir != 0) {
        Imp* desc = (Imp*)(uintptr_t)p->remoteImportDir;
        while (desc->Name != 0) {
            HMODULE mod = ll((LPCSTR)(base + desc->Name));
            if (!mod) return 1;
            uint32_t lkRva = desc->OFT ? desc->OFT : desc->FT;
            uint32_t* lk  = (uint32_t*)(base + lkRva);
            uint32_t* iat = (uint32_t*)(base + desc->FT);
            while (*lk != 0) {
                uint32_t t = *lk;
                FARPROC proc;
                if ((t & 0x80000000u) != 0) {
                    proc = gp(mod, (LPCSTR)(uintptr_t)(t & 0xFFFFu));
                } else {
                    proc = gp(mod, (LPCSTR)(base + t + 2));
                }
                if (!proc) return 2;
                *iat = (uint32_t)(uintptr_t)proc;
                ++lk; ++iat;
            }
            ++desc;
        }
    }

    uint32_t aep = *(uint32_t*)(nt + 40);
    DllMainFn entry = (DllMainFn)(base + aep);
    return entry((HINSTANCE)base, 1u /* DLL_PROCESS_ATTACH = 1 */, nullptr);
}

extern "C" int RemoteStubEnd() { return (int)0xDEADBEEF; }

#pragma check_stack()
#pragma runtime_checks("", restore)
#pragma code_seg(pop)

StubBlob getStubBlob() {
    const uint8_t* start = reinterpret_cast<const uint8_t*>(&RemoteStub);
    const uint8_t* end   = reinterpret_cast<const uint8_t*>(&RemoteStubEnd);
    if (end <= start) {
        throw std::runtime_error("RemoteStubEnd placed before RemoteStub (linker reorder?); "
                                 "build with /OPT:NOICF /INCREMENTAL:NO");
    }
    return StubBlob{start, static_cast<size_t>(end - start)};
}

} // namespace koi::stub

#endif
