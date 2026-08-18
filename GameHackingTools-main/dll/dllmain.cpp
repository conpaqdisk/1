// ============================================================================
// GameHackingTools - dllmain.cpp
// Ana DLL Dosyası - Oyuna Enjekte Edilen Kod
// ============================================================================
//
// BU DOSYA NE YAPAR?
// Bu DLL oyun process'ine inject edildikten sonra çalışır.
// UI uygulamasıyla Named Pipe üzerinden haberleşir.
// UI'dan gelen komutları işler ve yanıt gönderir.
//
// MİMARİ:
// ┌─────────────┐    Named Pipe    ┌─────────────┐
// │   UI (C#)   │◄────────────────►│  DLL (C++)  │
// └─────────────┘                  └──────┬──────┘
//                                         │
//                                         ▼
//                                  ┌─────────────┐
//                                  │ Knight Online│
//                                  │   (Oyun)    │
//                                  └─────────────┘
//
// THREAD'LER:
// 1. PipeThreadFunc       - UI ile haberleşme
// 2. MonitorThreadFunc    - Oyun durumu izleme (TODO)
// 3. HotkeyThreadFunc     - Kısayol tuşları (TODO)
// 4. ContinuousResumeThreads - Thread donmalarını önleme
//
// ============================================================================

// ============================================================================
// INCLUDE DOSYALARI
// ============================================================================
#include <windows.h>      // Windows API fonksiyonları
#include <string>         // std::string
#include <sstream>        // std::stringstream (string birleştirme)
#include <thread>         // std::thread (çoklu thread)
#include <atomic>         // std::atomic (thread-safe değişkenler)
#include <tlhelp32.h>     // Thread snapshot (ResumeThreads için)
#include "PipeServer.h"   // Named Pipe sunucu
#include "Memory.h"       // Memory okuma fonksiyonları
#include "Offsets.h"      // Oyun memory offset'leri

// ============================================================================
// GLOBAL DEĞİŞKENLER
// ============================================================================

// DLL modül handle'ı - DLL'in kendisini temsil eder
HMODULE g_hModule = NULL;

// Çalışma durumu - false olunca tüm thread'ler durur
// atomic = thread-safe, birden fazla thread güvenle erişebilir
std::atomic<bool> g_Running(true);

// ============================================================================
// PAKET GÖNDERME FONKSİYONU
// ============================================================================

// ----------------------------------------------------------------------------
// SendPacket - Sunucuya paket gönder
// ----------------------------------------------------------------------------
// Bu fonksiyon oyunun kendi SendPacket fonksiyonunu çağırır.
// ASM (Assembly) kullanılarak çağrılır çünkü:
// - Fonksiyon __thiscall calling convention kullanıyor
// - ECX register'ına socket pointer'ı koymamız gerekiyor
//
// Parametre:
//   pBuf - gönderilecek paket verisi
//   len  - paket uzunluğu
//
// ÖRNEK PAKETLER:
//   Town (kasaba): { 0x48 }
//   Basic Attack:  { 0x08, 0x01, ... }
// ----------------------------------------------------------------------------
void SendPacket(BYTE* pBuf, int len)
{
    __asm {
        MOV ECX, KO_PTR_PKT           // Socket pointer adresini ECX'e koy
        MOV ECX, DWORD PTR DS:[ECX]   // Pointer'ı dereference et (asıl socket)
        PUSH len                       // Paket uzunluğunu stack'e koy
        PUSH pBuf                      // Paket adresini stack'e koy
        MOV EAX, KO_FNC_SND           // SendPacket fonksiyon adresi
        CALL EAX                       // Fonksiyonu çağır
    }
}

// ============================================================================
// KOMUT İŞLEYİCİ
// ============================================================================

