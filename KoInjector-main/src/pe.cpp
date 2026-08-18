// KoInjector — PE32 parse ve yardımcıları
// Copyright (c) 2026 VenoMex

#include "pe.hpp"

#include "util.hpp"

#include <algorithm>
#include <iomanip>
#include <iostream>
#include <stdexcept>

namespace koi::pe {

namespace {

template <typename T>
const T* ptrAt(const std::vector<uint8_t>& data, size_t offset) {
    if (offset > data.size() || sizeof(T) > data.size() - offset) return nullptr;
    return reinterpret_cast<const T*>(data.data() + offset);
}

} // namespace

std::optional<PeView> parsePe(const std::vector<uint8_t>& data) {
    const auto* dos = ptrAt<ImageDosHeader>(data, 0);
    if (!dos || dos->e_magic != kMzSignature || dos->e_lfanew <= 0) return std::nullopt;

    const size_t ntOffset = static_cast<size_t>(dos->e_lfanew);
    const auto* signature = ptrAt<uint32_t>(data, ntOffset);
    if (!signature || *signature != kPeSignature) return std::nullopt;

    const auto* file = ptrAt<ImageFileHeader>(data, ntOffset + sizeof(uint32_t));
    if (!file) return std::nullopt;

    const auto* opt = ptrAt<ImageOptionalHeader32>(
        data, ntOffset + sizeof(uint32_t) + sizeof(ImageFileHeader));
    if (!opt || opt->Magic != kPe32Magic) return std::nullopt;

    const size_t sectionOffset = ntOffset + sizeof(uint32_t) + sizeof(ImageFileHeader)
                                 + file->SizeOfOptionalHeader;
    const auto* sections = ptrAt<ImageSectionHeader>(data, sectionOffset);
    if (!sections) return std::nullopt;

    const size_t sectionBytes = static_cast<size_t>(file->NumberOfSections)
                                * sizeof(ImageSectionHeader);
    if (sectionOffset > data.size() || sectionBytes > data.size() - sectionOffset) {
        return std::nullopt;
    }

    return PeView{dos, file, opt, sections, ntOffset};
}

std::vector<uint8_t> mapImageLocally(const std::vector<uint8_t>& fileData, const PeView& pe) {
    if (pe.opt->SizeOfImage == 0 || pe.opt->SizeOfImage > 512U * 1024U * 1024U) {
        throw std::runtime_error("unreasonable SizeOfImage");
    }

    std::vector<uint8_t> image(pe.opt->SizeOfImage);

    const size_t headerCopy = std::min<size_t>(pe.opt->SizeOfHeaders, fileData.size());
    std::copy_n(fileData.begin(), headerCopy, image.begin());

    for (uint16_t i = 0; i < pe.file->NumberOfSections; ++i) {
        const auto& s = pe.sections[i];
        if (s.SizeOfRawData == 0) continue;
        if (s.PointerToRawData > fileData.size()
            || s.SizeOfRawData > fileData.size() - s.PointerToRawData) {
            std::cout << "  [warn] section " << sectionName(s)
                      << " raw data outside file; skipped\n";
            continue;
        }
        if (s.VirtualAddress > image.size()
            || s.SizeOfRawData > image.size() - s.VirtualAddress) {
            std::cout << "  [warn] section " << sectionName(s)
                      << " does not fit SizeOfImage; skipped\n";
            continue;
        }
        std::copy_n(fileData.begin() + s.PointerToRawData, s.SizeOfRawData,
                    image.begin() + s.VirtualAddress);
    }

    return image;
}

std::string sectionName(const ImageSectionHeader& section) {
    size_t len = 0;
    while (len < sizeof(section.Name) && section.Name[len] != 0) ++len;
    std::string name(reinterpret_cast<const char*>(section.Name), len);
    for (char& ch : name) {
        const auto c = static_cast<unsigned char>(ch);
        if (c < 0x20 || c > 0x7E) ch = '.';
    }
    return name;
}

void printPeSummary(const PeView& pe) {
    using util::hex16;
    using util::hex32;
    std::cout << "\nPE summary:\n";
    std::cout << "  Machine        : " << hex16(pe.file->Machine) << " (0x014C = x86)\n";
    std::cout << "  Characteristics: " << hex16(pe.file->Characteristics)
              << (((pe.file->Characteristics & 0x2000) != 0) ? " DLL\n" : "\n");
    std::cout << "  ImageBase      : " << hex32(pe.opt->ImageBase) << "\n";
    std::cout << "  SizeOfImage    : " << hex32(pe.opt->SizeOfImage) << "\n";
    std::cout << "  SizeOfHeaders  : " << hex32(pe.opt->SizeOfHeaders) << "\n";
    std::cout << "  EntryRVA       : " << hex32(pe.opt->AddressOfEntryPoint) << "\n";
    std::cout << "  Sections       : " << pe.file->NumberOfSections << "\n";
    for (uint16_t i = 0; i < pe.file->NumberOfSections; ++i) {
        const auto& s = pe.sections[i];
        std::cout << "    " << std::left << std::setw(8) << sectionName(s) << std::right
                  << " raw " << hex32(s.PointerToRawData) << "+" << hex32(s.SizeOfRawData)
                  << " -> rva " << hex32(s.VirtualAddress)
                  << " vsize "  << hex32(s.VirtualSize) << "\n";
    }
}

} // namespace koi::pe
