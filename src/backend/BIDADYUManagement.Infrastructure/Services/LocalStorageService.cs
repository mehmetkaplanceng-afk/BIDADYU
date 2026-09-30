using System.Security.Cryptography;
using BIDADYUManagement.Application.Common.Interfaces;

namespace BIDADYUManagement.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _baseStoragePath;

    public LocalStorageService()
    {
        // Diskinizde tahsis edilen merkezi bulut depolama dizini
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "storage", "packages");
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<(string filePath, string fileHash, long fileSize)> SavePackageAsync(Stream stream, string fileName)
    {
        string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        string fullPath = Path.Combine(_baseStoragePath, uniqueFileName);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream);
        }

        long fileSize = new FileInfo(fullPath).Length;
        string fileHash = ComputeSha256(fullPath);

        string relativePath = Path.Combine("storage", "packages", uniqueFileName).Replace("\\", "/");
        return (relativePath, fileHash, fileSize);
    }

    public bool DeletePackage(string relativePath)
    {
        string fullPath = GetPhysicalPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }

    public string GetPhysicalPath(string relativePath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace("/", "\\"));
    }

    private string ComputeSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
