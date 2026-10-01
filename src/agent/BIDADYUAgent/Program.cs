using BIDADYUAgent;
using Microsoft.Extensions.Options;

// Windows Forms + Background Service birlikte çalışacak şekilde Application başlatılır
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Tray uygulamasını ayrı Thread'de çalıştır (WinForms STA gerektirir)
SupportForm? supportForm = null;
NotifyIcon? trayIcon = null;

var hostTask = host.RunAsync();

// Kısa bekleme — AppSettings yüklenip Token oluşana kadar
await Task.Delay(1500);

var appSettings = host.Services.GetRequiredService<IOptions<AppSettings>>().Value;
var httpClient = host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();

var trayThread = new Thread(() =>
{
    Application.SetHighDpiMode(HighDpiMode.SystemAware);

    Icon customIcon = SystemIcons.Information;
    try
    {
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            var extracted = Icon.ExtractAssociatedIcon(exePath);
            if (extracted != null) customIcon = extracted;
        }
    }
    catch { }

    trayIcon = new NotifyIcon
    {
        Text = $"BIDADYU Agent — {Environment.MachineName}",
        Visible = true,
        Icon = customIcon,
    };

    supportForm = new SupportForm(httpClient, appSettings, () =>
    {
        trayIcon.ShowBalloonTip(3000, "Bildirim Gönderildi", "Talebiniz Sistem Yöneticisine iletildi!", ToolTipIcon.Info);
    }, (title, message) =>
    {
        trayIcon.ShowBalloonTip(4000, title, message, ToolTipIcon.Info);
    });

    var contextMenu = new ContextMenuStrip();
    contextMenu.Items.Add("🔔 Destek & Bildirim Formu", null, (s, e) =>
    {
        if (supportForm != null)
        {
            supportForm.Show();
            supportForm.BringToFront();
            supportForm.WindowState = FormWindowState.Normal;
        }
    });
    contextMenu.Items.Add("-");
    contextMenu.Items.Add("🖥️ PC Bilgisi", null, (s, e) =>
    {
        string devIdShort = !string.IsNullOrEmpty(appSettings.DeviceId) && appSettings.DeviceId.Length >= 8 
            ? appSettings.DeviceId.Substring(0, 8).ToUpper() 
            : (appSettings.DeviceId?.ToUpper() ?? "YENİ");
        MessageBox.Show(
            $"Bilgisayar Adı: {Environment.MachineName}\nCihaz ID: #{devIdShort}\nSunucu: {appSettings.ServerUrl}",
            "BIDADYU Agent - PC Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    });
    contextMenu.Items.Add("-");
    var consoleMenuItem = new ToolStripMenuItem("💻 Konsol Penceresini Göster/Gizle");
    consoleMenuItem.Click += (s, e) =>
    {
        ConsoleHelper.ToggleConsole();
    };
    contextMenu.Items.Add(consoleMenuItem);
    contextMenu.Items.Add("-");
    contextMenu.Items.Add("❌ Çıkış", null, (s, e) =>
    {
        trayIcon.Visible = false;
        host.StopAsync();
        Application.Exit();
    });

    trayIcon.ContextMenuStrip = contextMenu;
    trayIcon.DoubleClick += (s, e) =>
    {
        if (supportForm != null)
        {
            supportForm.Show();
            supportForm.BringToFront();
            supportForm.WindowState = FormWindowState.Normal;
        }
    };

    trayIcon.ShowBalloonTip(2000, "BIDADYU Agent Başlatıldı",
        $"Agent çalışıyor. Arıza ya da eksik program bildirimi için çift tıklayın.\n🖥️ {Environment.MachineName}", ToolTipIcon.Info);

    Application.Run(); // WinForms mesaj döngüsü
});

trayThread.SetApartmentState(ApartmentState.STA);
trayThread.IsBackground = true;
trayThread.Start();

await hostTask;
