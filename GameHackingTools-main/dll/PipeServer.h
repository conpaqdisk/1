#pragma once
// ============================================================================
// GameHackingTools - PipeServer.h
// Named Pipe Sunucu - UI ile Haberleşme
// ============================================================================
//
// NAMED PIPE NEDİR?
// Named Pipe, iki process (uygulama) arasında veri alışverişi yapmanın 
// bir yoludur. Bizim durumumuzda:
//   - DLL (bu dosya) = Sunucu (Server)
//   - UI (C# uygulaması) = İstemci (Client)
//
// NEDEN PIPE KULLANIYORUZ?
// - DLL oyunun içinde çalışır (inject edilmiş)
// - UI ayrı bir uygulama olarak çalışır
// - İkisi arasında iletişim için Pipe kullanırız
//
// ÇALIŞMA PRENSİBİ:
// 1. DLL bir pipe oluşturur ve bekler (CreateNamedPipe)
// 2. UI bu pipe'a bağlanır (ConnectNamedPipe)
// 3. UI komut gönderir (örn: "CHARINFO")
// 4. DLL komutu işler ve yanıt gönderir
//
// ============================================================================

#include <windows.h>
#include <string>

namespace Pipe
{
    // ========================================================================
    // GLOBAL DEĞİŞKENLER
    // ========================================================================
    
    // Pipe handle'ı - pipe nesnesini temsil eder
    inline HANDLE g_hPipe = INVALID_HANDLE_VALUE;
    
    // Pipe adı - UI de aynı ismi kullanmalı!
    // Format: \\.\pipe\[isim]
    inline const char* PIPE_NAME = "\\\\.\\pipe\\GameHackingToolsPipe";
    
    // ========================================================================
    // PIPE FONKSİYONLARI
    // ========================================================================
    
    // ------------------------------------------------------------------------
    // Initialize - Pipe oluştur
    // ------------------------------------------------------------------------
    // Bu fonksiyon yeni bir Named Pipe oluşturur.
    // 
    // Parametreler:
    // - PIPE_ACCESS_DUPLEX: Hem okuma hem yazma yapılabilir
    // - PIPE_TYPE_MESSAGE: Veri mesaj olarak gönderilir (stream değil)
    // - PIPE_WAIT: Senkron mod (işlem bitene kadar bekler)
    // - 1: Maksimum instance sayısı (1 client olabilir)
    // - 4096: Output buffer boyutu
    // - 4096: Input buffer boyutu
    //
    // Döndürür: true = başarılı, false = hata
    // ------------------------------------------------------------------------
    inline bool Initialize()
    {
        g_hPipe = CreateNamedPipeA(
            PIPE_NAME,                                          // Pipe adı
            PIPE_ACCESS_DUPLEX,                                 // Çift yönlü
            PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,  // Mesaj modu
            1,                                                  // Max 1 client
            4096,                                               // Output buffer
            4096,                                               // Input buffer
            0,                                                  // Timeout (default)
            NULL                                                // Security (default)
        );
        
        // INVALID_HANDLE_VALUE = hata oluştu
        return g_hPipe != INVALID_HANDLE_VALUE;
    }
    
    // ------------------------------------------------------------------------
    // TryConnect - Client bağlantısı bekle
    // ------------------------------------------------------------------------
    // Bu fonksiyon UI'ın bağlanmasını bekler.
    // UI bağlanana kadar bloke olur (bekler).
    //
    // ConnectNamedPipe dönüş değerleri:
    // - TRUE: Client yeni bağlandı
    // - FALSE + ERROR_PIPE_CONNECTED: Client zaten bağlıydı
    // - FALSE + başka hata: Gerçek hata
    //
    // Döndürür: true = bağlantı var, false = hata
    // ------------------------------------------------------------------------
    inline bool TryConnect()
    {
        if (g_hPipe == INVALID_HANDLE_VALUE) return false;
        
        // Bağlantı bekle
        BOOL result = ConnectNamedPipe(g_hPipe, NULL);
        
        // Ya yeni bağlandı ya da zaten bağlıydı
        return result || GetLastError() == ERROR_PIPE_CONNECTED;
    }
    
    // ------------------------------------------------------------------------
    // Receive - Mesaj al
    // ------------------------------------------------------------------------
    // UI'dan gelen komutu okur.
    // Örnek: "CHARINFO", "TOWN", "PING"
    //
    // Eğer okuma başarısız olursa (client koptu), boş string döner
    // ve pipe disconnect edilir.
    //
    // Döndürür: alınan mesaj veya "" (hata/kopma)
    // ------------------------------------------------------------------------
    inline std::string Receive()
    {
        char buffer[1024] = {0};  // Mesaj buffer'ı
        DWORD bytesRead = 0;      // Okunan byte sayısı
        
        // Pipe'dan oku
        if (ReadFile(g_hPipe, buffer, sizeof(buffer)-1, &bytesRead, NULL))
        {
            // Başarılı - string olarak döndür
            return std::string(buffer, bytesRead);
        }
        
        // Okuma başarısız - muhtemelen client koptu
        DisconnectNamedPipe(g_hPipe);
        return "";
    }
    
    // ------------------------------------------------------------------------
    // Send - Mesaj gönder
    // ------------------------------------------------------------------------
    // UI'a yanıt gönderir.
    // Örnek: "CHARINFO:Ayaz|LV=83|HP=5000/5000"
    //
    // Parametre: msg - gönderilecek mesaj
    // Döndürür: true = başarılı, false = hata
    // ------------------------------------------------------------------------
    inline bool Send(const std::string& msg)
    {
        DWORD bytesWritten = 0;
        
        // Pipe'a yaz
        return WriteFile(
            g_hPipe,                    // Pipe handle
            msg.c_str(),                // Mesaj içeriği
            (DWORD)msg.size(),          // Mesaj uzunluğu
            &bytesWritten,              // Yazılan byte sayısı
            NULL                        // Overlapped (null = senkron)
        );
    }
    
    // ------------------------------------------------------------------------
    // Shutdown - Pipe'ı kapat
    // ------------------------------------------------------------------------
    // Pipe'ı kapatır ve kaynakları serbest bırakır.
    // DLL unload olurken veya hata durumunda çağrılır.
    // ------------------------------------------------------------------------
    inline void Shutdown()
    {
        if (g_hPipe != INVALID_HANDLE_VALUE)
        {
            DisconnectNamedPipe(g_hPipe);  // Client bağlantısını kes
            CloseHandle(g_hPipe);           // Handle'ı kapat
            g_hPipe = INVALID_HANDLE_VALUE; // Değişkeni sıfırla
        }
    }
}