// ----------------------------------------------------------------------------
// ProcessCommand - UI'dan gelen komutu işle
// ----------------------------------------------------------------------------
// UI Named Pipe üzerinden komut gönderir, bu fonksiyon işler ve yanıt verir.
//
// DESTEKLENEN KOMUTLAR:
//   "PING"     -> "PONG" (bağlantı testi)
//   "CHARINFO" -> "CHARINFO:İsim|LV=83|HP=5000/5000..." (karakter bilgisi)
//   "TOWN"     -> "TOWN:OK" (kasabaya dön)
//   Bilinmeyen -> "ERROR:UNKNOWN_COMMAND"
// ----------------------------------------------------------------------------
void ProcessCommand(const std::string& cmd)
{
    // ========================================================================
    // PING - Bağlantı Testi
    // ========================================================================
    // UI "PING" gönderir, DLL "PONG" ile yanıt verir
    // Bu sayede bağlantının aktif olduğu kontrol edilir
    // ========================================================================
    if (cmd == "PING")
    {
        Pipe::Send("PONG");
        return;
    }
    
    // ========================================================================
    // CHARINFO - Karakter Bilgisi
    // ========================================================================
    // Karakterin tüm temel bilgilerini okur ve gönderir.
    // Format: CHARINFO:İsim|LV=seviye|CL=sınıf|HP=x/y|MP=x/y|X=x|Y=y
    // 
    // Eğer oyunda değilsek "CHARINFO:OFFLINE" döner
    // ========================================================================
    if (cmd == "CHARINFO")
    {
        // Oyunda mıyız kontrol et
        if (!Memory::IsOnline())
        {
            Pipe::Send("CHARINFO:OFFLINE");
            return;
        }
        
        // Karakter pointer'ını al
        DWORD chr = Memory::GetCHR();
        
        // Tüm değerleri oku
        std::string name = Memory::ReadString(chr + KO_OFF_NAME, 20);  // İsim
        int level = Memory::ReadInt(chr + KO_OFF_LEVEL);               // Seviye
        int charClass = Memory::ReadInt(chr + KO_OFF_CLASS);           // Sınıf
        int hp = Memory::ReadInt(chr + KO_OFF_HP);                     // Mevcut HP
        int maxHp = Memory::ReadInt(chr + KO_OFF_MAX_HP);              // Max HP
        int mp = Memory::ReadInt(chr + KO_OFF_MP);                     // Mevcut MP
        int maxMp = Memory::ReadInt(chr + KO_OFF_MAX_MP);              // Max MP
        int x = (int)Memory::ReadFloat(chr + KO_OFF_POSX);             // X koordinat
        int y = (int)Memory::ReadFloat(chr + KO_OFF_POSY);             // Y koordinat
        
        // Yanıtı oluştur
        std::stringstream ss;
        ss << "CHARINFO:" << name 
           << "|LV=" << level
           << "|CL=" << charClass
           << "|HP=" << hp << "/" << maxHp 
           << "|MP=" << mp << "/" << maxMp
           << "|X=" << x << "|Y=" << y;
        
        // Yanıtı gönder
        Pipe::Send(ss.str());
        return;
    }
    
    // ========================================================================
    // TOWN - Kasabaya Dön
    // ========================================================================
    // 0x48 paketini göndererek karakteri kasabaya ışınlar.
    // NOT: Oyunda "Town" butonu ile aynı işlevi yapar
    // ========================================================================
    if (cmd == "TOWN")
    {
        // Oyunda mıyız kontrol et
        if (!Memory::IsOnline())
        {
            Pipe::Send("TOWN:OFFLINE");
            return;
        }
        
        // Town paketi - sadece 1 byte
        BYTE packet[] = { 0x48 };
        SendPacket(packet, 1);
        
        Pipe::Send("TOWN:OK");
        return;
    }
    
    // ========================================================================
    // BİLİNMEYEN KOMUT
    // ========================================================================
    Pipe::Send("ERROR:UNKNOWN_COMMAND");
}

// ============================================================================
// THREAD FONKSİYONLARI
// ============================================================================

// ----------------------------------------------------------------------------
// Thread 1: PipeThreadFunc - UI Haberleşme Thread'i
// ----------------------------------------------------------------------------
// Bu thread sürekli çalışır ve UI ile haberleşmeyi sağlar.
//
// ÇALIŞMA DÖNGÜSÜ:
// 1. Pipe oluştur
// 2. Client (UI) bağlantısını bekle
// 3. Bağlandığında sürekli komut al ve işle
// 4. Bağlantı koparsa 1'e dön
// ----------------------------------------------------------------------------
void PipeThreadFunc()
{
    while (g_Running)
    {
        // Pipe oluştur
        if (!Pipe::Initialize())
        {
            Sleep(1000);  // 1 saniye bekle ve tekrar dene
            continue;
        }
        
        // Client bağlantısı bekle (blocking - bağlanana kadar bekler)
        if (Pipe::TryConnect())
        {
            // Bağlantı kuruldu - komutları işle
            while (g_Running)
            {
                // UI'dan komut al
                std::string cmd = Pipe::Receive();
                
                // Boş string = bağlantı koptu
                if (cmd.empty())
                    break;
                
                // Komutu işle ve yanıt gönder
                ProcessCommand(cmd);
            }
        }
        
        // Pipe'ı kapat ve yeniden başla
        Pipe::Shutdown();
        Sleep(100);
    }
    
    // DLL kapanıyor - son temizlik
    Pipe::Shutdown();
}

// ----------------------------------------------------------------------------
// Thread 2: MonitorThreadFunc - Oyun Durumu İzleme
// ----------------------------------------------------------------------------
// Bu thread oyun penceresinin durumunu izler.
// Örneğin: minimize edildi mi, kapatıldı mı vs.
// 
// ŞU AN: Boş bırakıldı (TODO)
// GELİŞTİRME FİKRİ: Oyun kapandığında DLL'i temiz şekilde kapat
// ----------------------------------------------------------------------------
void MonitorThreadFunc()
{
    while (g_Running)
    {
        // TODO: Oyun penceresi durumunu kontrol et
        Sleep(500);  // Her 500ms'de bir kontrol
    }
}

