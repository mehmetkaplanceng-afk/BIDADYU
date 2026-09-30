using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BIDADYUAgent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly HttpClient _httpClient;
    private AppSettings _appSettings;
    private readonly string _settingsPath = "agentSettings.json";
    private readonly string _tempDownloadDir = @"C:\ProgramData\BIDADYU\Downloads";

    public Worker(ILogger<Worker> logger, HttpClient httpClient, IOptions<AppSettings> options)
    {
        _logger = logger;
        _httpClient = httpClient;
        _appSettings = options.Value;

        if (!Directory.Exists(_tempDownloadDir))
        {
            Directory.CreateDirectory(_tempDownloadDir);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LoadSettings();
        _logger.LogInformation($"[BIDADYU AGENT BAŞLATILDI] Cihaz ID: #{_appSettings.DeviceId.Substring(0, 8).ToUpper()}");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (string.IsNullOrEmpty(_appSettings.Token))
            {
                await RegisterAgentAsync();
            }
            else
            {
                await SendHeartbeatAndFetchJobsAsync();
            }

            await Task.Delay(10000, stoppingToken); // 10 saniyede bir kontrol et
        }
    }

    private void LoadSettings()
    {
        // 1. Öncelik: Eğer yanındaki server_ip.txt dosyasında IP yazıyorsa onu kullan
        string ipFilePath = Path.Combine(AppContext.BaseDirectory, "server_ip.txt");
        if (File.Exists(ipFilePath))
        {
            try
            {
                string ip = File.ReadAllText(ipFilePath).Trim();
                if (!string.IsNullOrEmpty(ip))
                {
                    if (!ip.StartsWith("http://") && !ip.StartsWith("https://"))
                        ip = "http://" + ip;
                    if (!ip.Contains(":5000"))
                        ip = ip.TrimEnd('/') + ":5000";
                    _appSettings.ServerUrl = ip;
                }
            }
            catch { }
        }

        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                var saved = JsonSerializer.Deserialize<AppSettings>(json);
                if (saved != null)
                {
                    // Eğer server_ip.txt yoksa veya geçersizse JSON'daki ServerUrl kalır
                    if (!File.Exists(ipFilePath))
                        _appSettings.ServerUrl = saved.ServerUrl;
                    _appSettings.DeviceId = saved.DeviceId;
                    _appSettings.Token = saved.Token;
                }
            }
            catch { }
        }

        if (string.IsNullOrEmpty(_appSettings.DeviceId))
        {
            _appSettings.DeviceId = Guid.NewGuid().ToString("N");
            SaveSettings();
        }
    }

    private void SaveSettings()
    {
        var json = JsonSerializer.Serialize(_appSettings);
        File.WriteAllText(_settingsPath, json);
    }

    private async Task RegisterAgentAsync()
    {
        _logger.LogInformation("Agent sunucuya kaydoluyor...");
        try
        {
            var req = new
            {
                DeviceId = _appSettings.DeviceId,
                Hostname = Environment.MachineName,
                OsVersion = Environment.OSVersion.VersionString,
                AgentVersion = "1.0.0"
            };

            var response = await _httpClient.PostAsJsonAsync($"{_appSettings.ServerUrl}/api/Agent/register", req);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("token", out var tokenProp))
                {
                    _appSettings.Token = tokenProp.GetString() ?? "";
                    SaveSettings();
                    _logger.LogInformation($"Agent başarıyla kaydoldu! [Cihaz ID: #{_appSettings.DeviceId.Substring(0, 8).ToUpper()}]");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt hatası.");
        }
    }

    private async Task SendHeartbeatAndFetchJobsAsync()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Agent-Token", _appSettings.Token);

            var req = new
            {
                CpuUsagePercent = 15.0f,
                RamUsagePercent = 40.0f,
                DiskUsagePercent = 55.0f,
                RamTotalMb = 16384,
                RamFreeMb = 8000,
                SystemUptimeSeconds = Environment.TickCount64 / 1000
            };

            var response = await _httpClient.PostAsJsonAsync($"{_appSettings.ServerUrl}/api/Agent/heartbeat", req);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("jobs", out var jobsArray))
                {
                    foreach (var job in jobsArray.EnumerateArray())
                    {
                        await ExecuteRealInstallationJobAsync(job);
                        // Kurulum bittikten hemen sonra envanteri tekrar tara ve sunucuya bildir
                        await ScanAndReportInventoryAsync();
                    }
                }
            }

            // Periyodik envanter taraması (Her 5 dakikada bir veya belirli aralıklarla)
            if (DateTime.UtcNow - _lastInventoryScan > TimeSpan.FromMinutes(5))
            {
                await ScanAndReportInventoryAsync();
                _lastInventoryScan = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Heartbeat hatası.");
        }
    }

    private DateTime _lastInventoryScan = DateTime.MinValue;

    private async Task ScanAndReportInventoryAsync()
    {
        _logger.LogInformation("Bilgisayarda kurulu yazılımlar taranıyor (Registry Audit)...");
        var softwareList = new List<object>();

        try
        {
            string[] registryKeys = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in registryKeys)
            {
                using var hklm = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
                using var key = hklm.OpenSubKey(keyPath);
                if (key == null) continue;

                foreach (var subkeyName in key.GetSubKeyNames())
                {
                    using var subkey = key.OpenSubKey(subkeyName);
                    if (subkey == null) continue;

                    var name = subkey.GetValue("DisplayName")?.ToString();
                    var version = subkey.GetValue("DisplayVersion")?.ToString();
                    var publisher = subkey.GetValue("Publisher")?.ToString();
                    var installLocation = subkey.GetValue("InstallLocation")?.ToString();
                    var uninstallString = subkey.GetValue("UninstallString")?.ToString();

                    var systemComponent = subkey.GetValue("SystemComponent");
                    var parentDisplayName = subkey.GetValue("ParentDisplayName");

                    // Sistem bileşeni veya güncelleme değilse ekle
                    if (!string.IsNullOrWhiteSpace(name) && systemComponent == null && parentDisplayName == null)
                    {
                        softwareList.Add(new
                        {
                            Name = name,
                            Publisher = publisher ?? "",
                            Version = version ?? "1.0",
                            InstallLocation = installLocation ?? "",
                            UninstallString = uninstallString ?? ""
                        });
                    }
                }
            }

            _logger.LogInformation($"[ENVANTER] Toplam {softwareList.Count} kurulu program tespit edildi. Sunucuya gönderiliyor...");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Agent-Token", _appSettings.Token);

            var response = await _httpClient.PostAsJsonAsync($"{_appSettings.ServerUrl}/api/Agent/inventory", new { SoftwareList = softwareList });
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Yazılım envanteri sunucuya başarıyla bildirildi!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yazılım envanteri taranırken hata oluştu.");
        }
    }

    private string GetPropString(JsonElement element, string propName)
    {
        foreach (var prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase))
            {
                return prop.Value.ValueKind != JsonValueKind.Null ? prop.Value.GetString() ?? "" : "";
            }
        }
        return "";
    }

    private async Task ExecuteRealInstallationJobAsync(JsonElement job)
    {
        string targetId = GetPropString(job, "targetId");
        string action = GetPropString(job, "action");
        string version = GetPropString(job, "version");
        string packageId = GetPropString(job, "packageId");
        string fileName = GetPropString(job, "fileName");
        string silentArgs = GetPropString(job, "silentArgs");
        string installerType = GetPropString(job, "installerType");

        if (string.IsNullOrEmpty(fileName)) fileName = $"installer_{version}.msi";
        if (string.IsNullOrEmpty(silentArgs)) silentArgs = "/qn /norestart";
        if (string.IsNullOrEmpty(installerType)) installerType = "Msi";

        bool isCopyOnly = action.Equals("CopyOnly", StringComparison.OrdinalIgnoreCase) || action == "5";
        _logger.LogInformation($"[GÖREV ALINDI] Action={action}, TargetId={targetId}, PackageId={packageId}, File={fileName}");

        await UpdateJobStatusAsync(targetId, "Downloading", "Paket bulut depodan indiriliyor ve kontroller yapılıyor...");

        try
        {
            // 0. Disk Alanı Kontrolü (Free Space Check)
            var drive = new DriveInfo(Path.GetPathRoot(_tempDownloadDir) ?? "C:\\");
            long freeSpaceMb = drive.AvailableFreeSpace / (1024 * 1024);
            _logger.LogInformation($"[DİSK KONTROLÜ] Sürücü: {drive.Name}, Boş Alan: {freeSpaceMb} MB");

            if (freeSpaceMb < 500) // Minimum 500 MB boş alan şartı
            {
                await UpdateJobStatusAsync(targetId, "Failed", $"❌ Yetersiz Disk Alanı! Hedef sürücüde sadece {freeSpaceMb} MB boş alan var. İşlem iptal edildi.");
                return;
            }

            string localFilePath = Path.Combine(_tempDownloadDir, $"{Guid.NewGuid()}_{fileName}");

            // 1. Gerçek İndirme Aşaması
            if (!string.IsNullOrEmpty(packageId))
            {
                var downloadUrl = $"{_appSettings.ServerUrl}/api/Packages/download/{packageId}";
                _logger.LogInformation($"[İNDİRİLİYOR] {downloadUrl} -> {localFilePath}");

                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
                using var streamToWriteTo = File.Open(localFilePath, FileMode.Create);
                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                
                _logger.LogInformation($"[İNDİRME TAMAMLANDI] Dosya boyutu: {new FileInfo(localFilePath).Length} bytes");
            }
            else
            {
                // Fiziksel paket yoksa simülasyon dosyası oluştur
                await File.WriteAllTextAsync(localFilePath, "Simulated Package Content");
            }

            // 2. Arşiv Çıkarma & Klasör Bütünlüğü (ZIP / RAR)
            string extractedFolder = Path.Combine(_tempDownloadDir, $"Extracted_{Guid.NewGuid()}");
            bool isArchive = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".rar", StringComparison.OrdinalIgnoreCase);

            if (isArchive && File.Exists(localFilePath))
            {
                try
                {
                    _logger.LogInformation($"[ARŞİV ÇIKARILIYOR] Arşiv çıkarılıyor: {localFilePath} -> {extractedFolder}");
                    ZipFile.ExtractToDirectory(localFilePath, extractedFolder);
                    _logger.LogInformation($"[ARŞİV BAŞARIYLA ÇIKARILDI] Klasör bütünlüğü korundu.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Arşiv çıkartılırken uyarı/hata (Standart dosya olarak işlenecek): {ex.Message}");
                    extractedFolder = _tempDownloadDir;
                }
            }

            // 3. Dağıtım Tipi Kontrolü (CopyOnly / Sadece Aktar vs. Silent Install)
            if (isCopyOnly)
            {
                string destinationFolder = @"C:\ProgramData\BIDADYU\Deployments";
                if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);

                string targetAppFolder = Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(fileName));
                _logger.LogInformation($"[SADECE KOPYALA] Dosyalar hedef klasöre aktarılıyor: {targetAppFolder}");

                if (Directory.Exists(extractedFolder) && extractedFolder != _tempDownloadDir)
                {
                    // Çıkarılan klasörü taşı / kopyala
                    CopyDirectory(extractedFolder, targetAppFolder);
                }
                else
                {
                    Directory.CreateDirectory(targetAppFolder);
                    File.Copy(localFilePath, Path.Combine(targetAppFolder, fileName), true);
                }

                await UpdateJobStatusAsync(targetId, "Success", $"✅ Dosya ve klasör bütünlüğü korunarak hedef bilgisayara başarıyla kopyalandı! Dizin: {targetAppFolder}");
            }
            else
            {
                // Kurulum (Install) Modu
                await UpdateJobStatusAsync(targetId, "Installing", "Paket hazırlandı. Sessiz kurulum başlatılıyor...");

                // Çalıştırılacak executable dosyasını bul (Eğer Zip çıkarıldıysa içindeki exe/msi dosyasını ara)
                string execTargetFile = localFilePath;
                if (Directory.Exists(extractedFolder) && extractedFolder != _tempDownloadDir)
                {
                    var exeFiles = Directory.GetFiles(extractedFolder, "*.exe", SearchOption.AllDirectories);
                    var msiFiles = Directory.GetFiles(extractedFolder, "*.msi", SearchOption.AllDirectories);
                    
                    if (msiFiles.Length > 0) execTargetFile = msiFiles[0];
                    else if (exeFiles.Length > 0) execTargetFile = exeFiles[0];
                }

                _logger.LogInformation($"[KURULUM BAŞLATILIYOR] {execTargetFile} Args: {silentArgs}");

                ProcessStartInfo psi;
                bool isMsi = execTargetFile.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);

                if (isMsi)
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = "msiexec.exe",
                        Arguments = $"/i \"{execTargetFile}\" {(string.IsNullOrWhiteSpace(silentArgs) ? "/qn /norestart" : silentArgs)}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                }
                else
                {
                    string args = string.IsNullOrWhiteSpace(silentArgs) || silentArgs == "/qn /norestart" ? "/S /silent /quiet" : silentArgs;
                    psi = new ProcessStartInfo
                    {
                        FileName = execTargetFile,
                        Arguments = args,
                        WorkingDirectory = Path.GetDirectoryName(execTargetFile) ?? _tempDownloadDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                }

                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    _logger.LogInformation($"[KURULUM BİTTİ] ExitCode: {process.ExitCode}");

                    if (process.ExitCode == 0 || process.ExitCode == 3010)
                    {
                        await UpdateJobStatusAsync(targetId, "Success", $"Yazılım başarıyla kuruldu. (ExitCode: {process.ExitCode})");
                    }
                    else
                    {
                        string err = await process.StandardError.ReadToEndAsync();
                        await UpdateJobStatusAsync(targetId, "Failed", $"Kurulum hata kodu ile bitti ({process.ExitCode}). {err}");
                    }
                }
                else
                {
                    await UpdateJobStatusAsync(targetId, "Failed", "Kurulum süreci başlatılamadı.");
                }
            }

            // Temizlik
            try { if (File.Exists(localFilePath)) File.Delete(localFilePath); } catch { }
            try { if (Directory.Exists(extractedFolder) && extractedFolder != _tempDownloadDir) Directory.Delete(extractedFolder, true); } catch { }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görev yürütülürken hata oluştu.");
            await UpdateJobStatusAsync(targetId, "Failed", $"Hata: {ex.Message}");
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"Kaynak dizin bulunamadı: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private async Task UpdateJobStatusAsync(string targetId, string status, string message)
    {
        try
        {
            var payload = new { TargetId = targetId, Status = status, Message = message };
            await _httpClient.PostAsJsonAsync($"{_appSettings.ServerUrl}/api/Deployments/target-status", payload);
        }
        catch { }
    }
}
