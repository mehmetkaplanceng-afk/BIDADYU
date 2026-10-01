using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIDADYUAgent;

public class SupportForm : Form
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;
    private readonly Action _onTicketSent;
    private readonly Action<string, string>? _showBalloonNotification;
    private string _lastTicketStatus = "";

    private Label lblHeader;
    private Label lblComputerName;
    private Label lblTicketType;
    private ComboBox cmbTicketType;
    private Label lblMessage;
    private TextBox txtMessage;
    private Button btnSend;
    private Panel statusPanel;
    private Label lblStatusHeader;
    private Label lblStatusDetail;
    private System.Windows.Forms.Timer statusTimer;

    public SupportForm(HttpClient httpClient, AppSettings appSettings, Action onTicketSent, Action<string, string>? showBalloonNotification = null)
    {
        _httpClient = httpClient;
        _appSettings = appSettings;
        _onTicketSent = onTicketSent;
        _showBalloonNotification = showBalloonNotification;

        InitializeComponent();
        FetchCurrentTicketStatus();

        statusTimer = new System.Windows.Forms.Timer();
        statusTimer.Interval = 5000; // 5 saniyede bir durumu sorgula
        statusTimer.Tick += async (s, e) => await FetchCurrentTicketStatus();
        statusTimer.Start();
    }

    private void InitializeComponent()
    {
        this.Text = "BİDB - Bilgisayar Destek & Bildirim Portalı";
        try
        {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
            if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
            {
                var extracted = Icon.ExtractAssociatedIcon(exePath);
                if (extracted != null) this.Icon = extracted;
            }
        }
        catch { }
        this.Size = new Size(460, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.FromArgb(248, 250, 252);

        // Header Panel
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(0, 21, 41)
        };

        lblHeader = new Label
        {
            Text = "BİDADYU BT Destek & Bildirim",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            Location = new Point(16, 12),
            AutoSize = true
        };

        string deviceIdShort = !string.IsNullOrEmpty(_appSettings.DeviceId) && _appSettings.DeviceId.Length >= 8 
            ? _appSettings.DeviceId.Substring(0, 8).ToUpper() 
            : (_appSettings.DeviceId?.ToUpper() ?? "YENİ");

        lblComputerName = new Label
        {
            Text = $"🖥️ PC Adı: {Environment.MachineName} (ID: #{deviceIdShort})",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            Location = new Point(18, 40),
            AutoSize = true
        };

        headerPanel.Controls.Add(lblHeader);
        headerPanel.Controls.Add(lblComputerName);
        this.Controls.Add(headerPanel);

        // Form Fields
        int top = 85;

        lblTicketType = new Label
        {
            Text = "Bildirim / Talep Türü:",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, top),
            AutoSize = true
        };
        this.Controls.Add(lblTicketType);

        top += 24;
        cmbTicketType = new ComboBox
        {
            Location = new Point(20, top),
            Width = 404,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        cmbTicketType.Items.Add(new TicketOption("🛠️ Arıza Kaydı (Ekran/Donanım/Ağ/Sistem)", "HardwareIssue"));
        cmbTicketType.Items.Add(new TicketOption("📦 Eksik Program / Yazılım Talebi", "SoftwareRequest"));
        cmbTicketType.SelectedIndex = 0;
        this.Controls.Add(cmbTicketType);

        top += 38;
        lblMessage = new Label
        {
            Text = "Açıklama & Detaylar:",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(20, top),
            AutoSize = true
        };
        this.Controls.Add(lblMessage);

        top += 24;
        txtMessage = new TextBox
        {
            Location = new Point(20, top),
            Width = 404,
            Height = 90,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 9.5f)
        };
        this.Controls.Add(txtMessage);

        top += 102;
        btnSend = new Button
        {
            Text = "🚀 Bildirimi Yönetime İlet",
            Location = new Point(20, top),
            Width = 404,
            Height = 40,
            BackColor = Color.FromArgb(22, 119, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSend.FlatAppearance.BorderSize = 0;
        btnSend.Click += async (s, e) => await SendTicketAsync();
        this.Controls.Add(btnSend);

        // Status Feedback Panel (Canlı Dönüt Alanı)
        top += 52;
        statusPanel = new Panel
        {
            Location = new Point(20, top),
            Width = 404,
            Height = 85,
            BackColor = Color.FromArgb(241, 245, 249),
            BorderStyle = BorderStyle.FixedSingle
        };

        lblStatusHeader = new Label
        {
            Text = "📋 Son Talebinizin Durumu:",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            Location = new Point(10, 8),
            AutoSize = true
        };

        lblStatusDetail = new Label
        {
            Text = "Henüz gönderilmiş bir bildirim bulunmuyor.",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(10, 30),
            Size = new Size(380, 48)
        };

        statusPanel.Controls.Add(lblStatusHeader);
        statusPanel.Controls.Add(lblStatusDetail);
        this.Controls.Add(statusPanel);
    }

    private async Task SendTicketAsync()
    {
        if (string.IsNullOrWhiteSpace(txtMessage.Text))
        {
            MessageBox.Show("Lütfen sorununuzu veya talebinizi açıklayan bir mesaj giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selected = cmbTicketType.SelectedItem as TicketOption;
        string ticketType = selected?.Value ?? "HardwareIssue";

        btnSend.Enabled = false;
        btnSend.Text = "Gönderiliyor...";

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Agent-Token", _appSettings.Token);

            var req = new
            {
                TicketType = ticketType,
                Message = txtMessage.Text.Trim()
            };

            var response = await _httpClient.PostAsJsonAsync($"{_appSettings.ServerUrl}/api/Agent/ticket", req);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Talebiniz başarıyla Sistem Yöneticisine iletildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtMessage.Clear();
                _onTicketSent?.Invoke();
                await FetchCurrentTicketStatus();
            }
            else
            {
                MessageBox.Show("Bildirim gönderilemedi. Sunucu bağlantısını kontrol ediniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gönderim hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSend.Enabled = true;
            btnSend.Text = "🚀 Bildirimi Yönetime İlet";
        }
    }

    private async Task FetchCurrentTicketStatus()
    {
        try
        {
            if (string.IsNullOrEmpty(_appSettings.Token)) return;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Agent-Token", _appSettings.Token);

            var response = await _httpClient.GetAsync($"{_appSettings.ServerUrl}/api/Agent/ticket-status");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TicketStatusResponse>();
                if (result != null && result.HasTicket)
                {
                    string statusText = "";
                    Color statusBgColor = Color.White;

                    if (result.Status == "InProgress")
                    {
                        statusText = $"🛠️ TALEBİNİZ ALINDI, ŞU AN İLGİLENİLİYOR!\n({result.Title})\nMesaj: {result.Message}";
                        statusBgColor = Color.FromArgb(219, 234, 254); // Açık mavi
                    }
                    else if (result.Status == "Completed")
                    {
                        statusText = $"✅ TALEBİNİZ TAMAMLANDI!\n({result.Title})";
                        statusBgColor = Color.FromArgb(220, 252, 231); // Açık yeşil
                    }
                    else
                    {
                        statusText = $"⏳ Yönetim Onayı Bekliyor...\n({result.Title})";
                        statusBgColor = Color.FromArgb(254, 249, 195); // Açık sarı
                    }

                    lblStatusDetail.Text = statusText;
                    lblStatusDetail.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    statusPanel.BackColor = statusBgColor;
                }
            }
        }
        catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide(); // X butonuna basınca uygulamayı kapatma, sadece gizle (Tray'de kalsın)
        }
        base.OnFormClosing(e);
    }
}

public class TicketOption
{
    public string Display { get; set; }
    public string Value { get; set; }

    public TicketOption(string display, string value)
    {
        Display = display;
        Value = value;
    }

    public override string ToString() => Display;
}

public class TicketStatusResponse
{
    public bool HasTicket { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
