# KoInjector

Knight Online için 32-bit **cross-process manual-map DLL injector**.

Windows PE yükleyicisini devre dışı bırakıp, hedef sürecin (`KnightOnLine.exe`)
belleğine bir DLL'i doğrudan yazar; base relocation ve import çözümlerini
position-independent bir remote stub içinde yapıp `DllMain`'i çağırır.

![platform](https://img.shields.io/badge/platform-Windows%20x86-0078D6)
![language](https://img.shields.io/badge/C%2B%2B-17-00599C)
![license](https://img.shields.io/badge/license-Proprietary-lightgrey)

---

## Özellikler

- **Tek komut** — oyun klasörü + DLL yolu ile injection tamamlanır.
- **XIGNCODE entegrasyonu** — dizindeki `xldr_KnightOnline_GB` ya da `_NA_` varyantı
  otomatik bulunur ve önce başlatılır.
- **Suspended process** — `KnightOnLine.exe` doğru komut satırı token'ı ile
  `CREATE_SUSPENDED` modunda oluşturulur, DLL yerleştirildikten sonra resume edilir.
- **Manual map** — `VirtualAllocEx` + `WriteProcessMemory` + `CreateRemoteThread`
  üçlüsüyle DLL'in kendisi, parametre bloğu ve remote stub hedef belleğe yazılır.
- **PIC remote stub** — hedef süreç bağlamında base relocation'ları uygular,
  `LoadLibraryA` / `GetProcAddress` üzerinden IAT'yi çözer, `DllMain`'i çağırır.
- **Şeffaf çıktı** — tüm adımlar (PE özeti, tahsis adresleri, stub boyutu, stub
  exit code, resume durumu) stdout'a basılır.

## Gereksinimler

| | |
|---|---|
| OS | Windows 10 / 11 |
| Toolchain | Visual Studio 2019+ (Build Tools yeterli) |
| Build system | CMake 3.20+ |
| Mimari | PE32 / x86 (32-bit) |

## Derleme

```powershell
git clone https://github.com/VenoMexx/KoInjector.git
cd KoInjector
cmake -S . -B build-x86 -A Win32
cmake --build build-x86 --config Release
```

Üretilen dosya: `build-x86\Release\KoInjector.exe`.

> 32-bit zorunlu: `-A Win32` bayrağı atlanırsa CMake derleme öncesi
> `FATAL_ERROR` ile durur.

## Kullanım

```
KoInjector.exe <knight_dir> <payload.dll>
```

| Argüman | Açıklama |
|---|---|
| `knight_dir` | `KnightOnLine.exe` ve `XIGNCODE\xldr_*.exe` içeren oyun klasörü |
| `payload.dll` | Enjekte edilecek **PE32 / x86** DLL |

### Örnek

```powershell
KoInjector.exe "C:\Games\KnightOnline" "C:\bots\myplugin.dll"
```

### Örnek çıktı

```
[setup]    game dir : C:\Games\KnightOnline
[setup]    payload  : C:\bots\myplugin.dll
[setup]    xigncode : C:\Games\KnightOnline\XIGNCODE\xldr_KnightOnline_NA_loader_win32.exe
[xigncode] launched pid=12345 ...
[knight]   suspended pid=12346 cmd="...\KnightOnLine.exe" E03ED890-...
[payload]  file size 1740800 bytes
PE summary:
  Machine     : 0x014C  (x86)
  ImageBase   : 0x00400000
  SizeOfImage : 0x001B0000
  ...
[inject]   image allocated at 0x02BC0000
[inject]   image written: 1740800/1740800 bytes
[param]    remoteImageBase    = 0x02BC0000
[param]    remoteNtHeaders    = 0x02BC0108
[param]    remoteBaseRelocDir = 0x02D50000
[param]    remoteImportDir    = 0x02D40000
[param]    pLoadLibraryA      = 0x75B32D80
[param]    pGetProcAddress    = 0x75B32EF0
[inject]   stub at 0x03200018 (178 bytes)
[inject]   remote thread created
[inject]   stub returned 0x00000001
[resume]   KnightOnLine.exe main thread resumed
```


## Çalışma Prensibi

Injection zinciri `main.cpp` içinde orkestre edilir:

1. **XIGNCODE başlatma** — `findXigncodeLoader` GB/NA varyantını seçer;
   `runXigncodeLoader` `CreateProcessA` ile başlatır (working dir = oyun klasörü).
2. **Suspended Knight** — `createSuspendedKnight` gerekli komut satırı token'ı
   ile `CreateProcessA(CREATE_SUSPENDED)` çağırır.
3. **PE parse** — `koi::pe::parsePe` DLL'i validate eder, `PeView` çıkartır.
4. **Lokal image** — `mapImageLocally` section'ları `SizeOfImage` buffer'a
   VirtualAddress konumlarında yerleştirir.
5. **Remote alloc + write** — image için `VirtualAllocEx` (RWX),
   `WriteProcessMemory` ile ham image aktarılır.
6. **Parametre bloğu + stub** — `VirtualAllocEx` ile 0x1000 byte ayrılır,
   0x18 byte parametre bloğu ve peşine PIC stub yazılır.
7. **Remote thread** — `CreateRemoteThread` stub'ı tetikler; stub hedef
   bağlamda reloc + import resolve + `DllMain(base, DLL_PROCESS_ATTACH, 0)` yapar.
8. **Resume** — ana thread `ResumeThread` ile serbest bırakılır.

### Parametre bloğu (0x18 byte)

```
offset | field                | açıklama
-------+----------------------+------------------------------------------
+0x00  | remoteImageBase      | uzaktaki image base adresi
+0x04  | remoteNtHeaders      | remoteImageBase + e_lfanew
+0x08  | remoteBaseRelocDir   | remoteImageBase + DataDir[5].VA (yoksa 0)
+0x0C  | remoteImportDir      | remoteImageBase + DataDir[1].VA (yoksa 0)
+0x10  | pLoadLibraryA        | kernel32!LoadLibraryA adresi
+0x14  | pGetProcAddress      | kernel32!GetProcAddress adresi
```

### Remote stub

`remote_stub.cpp` içinde kendine özel `.rstub` code section'ında derlenir
(`#pragma code_seg`). `/GS` cookie'si ve `/RTC` runtime check'leri pragma ile
kapatılır (`__declspec(safebuffers)` + `runtime_checks("", off)`).
`/OPT:NOICF /INCREMENTAL:NO` linker bayrakları fonksiyon sırasını korur,
böylece `RemoteStubEnd - RemoteStub` runtime'da stub boyutunu verir.

## Proje Yapısı

```
src/
  main.cpp            entry point, argv parse, orkestrasyon
  util.hpp  util.cpp  hex formatter, dosya I/O, path yardımcıları
  pe.hpp    pe.cpp    PE32 struct'ları, parsePe, mapImageLocally, printPeSummary
  process.hpp   .cpp  XIGNCODE launcher + suspended KnightOnLine process oluşturucu
  remote_stub.hpp cpp Parametre bloğu + PIC remote stub
  injector.hpp  .cpp  Cross-process manual-map akışı
```

Namespace düzeni: `koi::util`, `koi::pe`, `koi::process`, `koi::stub`, `koi::inject`.

## Lisans

Copyright © 2026 VenoMex. All rights reserved.

Bu yazılım kişisel ve eğitim amaçlı kullanım için tasarlanmıştır.
Dağıtım ve ticari kullanım yazarın iznine tabidir.
