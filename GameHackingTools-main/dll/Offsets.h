#pragma once
// ============================================================================
// GameHackingTools - Offsets.h
// Knight Online Memory Offset Tanımları
// ============================================================================
// 
// Bu dosya oyun hafızasındaki önemli adresleri içerir.
// Bu adresler Cheat Engine gibi araçlarla bulunmuştur.
// 
// NOT: Offset'ler oyun versiyonuna göre değişebilir!
// Yeni versiyon geldiğinde Cheat Engine ile güncellenmeli.
// ============================================================================

// ============================================================================
// ANA POINTER'LAR (BASE ADDRESSES)
// ============================================================================
// Bu adresler sabit ve oyun açıldığında bu adreslerde pointer'lar bulunur.
// Pointer = başka bir adrese işaret eden adres

#define KO_PTR_CHR                 0x010F4FE0  
// ^ Karakter pointer'ı
// Bu adreste karakterimizin bilgilerinin bulunduğu struct'a pointer var
// Örnek: *(DWORD*)KO_PTR_CHR = karakter struct adresi

#define KO_PTR_PKT                 0x010F50AC  
// ^ Socket/Paket pointer'ı  
// Bu adreste CAPISocket nesnesine pointer var
// Sunucuya paket göndermek için kullanılır

// ============================================================================
// KARAKTER OFFSET'LERİ (CHARACTER OFFSETS)
// ============================================================================
// Karakter pointer'ından (+offset) eklenerek okunur
// Örnek: *(int*)(charPtr + KO_OFF_HP) = mevcut HP değeri

#define KO_OFF_POSX                0x3CC   
// ^ X koordinatı (float)
// Karakterin haritadaki X pozisyonu

#define KO_OFF_POSY                0x3D4   
// ^ Y koordinatı (float)
// Karakterin haritadaki Y pozisyonu

#define KO_OFF_POSZ                0x194   
// ^ Z koordinatı (float) - yükseklik
// Karakterin haritadaki Z pozisyonu (yerden yükseklik)

#define KO_OFF_ID                  0x6A0   
// ^ Karakter ID (int)
// Sunucu tarafından atanan benzersiz ID

#define KO_OFF_NAME                0x6A4   
// ^ Karakter ismi (char[20])
// 20 karaktere kadar isim saklanır

#define KO_OFF_CLASS               0x6CC   
// ^ Sınıf kodu (int)
// 101=Warrior(K), 102=Rogue(K), 103=Mage(K), 104=Priest(K), 108=Master(K)
// 201=Warrior(E), 202=Rogue(E), 203=Mage(E), 204=Priest(E), 208=Master(E)

#define KO_OFF_NATION              0x6C4   
// ^ Ulus (int)
// 1 = Karus, 2 = El Morad

#define KO_OFF_LEVEL               0x6D0   
// ^ Level (int)
// Karakterin mevcut seviyesi

#define KO_OFF_HP                  0x6D8   
// ^ Mevcut HP (int)
// Karakterin şu anki can puanı

#define KO_OFF_MAX_HP              0x6D4   
// ^ Maksimum HP (int)
// Karakterin sahip olabileceği maksimum can

#define KO_OFF_MP                  0xBF0   
// ^ Mevcut MP (int)
// Karakterin şu anki mana puanı

#define KO_OFF_MAX_MP              0xBEC   
// ^ Maksimum MP (int)
// Karakterin sahip olabileceği maksimum mana

// ============================================================================
// FONKSİYON ADRESLERİ (FUNCTION ADDRESSES)
// ============================================================================
// Oyunun kendi fonksiyonlarını çağırmak için kullanılır
// ASM ile __asm { call functionAddress } şeklinde çağrılır

#define KO_FNC_SND                 0x00701660  
// ^ SendPacket fonksiyonu
// Sunucuya paket göndermek için kullanılır
// Kullanım: ECX = socket pointer, PUSH len, PUSH buffer, CALL KO_FNC_SND
