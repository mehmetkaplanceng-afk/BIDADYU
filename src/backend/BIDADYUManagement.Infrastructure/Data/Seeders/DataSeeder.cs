using BIDADYUManagement.Domain.Entities.Auth;
using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Entities.Software;
using BIDADYUManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BIDADYUManagement.Infrastructure.Data.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync()) return; // Already seeded

        // 1. Roles
        var roles = new List<Role>
        {
            new Role { Id = Guid.NewGuid(), Name = Roles.SuperAdmin, Description = "Tam yetkili sistem yöneticisi" },
            new Role { Id = Guid.NewGuid(), Name = Roles.SystemAdmin, Description = "Sistem yöneticisi" },
            new Role { Id = Guid.NewGuid(), Name = Roles.ITOperator, Description = "BT Operatörü" },
            new Role { Id = Guid.NewGuid(), Name = Roles.Viewer, Description = "Sadece görüntüleme" }
        };
        await context.Roles.AddRangeAsync(roles);

        // 2. Users
        var adminId = Guid.NewGuid();
        var adminUser = new User
        {
            Id = adminId,
            Username = "admin",
            Email = "admin@university.edu.tr",
            FirstName = "Sistem",
            LastName = "Yöneticisi",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsActive = true
        };
        await context.Users.AddAsync(adminUser);

        var userRole = new UserRole
        {
            UserId = adminId,
            RoleId = roles.First(r => r.Name == Roles.SuperAdmin).Id,
            AssignedBy = adminId
        };
        await context.UserRoles.AddAsync(userRole);

        // 3. Categories & Software
        var browserCategory = new SoftwareCategory { Id = Guid.NewGuid(), Name = "Browser" };
        var toolsCategory = new SoftwareCategory { Id = Guid.NewGuid(), Name = "Araçlar" };
        await context.SoftwareCategories.AddRangeAsync(browserCategory, toolsCategory);

        var chromeId = Guid.NewGuid();
        var chrome = new Software { Id = chromeId, Name = "Google Chrome", Publisher = "Google", CategoryId = browserCategory.Id };
        var zip7Id = Guid.NewGuid();
        var zip7 = new Software { Id = zip7Id, Name = "7-Zip", Publisher = "Igor Pavlov", CategoryId = toolsCategory.Id };
        await context.Softwares.AddRangeAsync(chrome, zip7);

        // 4. Versions
        var chromeVersionId = Guid.NewGuid();
        var chromeVersion = new SoftwareVersion { Id = chromeVersionId, SoftwareId = chromeId, Version = "120.0", IsCurrent = true };
        await context.SoftwareVersions.AddAsync(chromeVersion);

        // 5. Packages
        var chromePackage = new SoftwarePackage
        {
            Id = Guid.NewGuid(),
            SoftwareVersionId = chromeVersionId,
            FileName = "ChromeSetup.msi",
            InstallerType = InstallerType.Msi,
            SilentInstallArgs = "/qn /norestart",
            Sha256Hash = "dummyhash"
        };
        await context.SoftwarePackages.AddAsync(chromePackage);

        // 6. Computer Groups
        var lab1Id = Guid.NewGuid();
        var lab1 = new ComputerGroup { Id = lab1Id, Name = "Bilgisayar Laboratuvarı 1" };
        await context.ComputerGroups.AddAsync(lab1);

        // 7. Computers
        var pc1Id = Guid.NewGuid();
        var pc1 = new Computer { Id = pc1Id, Hostname = "PC-LAB1-01", Status = ComputerStatus.Online };
        await context.Computers.AddAsync(pc1);

        await context.ComputerGroupMembers.AddAsync(new ComputerGroupMember { ComputerId = pc1Id, GroupId = lab1Id });

        await context.SaveChangesAsync();
    }
}
