// KoInjector — cross-process manual-map injector
// Copyright (c) 2026 VenoMex

#include "injector.hpp"

#include "remote_stub.hpp"
#include "util.hpp"

#include <iostream>
#include <stdexcept>
#include <string>

#ifdef _WIN32

namespace koi::inject {

namespace {

uint32_t u32(const void* p) {
    return static_cast<uint32_t>(reinterpret_cast<uintptr_t>(p));
}

} // namespace

void injectCrossProcess(HANDLE hProcess,
                        std::vector<uint8_t>& fileData,
                        const pe::PeView& pe) {
    using util::hex32;

    // Ham mapped image (header + section'lar kendi VirtualAddress konumlarında).
    // Burada relocation uygulanmaz — remote stub reloc directory'yi kendisi
    // gezerek hedef bellekte in-place düzeltir.
    std::vector<uint8_t> image = pe::mapImageLocally(fileData, pe);

    LPVOID remoteBase = VirtualAllocEx(hProcess, nullptr, pe.opt->SizeOfImage,
                                       MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!remoteBase) {
        throw std::runtime_error("VirtualAllocEx(image) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    const uint32_t remoteBase32 = u32(remoteBase);
    std::cout << "[inject] image allocated at " << hex32(remoteBase32)
              << " (SizeOfImage " << hex32(pe.opt->SizeOfImage) << ")\n";

    SIZE_T written = 0;
    if (!WriteProcessMemory(hProcess, remoteBase, image.data(), image.size(), &written)) {
        throw std::runtime_error("WriteProcessMemory(image) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    std::cout << "[inject] image written: " << written << "/" << image.size()
              << " bytes (raw, unrelocated)\n";

    LPVOID remoteParamStub = VirtualAllocEx(hProcess, nullptr, 0x1000,
                                            MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
    if (!remoteParamStub) {
        throw std::runtime_error("VirtualAllocEx(param+stub) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }

    HMODULE k32 = GetModuleHandleA("kernel32.dll");
    FARPROC pLL = k32 ? GetProcAddress(k32, "LoadLibraryA")  : nullptr;
    FARPROC pGP = k32 ? GetProcAddress(k32, "GetProcAddress") : nullptr;
    if (!pLL || !pGP) throw std::runtime_error("failed to resolve kernel32 exports");

    const uint32_t relocVa  = pe.opt->DataDirectory[pe::BaseRelocDirectory].VirtualAddress;
    const uint32_t importVa = pe.opt->DataDirectory[pe::ImportDirectory].VirtualAddress;

    stub::RemoteParam params{};
    params.remoteImageBase    = remoteBase32;
    params.remoteNtHeaders    = remoteBase32 + static_cast<uint32_t>(pe.ntOffset);
    params.remoteBaseRelocDir = relocVa  ? (remoteBase32 + relocVa)  : 0;
    params.remoteImportDir    = importVa ? (remoteBase32 + importVa) : 0;
    params.pLoadLibraryA      = u32(pLL);
    params.pGetProcAddress    = u32(pGP);

    std::cout << "[param] remoteImageBase    = " << hex32(params.remoteImageBase)    << "\n";
    std::cout << "[param] remoteNtHeaders    = " << hex32(params.remoteNtHeaders)    << "\n";
    std::cout << "[param] remoteBaseRelocDir = " << hex32(params.remoteBaseRelocDir) << "\n";
    std::cout << "[param] remoteImportDir    = " << hex32(params.remoteImportDir)    << "\n";
    std::cout << "[param] pLoadLibraryA      = " << hex32(params.pLoadLibraryA)      << "\n";
    std::cout << "[param] pGetProcAddress    = " << hex32(params.pGetProcAddress)    << "\n";

    if (!WriteProcessMemory(hProcess, remoteParamStub, &params, sizeof(params), &written)) {
        throw std::runtime_error("WriteProcessMemory(param) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }

    const auto blob = stub::getStubBlob();
    if (blob.size > 0x1000 - sizeof(params)) {
        throw std::runtime_error("stub size " + std::to_string(blob.size) + " too large");
    }

    LPVOID remoteStubAddr = static_cast<uint8_t*>(remoteParamStub) + sizeof(params);
    if (!WriteProcessMemory(hProcess, remoteStubAddr, blob.bytes, blob.size, &written)) {
        throw std::runtime_error("WriteProcessMemory(stub) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    std::cout << "[inject] param at " << hex32(u32(remoteParamStub))
              << ", stub at "         << hex32(u32(remoteStubAddr))
              << " (" << blob.size << " bytes)\n";

    HANDLE hThread = CreateRemoteThread(
        hProcess, nullptr, 0,
        reinterpret_cast<LPTHREAD_START_ROUTINE>(remoteStubAddr),
        remoteParamStub, 0, nullptr);
    if (!hThread) {
        throw std::runtime_error("CreateRemoteThread failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    std::cout << "[inject] remote thread created\n";

    const DWORD w = WaitForSingleObject(hThread, 30000);
    if (w == WAIT_OBJECT_0) {
        DWORD code = 0;
        GetExitCodeThread(hThread, &code);
        std::cout << "[inject] stub returned " << hex32(code) << "\n";
    } else {
        std::cout << "[inject] stub wait status " << hex32(w)
                  << " (timeout or error, continuing)\n";
    }
    CloseHandle(hThread);
}

} // namespace koi::inject

#endif
