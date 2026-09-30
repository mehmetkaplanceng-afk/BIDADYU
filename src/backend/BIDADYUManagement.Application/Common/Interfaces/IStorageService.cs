namespace BIDADYUManagement.Application.Common.Interfaces;

public interface IStorageService
{
    Task<(string filePath, string fileHash, long fileSize)> SavePackageAsync(Stream stream, string fileName);
    bool DeletePackage(string relativePath);
    string GetPhysicalPath(string relativePath);
}