// ----------------------------------------------------------------------------
// Thread 3: HotkeyThreadFunc - Kısayol Tuşları
// ----------------------------------------------------------------------------
// Bu thread klavye kısayollarını dinler.
// Örneğin: F1 = Town, F2 = Buff vs.
//
// ŞU AN: Boş bırakıldı (TODO)
// GELİŞTİRME FİKRİ: GetAsyncKeyState ile tuş dinleme
// ----------------------------------------------------------------------------
void HotkeyThreadFunc()
{
    while (g_Running)
    {
        // TODO: Kısayol tuşlarını dinle
        Sleep(50);  // Her 50ms'de bir kontrol (hızlı tepki için)
    }
}

// ----------------------------------------------------------------------------
// Thread 4: ContinuousResumeThreads - Thread Donma Önleyici
// ----------------------------------------------------------------------------
// Bu thread diğer thread'lerin suspend durumuna düşmesini önler.
// Bazı anti-cheat sistemleri thread'leri suspend eder, bu fonksiyon
// sürekli resume yaparak donmayı engeller.
//
// ÇALIŞMA PRENSİBİ:
// 1. Process'teki tüm thread'leri listele
// 2. Her thread için ResumeThread çağır
// 3. Bu sayede suspend edilen thread'ler tekrar çalışır
// ----------------------------------------------------------------------------
void ContinuousResumeThreads()
{
    while (g_Running)
    {
        // Mevcut thread ve process ID'lerini al
        DWORD currentThreadId = GetCurrentThreadId();
        DWORD currentProcessId = GetCurrentProcessId();
        
        // Process'teki tüm thread'lerin snapshot'ını al
        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
        
        if (hSnapshot != INVALID_HANDLE_VALUE)
        {
            THREADENTRY32 te = { sizeof(THREADENTRY32) };
            
            // İlk thread'i al
            if (Thread32First(hSnapshot, &te))
            {
                do
                {
                    // Sadece kendi process'imizdeki thread'leri işle
                    // Kendimizi (bu thread'i) hariç tut
                    if (te.th32OwnerProcessID == currentProcessId && 
                        te.th32ThreadID != currentThreadId)
                    {
                        // Thread handle'ını aç
                        HANDLE hThread = OpenThread(THREAD_SUSPEND_RESUME, FALSE, te.th32ThreadID);
                        if (hThread)
                        {
                            // Thread'i resume et (suspend'den çıkar)
                            ResumeThread(hThread);
                            CloseHandle(hThread);
                        }
                    }
                } while (Thread32Next(hSnapshot, &te));
            }
            
            CloseHandle(hSnapshot);
        }
        
        Sleep(100);  // Her 100ms'de bir kontrol
    }
}

// ============================================================================
// DLL GİRİŞ NOKTASI
// ============================================================================

// ----------------------------------------------------------------------------
// DllMain - DLL'in ana giriş noktası
// ----------------------------------------------------------------------------
// Bu fonksiyon DLL yüklendiğinde veya kaldırıldığında otomatik çağrılır.
//
// PARAMETRELER:
//   hModule  - DLL'in handle'ı
//   reason   - çağrılma nedeni
//   reserved - sistem tarafından ayrılmış
//
// REASON DEĞERLERİ:
//   DLL_PROCESS_ATTACH - DLL process'e yüklendi (inject)
//   DLL_PROCESS_DETACH - DLL process'ten kaldırılıyor
//   DLL_THREAD_ATTACH  - Yeni thread oluşturuldu
//   DLL_THREAD_DETACH  - Thread sonlandı
// ----------------------------------------------------------------------------
extern "C" __declspec(dllexport) BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID reserved)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        // ====================================================================
        // DLL YÜKLEME (INJECT)
        // ====================================================================
        
        // DLL handle'ını sakla
        g_hModule = hModule;
        
        // Thread attach/detach callback'lerini kapat (performans için)
        DisableThreadLibraryCalls(hModule);
        
        // Tüm thread'leri başlat
        // detach() = ana thread'den bağımsız çalış
        std::thread(PipeThreadFunc).detach();         // UI haberleşme
        std::thread(MonitorThreadFunc).detach();      // Oyun izleme
        std::thread(HotkeyThreadFunc).detach();       // Kısayol tuşları
        std::thread(ContinuousResumeThreads).detach(); // Donma önleyici
    }
    else if (reason == DLL_PROCESS_DETACH)
    {
        // ====================================================================
        // DLL KALDIRMA
        // ====================================================================
        
        // Tüm thread'leri durdur
        g_Running = false;
    }
    
    return TRUE;  // Başarılı
}
