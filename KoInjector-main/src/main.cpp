// KoInjector — orkestrasyon + argv parse (entry point)
// Copyright (c) 2026 VenoMex

#include "injector.hpp"
#include "pe.hpp"
#include "process.hpp"
#include "util.hpp"

#include <iostream>
#include <optional>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#endif

namespace {

void usage() {
    std::cout << "usage: KoInjector <knight_dir> <payload.dll>\n"
              << "  <knight_dir>    folder containing KnightOnLine.exe and XIGNCODE\\xldr_*.exe\n"
              << "  <payload.dll>   PE32/x86 DLL to inject into KnightOnLine.exe\n"
              << "\n"
              << "NOTE: do not end a quoted <knight_dir> with a trailing backslash.\n"
              << "  wrong: \"C:\\Games\\Knight Online\\\"   (cmd.exe escapes the closing quote)\n"
              << "  right: \"C:\\Games\\Knight Online\"     (drop the trailing backslash)\n";
}

bool parseArgs(int argc, char** argv, std::string& gameDir, std::string& payloadPath) {
    if (argc == 3) {
        gameDir     = argv[1];
        payloadPath = argv[2];
        return true;
    }
    if (argc == 2) {
        // cmd.exe sondaki-backslash quote tuzağı için kurtarma yolu:
        //   "C:\...\Knight Online\" payload.dll
        // \" escape'li quote olarak yorumlanır; bu yüzden iki yol da argv[1]'e
        // aralarında literal bir '"' ile yapışık gelir.
        std::string merged = argv[1];
        const size_t q = merged.find("\" ");
        if (q != std::string::npos) {
            gameDir     = merged.substr(0, q);
            payloadPath = merged.substr(q + 2);
            std::cerr << "[warn] recovered from trailing-backslash quoting issue in argv\n";
            return true;
        }
    }
    return false;
}

} // namespace

int main(int argc, char** argv) {
#ifndef _WIN32
    (void)argc; (void)argv;
    std::cerr << "error: Windows only\n";
    return 1;
#else
    try {
        std::string gameDir, payloadPath;
        if (!parseArgs(argc, argv, gameDir, payloadPath)) {
            usage();
            return 2;
        }

        gameDir = koi::util::stripTrailingSlashes(std::move(gameDir));
        if (gameDir.empty()) throw std::runtime_error("empty game directory");
        if (GetFileAttributesA(gameDir.c_str()) == INVALID_FILE_ATTRIBUTES) {
            throw std::runtime_error("game directory not found: " + gameDir);
        }

        std::cout << "[setup] game dir : " << gameDir << "\n";
        std::cout << "[setup] payload  : " << payloadPath << "\n";

        const std::string xldr = koi::process::findXigncodeLoader(gameDir);
        std::cout << "[setup] xigncode : " << xldr << "\n";

        koi::process::runXigncodeLoader(xldr, gameDir);

        PROCESS_INFORMATION pi = koi::process::createSuspendedKnight(gameDir);

        std::vector<uint8_t> fileData = koi::util::readFile(payloadPath);
        std::cout << "[payload] file size " << fileData.size() << " bytes\n";

        std::optional<koi::pe::PeView> pe = koi::pe::parsePe(fileData);
        if (!pe.has_value()) {
            throw std::runtime_error("payload is not a valid PE32 image");
        }
        koi::pe::printPeSummary(*pe);

        std::cout << "\n[map] preparing local image buffer:\n";
        std::cout << "[map] SizeOfImage = " << koi::util::hex32(pe->opt->SizeOfImage) << "\n";
        for (uint16_t i = 0; i < pe->file->NumberOfSections; ++i) {
            const auto& s = pe->sections[i];
            std::cout << "[map] section " << koi::pe::sectionName(s)
                      << " raw " << koi::util::hex32(s.PointerToRawData)
                      << "+"     << koi::util::hex32(s.SizeOfRawData)
                      << " -> rva " << koi::util::hex32(s.VirtualAddress) << "\n";
        }

        koi::inject::injectCrossProcess(pi.hProcess, fileData, *pe);

        if (ResumeThread(pi.hThread) == static_cast<DWORD>(-1)) {
            std::cerr << "[resume] ResumeThread failed, GetLastError=" << GetLastError() << "\n";
        } else {
            std::cout << "[resume] KnightOnLine.exe main thread resumed\n";
        }

        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return 0;
    } catch (const std::exception& ex) {
        std::cerr << "error: " << ex.what() << "\n";
        return 1;
    }
#endif
}
