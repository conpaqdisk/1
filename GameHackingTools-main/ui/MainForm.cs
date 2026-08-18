// ============================================================================
// GameHackingTools - MainForm.cs
// Ana Kullanıcı Arayüzü
// ============================================================================
//
// BU DOSYA NE YAPAR?
// Uygulamanın ana penceresidir. Kullanıcı bu pencere üzerinden:
// - Karakter bilgilerini görür (isim, level, HP, MP, pozisyon)
// - DLL bağlantı durumunu izler
// - Town butonuyla kasabaya dönebilir
//
// ÖZELLİKLER:
// - Uygulama açılınca otomatik DLL'e bağlanır
// - Pencere başlığında karakter adı gösterilir
// - Her saniye karakter bilgileri güncellenir
//
// ============================================================================

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameHackingTools
{
    /// <summary>
    /// Ana Form - Karakter Bilgisi + Town Butonu
    /// DLL'e otomatik bağlanır, injector YOK
    /// </summary>
    public class MainForm : Form
    {
        // ====================================================================
        // SINIF DEĞİŞKENLERİ
        // ====================================================================
        
        /// <summary>
        /// Pipe client - DLL ile haberleşme için
        /// </summary>
        private PipeClient _pipe;
        
        /// <summary>
        /// Timer - her saniye karakter bilgisini günceller
        /// </summary>
        private Timer _timer;
        
        /// <summary>
        /// Mevcut karakter adı - pencere başlığı için
        /// </summary>
        private string _currentCharName = "";
        
        // ====================================================================
        // UI KONTROLLER
        // ====================================================================
        
        // Bağlantı durumu
        private Label lblStatus;       // "● Bağlı" veya "● Bağlı değil"
        private Button btnConnect;     // Yeniden bağlan butonu
        
        // Karakter bilgileri
        private Label lblName;         // Karakter adı
        private Label lblLevel;        // Seviye
        private Label lblClass;        // Sınıf
        private ProgressBar barHP;     // HP bar
        private ProgressBar barMP;     // MP bar
        private Label lblHP;           // HP değeri (örn: 5000/5000)
        private Label lblMP;           // MP değeri
        private Label lblPosition;     // X, Y koordinatları
        
        // Butonlar
        private Button btnTown;        // Kasabaya dön
        
        // Log
        private ListBox lstLog;        // İşlem geçmişi
        
        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================
        
        /// <summary>
        /// Form constructor - UI oluştur ve başlat
        /// </summary>
        public MainForm()
        {
            // UI kontrollerini oluştur
            InitializeComponent();
            
            // Pipe client oluştur ve event'e abone ol
            _pipe = new PipeClient();
            _pipe.OnConnectionChanged += OnConnectionChanged;
            
            // Form yüklenince otomatik bağlan
            this.Load += async (s, e) => await TryConnect();
        }
        
        // ====================================================================
        // UI OLUŞTURMA
        // ====================================================================
        
        /// <summary>
        /// UI kontrollerini oluştur ve konumlandır
        /// </summary>
        private void InitializeComponent()
        {
            // ----------------------------------------------------------------
            // FORM AYARLARI
            // ----------------------------------------------------------------
            this.Text = "GameHackingTools";  // Başlangıç başlığı
            this.Size = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;  // Boyut değiştirilemez
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;  // Ekran ortası
            this.BackColor = Color.FromArgb(30, 30, 35);  // Koyu arka plan
            
            int y = 15;  // Dikey pozisyon sayacı
            
            // ----------------------------------------------------------------
            // BAĞLANTI DURUMU
            // ----------------------------------------------------------------
            lblStatus = new Label
            {
                Text = "● Bağlantı bekleniyor...",
                ForeColor = Color.Orange,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, y),
                Size = new Size(250, 25)
            };
            this.Controls.Add(lblStatus);
            
            // Bağlan butonu
            btnConnect = new Button
            {
                Text = "Bağlan",
                Location = new Point(280, y - 3),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnect.Click += async (s, e) => await TryConnect();
            this.Controls.Add(btnConnect);
            
            y += 40;
            
            // ----------------------------------------------------------------
            // KARAKTER PANELİ
            // ----------------------------------------------------------------
            var panel = new Panel
            {
                Location = new Point(15, y),
                Size = new Size(355, 180),
                BackColor = Color.FromArgb(45, 45, 55),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(panel);
            
            int py = 10;  // Panel içi dikey pozisyon
            
            // Karakter adı - büyük ve renkli
            lblName = new Label
            {
                Text = "Karakter: -",
                ForeColor = Color.Cyan,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, py),
                Size = new Size(330, 25)
            };
            panel.Controls.Add(lblName);
            py += 30;
            
            // Level ve Sınıf - yan yana
            lblLevel = new Label
            {
                Text = "Level: -",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, py),
                Size = new Size(100, 22)
            };
            panel.Controls.Add(lblLevel);
            
            lblClass = new Label
            {
                Text = "Sınıf: -",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(120, py),
                Size = new Size(200, 22)
            };
            panel.Controls.Add(lblClass);
            py += 30;
            
            // HP Bar
            var lblHPTitle = new Label
            {
                Text = "HP:",
                ForeColor = Color.LightGray,
                Location = new Point(10, py + 2),
                Size = new Size(30, 20)
            };
            panel.Controls.Add(lblHPTitle);
            
            barHP = new ProgressBar
            {
                Location = new Point(45, py),
                Size = new Size(200, 22),
                Maximum = 100,
                Value = 0
            };
            panel.Controls.Add(barHP);
            
            lblHP = new Label
            {
                Text = "0 / 0",
                ForeColor = Color.LightCoral,
                Location = new Point(250, py + 2),
                Size = new Size(90, 20)
            };
            panel.Controls.Add(lblHP);
            py += 28;
            
            // MP Bar
            var lblMPTitle = new Label
            {
                Text = "MP:",
                ForeColor = Color.LightGray,
                Location = new Point(10, py + 2),
                Size = new Size(30, 20)
            };
            panel.Controls.Add(lblMPTitle);
            
            barMP = new ProgressBar
            {
                Location = new Point(45, py),
                Size = new Size(200, 22),
                Maximum = 100,
                Value = 0
            };
            panel.Controls.Add(barMP);
            
            lblMP = new Label
            {
                Text = "0 / 0",
                ForeColor = Color.LightBlue,
                Location = new Point(250, py + 2),
                Size = new Size(90, 20)
            };
            panel.Controls.Add(lblMP);
            py += 30;
            
            // Pozisyon
            lblPosition = new Label
            {
                Text = "Pozisyon: X=0, Y=0",
                ForeColor = Color.Gray,
                Location = new Point(10, py),
                Size = new Size(330, 22)
            };
            panel.Controls.Add(lblPosition);
            
            y += 195;
            
            // ----------------------------------------------------------------
            // TOWN BUTONU
            // ----------------------------------------------------------------
            btnTown = new Button
            {
                Text = "🏠 TOWN",
                Location = new Point(15, y),
                Size = new Size(355, 45),
                BackColor = Color.FromArgb(60, 120, 60),  // Yeşil
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false  // Bağlantı olmadan devre dışı
            };
            btnTown.Click += BtnTown_Click;
            this.Controls.Add(btnTown);
            
            y += 60;
            
            // ----------------------------------------------------------------
            // LOG LİSTESİ
            // ----------------------------------------------------------------
            lstLog = new ListBox
            {
                Location = new Point(15, y),
                Size = new Size(355, 100),
                BackColor = Color.FromArgb(25, 25, 30),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lstLog);
            
            // ----------------------------------------------------------------
            // TIMER - KARAKTERİ GÜNCELLE
            // ----------------------------------------------------------------
            _timer = new Timer { Interval = 1000 };  // Her 1 saniye
            _timer.Tick += Timer_Tick;
        }
        
        // ====================================================================
        // BAĞLANTI FONKSİYONLARI
        // ====================================================================
        
        /// <summary>
        /// DLL'e bağlanmayı dene
        /// </summary>
        private async Task TryConnect()
        {
            Log("DLL'e bağlanılıyor...");
            
            // 3 saniye timeout ile bağlan
            bool connected = await _pipe.ConnectAsync(3000);
            
            if (connected)
            {
                Log("✓ Bağlantı başarılı!");
                _timer.Start();  // Karakter güncellemeyi başlat
            }
            else
            {
                Log("✗ Bağlantı başarısız! DLL inject edilmiş mi?");
            }
        }
        
        /// <summary>
        /// Bağlantı durumu değiştiğinde çağrılır
        /// </summary>
        private void OnConnectionChanged(bool connected)
        {
            // UI thread'inde çalış (farklı thread'den çağrılabilir)
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionChanged(connected)));
                return;
            }
            
            if (connected)
            {
                // Bağlandı
                lblStatus.Text = "● Bağlı";
                lblStatus.ForeColor = Color.LightGreen;
                btnTown.Enabled = true;
                btnConnect.Enabled = false;
            }
            else
            {
                // Bağlantı koptu
                lblStatus.Text = "● Bağlantı kesildi";
                lblStatus.ForeColor = Color.Red;
                btnTown.Enabled = false;
                btnConnect.Enabled = true;
                _timer.Stop();
                
                // Pencere başlığını sıfırla
                this.Text = "GameHackingTools";
            }
        }
        
        // ====================================================================
        // TIMER - KARAKTERİ GÜNCELLE
        // ====================================================================
        
        /// <summary>
        /// Her saniye karakter bilgisini güncelle
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_pipe.IsConnected) return;
            
            // DLL'den karakter bilgisini iste
            string response = _pipe.SendCommand("CHARINFO");
            
            // Oyunda değilse
            if (response.StartsWith("CHARINFO:OFFLINE"))
            {
                lblName.Text = "Karakter: (Oyunda değil)";
                this.Text = "GameHackingTools";
                return;
            }
            
            // Bilgiyi parse et
            if (response.StartsWith("CHARINFO:"))
            {
                ParseCharInfo(response.Substring(9));
            }
        }
        
        /// <summary>
        /// CHARINFO yanıtını parse et ve UI'ı güncelle
        /// </summary>
        /// <param name="data">Format: İsim|LV=83|CL=108|HP=5000/5000|MP=1000/1000|X=100|Y=200</param>
        private void ParseCharInfo(string data)
        {
            // "|" ile ayır
            string[] parts = data.Split('|');
            
            // İlk kısım karakter adı
            if (parts.Length > 0)
            {
                string charName = parts[0];
                lblName.Text = "Karakter: " + charName;
                
                // Pencere başlığını güncelle (karakter adı değiştiyse)
                if (charName != _currentCharName)
                {
                    _currentCharName = charName;
                    this.Text = $"GameHackingTools - {charName}";
                }
            }
            
            // Diğer değerleri parse et
            foreach (string part in parts)
            {
                if (part.StartsWith("LV="))
                {
                    lblLevel.Text = "Level: " + part.Substring(3);
                }
                else if (part.StartsWith("CL="))
                {
                    lblClass.Text = "Sınıf: " + GetClassName(part.Substring(3));
                }
                else if (part.StartsWith("HP="))
                {
                    string[] hp = part.Substring(3).Split('/');
                    if (hp.Length == 2)
                    {
                        lblHP.Text = part.Substring(3);
                        int cur = int.Parse(hp[0]);
                        int max = int.Parse(hp[1]);
                        barHP.Value = max > 0 ? Math.Min(100, cur * 100 / max) : 0;
                    }
                }
                else if (part.StartsWith("MP="))
                {
                    string[] mp = part.Substring(3).Split('/');
                    if (mp.Length == 2)
                    {
                        lblMP.Text = part.Substring(3);
                        int cur = int.Parse(mp[0]);
                        int max = int.Parse(mp[1]);
                        barMP.Value = max > 0 ? Math.Min(100, cur * 100 / max) : 0;
                    }
                }
                else if (part.StartsWith("X="))
                {
                    string x = part.Substring(2);
                    string y = "0";
                    foreach (string p in parts)
                        if (p.StartsWith("Y=")) y = p.Substring(2);
                    lblPosition.Text = $"Pozisyon: X={x}, Y={y}";
                }
            }
        }
        
        /// <summary>
        /// Sınıf kodunu okunabilir isme çevir
        /// </summary>
        private string GetClassName(string classId)
        {
            switch (classId)
            {
                case "101": return "Warrior (K)";
                case "102": return "Rogue (K)";
                case "103": return "Mage (K)";
                case "104": return "Priest (K)";
                case "108": return "Master (K)";
                case "201": return "Warrior (E)";
                case "202": return "Rogue (E)";
                case "203": return "Mage (E)";
                case "204": return "Priest (E)";
                case "208": return "Master (E)";
                default: return classId;
            }
        }
        
        // ====================================================================
        // BUTON OLAYLARI
        // ====================================================================
        
        /// <summary>
        /// Town butonuna tıklandı
        /// </summary>
        private void BtnTown_Click(object sender, EventArgs e)
        {
            Log("Town komutu gönderiliyor...");
            
            // DLL'e TOWN komutu gönder
            string response = _pipe.SendCommand("TOWN");
            
            Log("Yanıt: " + response);
        }
        
        // ====================================================================
        // LOG FONKSİYONU
        // ====================================================================
        
        /// <summary>
        /// Log listesine mesaj ekle
        /// </summary>
        private void Log(string message)
        {
            // UI thread'inde çalış
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message)));
                return;
            }
            
            // Zaman damgası ile ekle (en üste)
            lstLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            
            // Max 50 satır tut
            while (lstLog.Items.Count > 50)
                lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
        }
        
        // ====================================================================
        // FORM KAPANIŞI
        // ====================================================================
        
        /// <summary>
        /// Form kapanırken kaynakları temizle
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer?.Stop();
            _pipe?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
