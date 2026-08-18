// KoInjector — genel yardımcı fonksiyonlar (util)
// Copyright (c) 2026 VenoMex

#pragma once

#include <cstdint>
#include <string>
#include <vector>

namespace koi::util {

std::string hex32(uint32_t value);
std::string hex16(uint16_t value);

std::vector<uint8_t> readFile(const std::string& path);

bool fileExists(const std::string& path);

std::string stripTrailingSlashes(std::string s);

} // namespace koi::util
