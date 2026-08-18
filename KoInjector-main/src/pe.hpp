// KoInjector — PE32 parse ve yardımcıları
// Copyright (c) 2026 VenoMex

#pragma once

#include <cstdint>
#include <optional>
#include <string>
#include <vector>

namespace koi::pe {

inline constexpr uint16_t kMzSignature   = 0x5A4D;
inline constexpr uint32_t kPeSignature   = 0x00004550;
inline constexpr uint16_t kPe32Magic     = 0x010B;
inline constexpr uint32_t kOrdinalFlag32 = 0x80000000u;

enum DirectoryIndex : size_t {
    ExportDirectory    = 0,
    ImportDirectory    = 1,
    BaseRelocDirectory = 5,
};

#pragma pack(push, 1)
struct ImageDosHeader {
    uint16_t e_magic;
    uint8_t  unused1[58];
    int32_t  e_lfanew;
};

struct ImageFileHeader {
    uint16_t Machine;
    uint16_t NumberOfSections;
    uint32_t TimeDateStamp;
    uint32_t PointerToSymbolTable;
    uint32_t NumberOfSymbols;
    uint16_t SizeOfOptionalHeader;
    uint16_t Characteristics;
};

struct ImageDataDirectory {
    uint32_t VirtualAddress;
    uint32_t Size;
};

struct ImageOptionalHeader32 {
    uint16_t Magic;
    uint8_t  MajorLinkerVersion;
    uint8_t  MinorLinkerVersion;
    uint32_t SizeOfCode;
    uint32_t SizeOfInitializedData;
    uint32_t SizeOfUninitializedData;
    uint32_t AddressOfEntryPoint;
    uint32_t BaseOfCode;
    uint32_t BaseOfData;
    uint32_t ImageBase;
    uint32_t SectionAlignment;
    uint32_t FileAlignment;
    uint16_t MajorOperatingSystemVersion;
    uint16_t MinorOperatingSystemVersion;
    uint16_t MajorImageVersion;
    uint16_t MinorImageVersion;
    uint16_t MajorSubsystemVersion;
    uint16_t MinorSubsystemVersion;
    uint32_t Win32VersionValue;
    uint32_t SizeOfImage;
    uint32_t SizeOfHeaders;
    uint32_t CheckSum;
    uint16_t Subsystem;
    uint16_t DllCharacteristics;
    uint32_t SizeOfStackReserve;
    uint32_t SizeOfStackCommit;
    uint32_t SizeOfHeapReserve;
    uint32_t SizeOfHeapCommit;
    uint32_t LoaderFlags;
    uint32_t NumberOfRvaAndSizes;
    ImageDataDirectory DataDirectory[16];
};

struct ImageSectionHeader {
    uint8_t  Name[8];
    uint32_t VirtualSize;
    uint32_t VirtualAddress;
    uint32_t SizeOfRawData;
    uint32_t PointerToRawData;
    uint32_t PointerToRelocations;
    uint32_t PointerToLinenumbers;
    uint16_t NumberOfRelocations;
    uint16_t NumberOfLinenumbers;
    uint32_t Characteristics;
};

struct ImageBaseRelocation {
    uint32_t VirtualAddress;
    uint32_t SizeOfBlock;
};

struct ImageImportDescriptor {
    uint32_t OriginalFirstThunk;
    uint32_t TimeDateStamp;
    uint32_t ForwarderChain;
    uint32_t Name;
    uint32_t FirstThunk;
};
#pragma pack(pop)

struct PeView {
    const ImageDosHeader*        dos      = nullptr;
    const ImageFileHeader*       file     = nullptr;
    const ImageOptionalHeader32* opt      = nullptr;
    const ImageSectionHeader*    sections = nullptr;
    size_t                       ntOffset = 0;
};

// PE32 image'i non-owning bir view'e parse eder. PE olmayan / PE32 olmayan
// girişler için std::nullopt döner.
std::optional<PeView> parsePe(const std::vector<uint8_t>& data);

// SizeOfImage büyüklüğünde std::vector<uint8_t> oluşturur; header ve
// section'ları kendi VirtualAddress konumlarına yerleştirir. Bu aşamada
// relocation uygulanmaz.
std::vector<uint8_t> mapImageLocally(const std::vector<uint8_t>& fileData, const PeView& pe);

std::string sectionName(const ImageSectionHeader& section);

void printPeSummary(const PeView& pe);

} // namespace koi::pe
