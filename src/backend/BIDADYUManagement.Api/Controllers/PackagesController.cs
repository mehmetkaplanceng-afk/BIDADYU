using BIDADYUManagement.Application.Common.Interfaces;
using BIDADYUManagement.Domain.Entities.Software;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public PackagesController(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPackages()
    {
        var packages = await _context.SoftwarePackages
            .Include(p => p.SoftwareVersion)
                .ThenInclude(v => v.Software)
                    .ThenInclude(s => s.Category)
            .Select(p => new
            {
                p.Id,
                p.FileName,
                SoftwareId = p.SoftwareVersion != null ? p.SoftwareVersion.SoftwareId : (Guid?)null,
                SoftwareVersionId = p.SoftwareVersionId,
                SoftwareName = p.SoftwareVersion != null && p.SoftwareVersion.Software != null ? p.SoftwareVersion.Software.Name : "Genel Paket",
                CategoryName = p.SoftwareVersion != null && p.SoftwareVersion.Software != null && p.SoftwareVersion.Software.Category != null ? p.SoftwareVersion.Software.Category.Name : "Genel",
                Version = p.SoftwareVersion != null ? p.SoftwareVersion.Version : "1.0",
                p.InstallerType,
                p.Architecture,
                p.SilentInstallArgs,
                p.Sha256Hash,
                p.FileSizeBytes,
                p.FilePath,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(packages);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPackage([FromForm] Guid? softwareVersionId, [FromForm] IFormFile file, [FromForm] string? silentArgs)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "Lütfen bir dosya yükleyin." });

        if (!softwareVersionId.HasValue || softwareVersionId.Value == Guid.Empty)
        {
            var defaultSoftware = await _context.Softwares.Include(s => s.Versions).FirstOrDefaultAsync();
            if (defaultSoftware == null)
            {
                defaultSoftware = new Software { Id = Guid.NewGuid(), Name = "Genel Yazılımlar", Publisher = "Sistem" };
                await _context.Softwares.AddAsync(defaultSoftware);
            }

            var defaultVersion = defaultSoftware.Versions.FirstOrDefault();
            if (defaultVersion == null)
            {
                defaultVersion = new SoftwareVersion { Id = Guid.NewGuid(), SoftwareId = defaultSoftware.Id, Version = "1.0.0", IsCurrent = true };
                await _context.SoftwareVersions.AddAsync(defaultVersion);
            }
            await _context.SaveChangesAsync();
            softwareVersionId = defaultVersion.Id;
        }

        using var stream = file.OpenReadStream();
        var (filePath, fileHash, fileSize) = await _storageService.SavePackageAsync(stream, file.FileName);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var installerType = ext == ".msi" ? InstallerType.Msi : InstallerType.Exe;

        var package = new SoftwarePackage
        {
            Id = Guid.NewGuid(),
            SoftwareVersionId = softwareVersionId.Value,
            FileName = file.FileName,
            FilePath = filePath,
            InstallerType = installerType,
            Architecture = Architecture.X64,
            SilentInstallArgs = silentArgs ?? (installerType == InstallerType.Msi ? "/qn /norestart" : "/S"),
            Sha256Hash = fileHash,
            FileSizeBytes = fileSize,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SoftwarePackages.AddAsync(package);
        await _context.SaveChangesAsync();

        return Ok(package);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdatePackageRequest request)
    {
        var package = await _context.SoftwarePackages.FindAsync(id);
        if (package == null) return NotFound("Paket bulunamadı.");

        if (request.SoftwareVersionId.HasValue && request.SoftwareVersionId.Value != Guid.Empty)
        {
            package.SoftwareVersionId = request.SoftwareVersionId.Value;
        }

        if (!string.IsNullOrEmpty(request.SilentArgs))
        {
            package.SilentInstallArgs = request.SilentArgs;
        }

        await _context.SaveChangesAsync();
        return Ok(package);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var package = await _context.SoftwarePackages.FindAsync(id);
        if (package == null) return NotFound("Paket bulunamadı.");

        _context.SoftwarePackages.Remove(package);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("download/{packageId}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadPackage(Guid packageId)
    {
        var package = await _context.SoftwarePackages.FindAsync(packageId);
        if (package == null) return NotFound("Paket veritabanında bulunamadı.");

        var fullPath = Path.IsPathRooted(package.FilePath) 
            ? package.FilePath 
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", package.FilePath);

        if (string.IsNullOrEmpty(package.FilePath) || !System.IO.File.Exists(fullPath))
            return NotFound($"Fiziksel paket dosyası sunucu diskinde bulunamadı ({fullPath}).");

        var contentType = package.InstallerType == InstallerType.Msi ? "application/x-msi" : "application/octet-stream";
        return PhysicalFile(fullPath, contentType, package.FileName);
    }
}

public class UpdatePackageRequest
{
    public Guid? SoftwareVersionId { get; set; }
    public string? SilentArgs { get; set; }
}
