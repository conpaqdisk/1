// KoInjector — genel yardımcı fonksiyonlar (util)
// Copyright (c) 2026 VenoMex

#include "util.hpp"

#include <fstream>
#include <iomanip>
#include <sstream>
#include <stdexcept>

#ifdef _WIN32
#ifndef NOMINMAX
#define NOMINMAX
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#endif

namespace koi::util {

std::string hex32(uint32_t value) {
    std::ostringstream out;
    out << "0x" << std::uppercase << std::hex << std::setw(8) << std::setfill('0') << value;
    return out.str();
}

std::string hex16(uint16_t value) {
    std::ostringstream out;
    out << "0x" << std::uppercase << std::hex << std::setw(4) << std::setfill('0') << value;
    return out.str();
}

std::vector<uint8_t> readFile(const std::string& path) {
    std::ifstream f(path, std::ios::binary);
    if (!f) throw std::runtime_error("cannot open file: " + path);
    f.seekg(0, std::ios::end);
    const auto sz = f.tellg();
    f.seekg(0, std::ios::beg);
    if (sz < 0) throw std::runtime_error("cannot get file size: " + path);
    std::vector<uint8_t> data(static_cast<size_t>(sz));
    if (!data.empty()) {
        f.read(reinterpret_cast<char*>(data.data()),
               static_cast<std::streamsize>(data.size()));
    }
    return data;
}

bool fileExists(const std::string& path) {
#ifdef _WIN32
    const DWORD a = GetFileAttributesA(path.c_str());
    return a != INVALID_FILE_ATTRIBUTES && !(a & FILE_ATTRIBUTE_DIRECTORY);
#else
    std::ifstream f(path);
    return f.good();
#endif
}

std::string stripTrailingSlashes(std::string s) {
    while (!s.empty() && (s.back() == '\\' || s.back() == '/')) s.pop_back();
    return s;
}

} // namespace koi::util
