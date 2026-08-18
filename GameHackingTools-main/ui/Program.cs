// ============================================================================
// GameHackingTools - Program.cs
// Uygulama Giriş Noktası
// ============================================================================
//
// BU DOSYA NE YAPAR?
// C# uygulamasının başlangıç noktasıdır.
// Main() fonksiyonu uygulama başladığında ilk çalışan koddur.
//
// ============================================================================

using System;
using System.Windows.Forms;

namespace GameHackingTools
{
    /// <summary>
    /// Program sınıfı - uygulama başlangıç noktası
    /// static = instance oluşturmaya gerek yok
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Main - Uygulamanın giriş noktası
        /// </summary>
        /// <remarks>
        /// [STAThread] = Single-Threaded Apartment
        /// Windows Forms için gerekli, COM nesneleriyle uyumluluk sağlar.
        /// Bu attribute olmadan bazı UI bileşenleri düzgün çalışmaz.
        /// </remarks>
        [STAThread]
        static void Main()
        {
            // Visual styles'ı aktifleştir (modern Windows görünümü)
            // Bu olmadan butonlar eski Windows 95 stilinde görünür
            Application.EnableVisualStyles();
            
            // GDI+ metin rendering ayarı
            // false = GDI+ kullan (daha iyi görünüm)
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Ana formu oluştur ve çalıştır
            // Bu satır uygulama kapanana kadar bloke olur
            Application.Run(new MainForm());
        }
    }
}
