// ============================================================================
// GameHackingTools - PipeClient.cs
// Named Pipe İstemci - DLL ile Haberleşme
// ============================================================================
//
// BU DOSYA NE YAPAR?
// UI uygulaması ile DLL arasındaki iletişimi sağlar.
// DLL oyunun içinde çalışırken, UI ayrı bir uygulama.
// Named Pipe ile bu iki uygulama veri alışverişi yapar.
//
// NAMED PIPE NEDİR?
// Named Pipe, Windows'ta iki process arasında iletişim sağlayan bir mekanizma.
// Bizim durumumuzda:
//   - DLL (C++) = Server - pipe oluşturur ve bekler
//   - UI (C#) = Client - pipe'a bağlanır ve komut gönderir
//
// ÇALIŞMA PRENSİBİ:
//   1. UI başlar ve DLL'e bağlanmaya çalışır
//   2. Bağlantı kurulunca komut gönderebilir
//   3. Komut gönder -> Yanıt bekle -> İşle
//
// ÖRNEK KULLANIM:
//   var pipe = new PipeClient();
//   await pipe.ConnectAsync();
//   string response = pipe.SendCommand("CHARINFO");
//   // response = "CHARINFO:Ayaz|LV=83|HP=5000/5000..."
//
// ============================================================================

using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace GameHackingTools
{
    /// <summary>
    /// Named Pipe Client - DLL ile haberleşme sınıfı
    /// IDisposable = using() bloğuyla kullanılabilir, otomatik temizlik
    /// </summary>
    public class PipeClient : IDisposable
    {
        // ====================================================================
        // SABİTLER VE DEĞİŞKENLER
        // ====================================================================
        
        /// <summary>
        /// Pipe adı - DLL'deki PIPE_NAME ile AYNI olmalı!
        /// DLL:  "\\\\.\\pipe\\GameHackingToolsPipe"
        /// C#:   "GameHackingToolsPipe" (prefix otomatik eklenir)
        /// </summary>
        private const string PIPE_NAME = "GameHackingToolsPipe";
        
        /// <summary>
        /// Pipe stream nesnesi - veri okuma/yazma için
        /// </summary>
        private NamedPipeClientStream _pipe;
        
        /// <summary>
        /// Bağlantı durumu
        /// </summary>
        private bool _connected;
        
        /// <summary>
        /// Thread güvenliği için kilit nesnesi
        /// Birden fazla thread aynı anda pipe'a erişmesin
        /// </summary>
        private readonly object _lock = new object();
        
        // ====================================================================
        // EVENT'LER
        // ====================================================================
        
        /// <summary>
        /// Bağlantı durumu değiştiğinde tetiklenir
        /// true = bağlandı, false = koptu
        /// UI buna abone olup arayüzü güncelleyebilir
        /// </summary>
        public event Action<bool> OnConnectionChanged;
        
        /// <summary>
        /// Bağlantı durumu (readonly property)
        /// </summary>
        public bool IsConnected => _connected;
        
        // ====================================================================
        // BAĞLANTI FONKSİYONLARI
        // ====================================================================
        
        /// <summary>
        /// DLL'e bağlan (asenkron)
        /// </summary>
        /// <param name="timeoutMs">Maksimum bekleme süresi (ms)</param>
        /// <returns>true = bağlandı, false = zaman aşımı veya hata</returns>
        /// <remarks>
        /// Bu fonksiyon async/await kullanır.
        /// UI thread'i bloke etmeden arka planda çalışır.
        /// 
        /// KULLANIM:
        ///   bool connected = await pipe.ConnectAsync(3000); // 3 saniye bekle
        /// </remarks>
        public async Task<bool> ConnectAsync(int timeoutMs = 2000)
        {
            // Zaten bağlıysak tekrar bağlanma
            if (_connected && _pipe != null && _pipe.IsConnected)
                return true;
            
            // Eski pipe varsa temizle
            if (_pipe != null)
            {
                try { _pipe.Dispose(); } catch { }
                _pipe = null;
            }
            
            try
            {
                // Yeni pipe oluştur
                // "." = local bilgisayar
                // PipeDirection.InOut = hem okuma hem yazma
                _pipe = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.InOut);
                
                // Bağlanmayı bekle (async = UI donmaz)
                await _pipe.ConnectAsync(timeoutMs);
                
                // Başarılı!
                _connected = true;
                OnConnectionChanged?.Invoke(true);  // Event tetikle
                return true;
            }
            catch
            {
                // Bağlantı başarısız (DLL inject edilmemiş olabilir)
                _connected = false;
                OnConnectionChanged?.Invoke(false);
                return false;
            }
        }
        
        /// <summary>
        /// Bağlantıyı kes
        /// </summary>
        public void Disconnect()
        {
            lock (_lock)  // Thread güvenliği
            {
                _connected = false;
                _pipe?.Dispose();
                _pipe = null;
            }
            OnConnectionChanged?.Invoke(false);  // Event tetikle
        }
        
        // ====================================================================
        // KOMUT GÖNDERİM FONKSİYONLARI
        // ====================================================================
        
        /// <summary>
        /// Komut gönder ve yanıt al
        /// </summary>
        /// <param name="command">Gönderilecek komut (örn: "CHARINFO")</param>
        /// <returns>DLL'den gelen yanıt veya hata mesajı</returns>
        /// <remarks>
        /// Bu fonksiyon SENKRON çalışır - yanıt gelene kadar bekler.
        /// Timer içinden çağırılabilir.
        /// 
        /// ÖRNEK YANIT FORMATLARI:
        ///   "PONG"
        ///   "CHARINFO:İsim|LV=83|HP=5000/5000|MP=1000/1000|X=100|Y=200"
        ///   "TOWN:OK"
        ///   "ERROR:UNKNOWN_COMMAND"
        /// </remarks>
        public string SendCommand(string command)
        {
            lock (_lock)  // Thread güvenliği
            {
                // Bağlı mıyız kontrol et
                if (!_connected || _pipe == null)
                    return "ERROR:NOT_CONNECTED";
                
                try
                {
                    // Komutu byte array'e çevir ve gönder
                    byte[] cmdBytes = Encoding.UTF8.GetBytes(command);
                    _pipe.Write(cmdBytes, 0, cmdBytes.Length);
                    _pipe.Flush();  // Buffer'ı boşalt - hemen gönder
                    
                    // Yanıtı bekle ve oku
                    byte[] buffer = new byte[4096];  // Yanıt buffer'ı
                    int bytesRead = _pipe.Read(buffer, 0, buffer.Length);
                    
                    // Okunan veriyi string'e çevir
                    if (bytesRead > 0)
                        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    return "ERROR:NO_RESPONSE";
                }
                catch (Exception ex)
                {
                    // Hata oldu - muhtemelen bağlantı koptu
                    _connected = false;
                    OnConnectionChanged?.Invoke(false);
                    return $"ERROR:{ex.Message}";
                }
            }
        }
        
        // ====================================================================
        // TEMİZLİK
        // ====================================================================
        
        /// <summary>
        /// IDisposable implementasyonu - kaynakları temizle
        /// </summary>
        /// <remarks>
        /// using() bloğu kullanıldığında otomatik çağrılır:
        ///   using (var pipe = new PipeClient())
        ///   {
        ///       // kullan...
        ///   } // burada Dispose() otomatik çağrılır
        /// </remarks>
        public void Dispose() => Disconnect();
    }
}
