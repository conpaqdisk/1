#pragma once
// ============================================================================
// GameHackingTools - Memory.h
// Güvenli Memory Okuma/Yazma Fonksiyonları
// ============================================================================
//
// Bu dosya oyun hafızasından güvenli şekilde veri okumak için kullanılır.
// SEH (Structured Exception Handling) ile crash önlenir.
//
// Neden SEH kullanıyoruz?
// - Geçersiz bir adresi okumaya çalışırsak oyun crash olur
// - SEH ile hata yakalayıp 0 döndürürüz, crash olmaz
//
// Kullanım örneği:
//   int hp = Memory::ReadInt(charPtr + 0x6D8);
//   if (hp == 0) { /* okuma başarısız veya hp gerçekten 0 */ }
//
// ============================================================================

#include <windows.h>
#include <string>
#include "Offsets.h"

namespace Memory
{
    // ========================================================================
    // TEMEL OKUMA FONKSİYONLARI
    // ========================================================================
    
    // ------------------------------------------------------------------------
    // ReadDWORD - 4 byte unsigned integer okur
    // ------------------------------------------------------------------------
    // Parametre: address - okunacak hafıza adresi
    // Döndürür: okunan değer veya hata durumunda 0
    // ------------------------------------------------------------------------
    inline DWORD ReadDWORD(DWORD address) {
        if (address == 0) return 0;  // Null pointer kontrolü
        __try { 
            return *(DWORD*)address;  // Adresten değeri oku
        }
        __except (EXCEPTION_EXECUTE_HANDLER) { 
            return 0;  // Hata olursa 0 döndür (crash önle)
        }
    }
    
    // ------------------------------------------------------------------------
    // ReadFloat - 4 byte ondalıklı sayı okur
    // ------------------------------------------------------------------------
    // Koordinatlar gibi ondalıklı değerler için kullanılır
    // Örnek: X = Memory::ReadFloat(charPtr + KO_OFF_POSX);
    // ------------------------------------------------------------------------
    inline float ReadFloat(DWORD address) {
        if (address == 0) return 0.0f;
        __try { 
            return *(float*)address; 
        }
        __except (EXCEPTION_EXECUTE_HANDLER) { 
            return 0.0f; 
        }
    }
    
    // ------------------------------------------------------------------------
    // ReadInt - 4 byte signed integer okur
    // ------------------------------------------------------------------------
    // HP, MP, Level gibi değerler için kullanılır
    // Örnek: int hp = Memory::ReadInt(charPtr + KO_OFF_HP);
    // ------------------------------------------------------------------------
    inline int ReadInt(DWORD address) {
        if (address == 0) return 0;
        __try { 
            return *(int*)address; 
        }
        __except (EXCEPTION_EXECUTE_HANDLER) { 
            return 0; 
        }
    }
    
    // ========================================================================
    // STRING OKUMA FONKSİYONLARI
    // ========================================================================
    
    // String okuma için global buffer (SEH ile std::string kullanılamaz)
    static char g_strBuffer[64];
    
    // ------------------------------------------------------------------------
    // ReadStringRaw - Ham string okuma (SEH korumalı)
    // ------------------------------------------------------------------------
    // Bu fonksiyon doğrudan kullanılmaz, ReadString wrapper'ı kullanılır
    // SEH (__try/__except) bloğunda std::string oluşturulamaz,
    // bu yüzden önce raw buffer'a okuyoruz
    // ------------------------------------------------------------------------
    inline bool ReadStringRaw(DWORD address, int maxLen) {
        if (address == 0) return false;
        if (maxLen > 63) maxLen = 63;  // Buffer overflow önle
        
        __try {
            for (int i = 0; i < maxLen; i++) {
                char c = *(char*)(address + i);  // Her karakteri tek tek oku
                if (c == 0) { 
                    g_strBuffer[i] = 0;  // Null terminator
                    break; 
                }
                g_strBuffer[i] = c;
            }
            g_strBuffer[maxLen] = 0;  // Güvenlik için son karakter null
            return true;
        }
        __except (EXCEPTION_EXECUTE_HANDLER) { 
            g_strBuffer[0] = 0;
            return false; 
        }
    }
    
    // ------------------------------------------------------------------------
    // ReadString - String okuma (kullanıcı dostu wrapper)
    // ------------------------------------------------------------------------
    // Karakter ismi gibi string değerler için kullanılır
    // Örnek: std::string name = Memory::ReadString(charPtr + KO_OFF_NAME, 20);
    // ------------------------------------------------------------------------
    inline std::string ReadString(DWORD address, int maxLen = 20) {
        if (ReadStringRaw(address, maxLen)) {
            return std::string(g_strBuffer);
        }
        return "";  // Okuma başarısız
    }
    
    // ========================================================================
    // YARDIMCI FONKSİYONLAR
    // ========================================================================
    
    // ------------------------------------------------------------------------
    // GetCHR - Karakter pointer'ını al
    // ------------------------------------------------------------------------
    // Karakterimizin bilgilerinin bulunduğu struct'ın adresini döndürür
    // Bu adres üzerine offset ekleyerek HP, MP vs. okunur
    // Örnek: DWORD chr = Memory::GetCHR();
    //        int hp = Memory::ReadInt(chr + KO_OFF_HP);
    // ------------------------------------------------------------------------
    inline DWORD GetCHR() { 
        return ReadDWORD(KO_PTR_CHR); 
    }
    
    // ------------------------------------------------------------------------
    // IsOnline - Oyunda mıyız kontrolü
    // ------------------------------------------------------------------------
    // Karakter pointer'ı 0 değilse oyundayız demektir
    // Login ekranında veya karakter seçiminde 0 döner
    // ------------------------------------------------------------------------
    inline bool IsOnline() { 
        return GetCHR() != 0; 
    }
}
