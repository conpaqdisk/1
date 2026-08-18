// KoInjector — XIGNCODE + KnightOnLine process başlatma
// Copyright (c) 2026 VenoMex

#pragma once

#include <string>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#endif

namespace koi::process {

// Knight Online başlatılırken exe yolundan sonra bu token gerekli.
inline constexpr const char* kKnightStartToken =
    " E03ED890-8E94-4B42-B1C5-3CDA401AA9C2";

#ifdef _WIN32
// <gameDir>\XIGNCODE\xldr_KnightOnline_{GB,NA}_loader_win32.exe yolundaki
// XIGNCODE loader'ın tam yolunu döner. İki varyant da yoksa fırlatır.
std::string findXigncodeLoader(const std::string& gameDir);

// XIGNCODE loader'ı "KnightOnLine.exe" tek argümanıyla, çalışma dizini gameDir
// olacak şekilde başlatır. Fire-and-forget (bekleme yok).
void runXigncodeLoader(const std::string& xldrPath, const std::string& gameDir);

// KnightOnLine.exe'yi CREATE_SUSPENDED ve gerekli command-line token ile
// CreateProcessA ile başlatır. PROCESS_INFORMATION handle'larının ömrü
// çağıranın sorumluluğundadır.
PROCESS_INFORMATION createSuspendedKnight(const std::string& gameDir);
#endif

} // namespace koi::process
