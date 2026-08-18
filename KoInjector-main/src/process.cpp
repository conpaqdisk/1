// KoInjector — XIGNCODE + KnightOnLine process başlatma
// Copyright (c) 2026 VenoMex

#include "process.hpp"

#include "util.hpp"

#include <iostream>
#include <stdexcept>
#include <vector>

#ifdef _WIN32

namespace koi::process {

std::string findXigncodeLoader(const std::string& gameDir) {
    const std::string gb = gameDir + "\\XIGNCODE\\xldr_KnightOnline_GB_loader_win32.exe";
    const std::string na = gameDir + "\\XIGNCODE\\xldr_KnightOnline_NA_loader_win32.exe";
    if (util::fileExists(gb)) return gb;
    if (util::fileExists(na)) return na;
    throw std::runtime_error("no XIGNCODE loader under " + gameDir + "\\XIGNCODE\\");
}

void runXigncodeLoader(const std::string& xldrPath, const std::string& gameDir) {
    const std::string cmd = "\"" + xldrPath + "\" KnightOnLine.exe";
    std::vector<char> cmdBuf(cmd.begin(), cmd.end());
    cmdBuf.push_back('\0');
    STARTUPINFOA si{}; si.cb = sizeof(si);
    PROCESS_INFORMATION pi{};
    if (!CreateProcessA(xldrPath.c_str(), cmdBuf.data(), nullptr, nullptr, FALSE,
                        0, nullptr, gameDir.c_str(), &si, &pi)) {
        throw std::runtime_error("CreateProcess(XIGNCODE) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    std::cout << "[xigncode] launched pid=" << pi.dwProcessId << " cmd=" << cmd << "\n";
    CloseHandle(pi.hThread);
    CloseHandle(pi.hProcess);
}

PROCESS_INFORMATION createSuspendedKnight(const std::string& gameDir) {
    const std::string koPath = gameDir + "\\KnightOnLine.exe";
    if (!util::fileExists(koPath)) {
        throw std::runtime_error("KnightOnLine.exe not found: " + koPath);
    }
    const std::string cmd = "\"" + koPath + "\"" + kKnightStartToken;
    std::vector<char> cmdBuf(cmd.begin(), cmd.end());
    cmdBuf.push_back('\0');
    STARTUPINFOA si{}; si.cb = sizeof(si);
    PROCESS_INFORMATION pi{};
    if (!CreateProcessA(koPath.c_str(), cmdBuf.data(), nullptr, nullptr, FALSE,
                        CREATE_SUSPENDED, nullptr, gameDir.c_str(), &si, &pi)) {
        throw std::runtime_error("CreateProcess(KnightOnLine) failed, GetLastError="
                                 + std::to_string(GetLastError()));
    }
    std::cout << "[knight] suspended pid=" << pi.dwProcessId << " cmd=" << cmd << "\n";
    return pi;
}

} // namespace koi::process

#endif
