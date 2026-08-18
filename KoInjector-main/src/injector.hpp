// KoInjector — cross-process manual-map injector
// Copyright (c) 2026 VenoMex

#pragma once

#include "pe.hpp"

#include <cstdint>
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

namespace koi::inject {

#ifdef _WIN32
// fileData / pe ile temsil edilen DLL'i hProcess içine manual-map eder.
// Ham (relocate edilmemiş) image'i, 0x18 byte'lık parametre bloğunu ve PIC
// remote stub'ı yazar; ardından stub'ı CreateRemoteThread ile başlatıp
// DllMain tamamlanana kadar 30 saniyeye kadar bekler.
void injectCrossProcess(HANDLE hProcess,
                        std::vector<uint8_t>& fileData,
                        const pe::PeView& pe);
#endif

} // namespace koi::inject
