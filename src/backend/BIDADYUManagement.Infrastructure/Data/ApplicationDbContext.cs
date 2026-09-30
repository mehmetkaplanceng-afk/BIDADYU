using BIDADYUManagement.Domain.Entities.Auth;
using BIDADYUManagement.Domain.Entities.Compliance;
using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Entities.Deployments;
using BIDADYUManagement.Domain.Entities.Inventory;
using BIDADYUManagement.Domain.Entities.Software;
using BIDADYUManagement.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Software
    public DbSet<SoftwareCategory> SoftwareCategories => Set<SoftwareCategory>();
    public DbSet<Software> Softwares => Set<Software>();
    public DbSet<SoftwareVersion> SoftwareVersions => Set<SoftwareVersion>();
    public DbSet<SoftwarePackage> SoftwarePackages => Set<SoftwarePackage>();

    // Computers & Agents
    public DbSet<Computer> Computers => Set<Computer>();
    public DbSet<ComputerGroup> ComputerGroups => Set<ComputerGroup>();
    public DbSet<ComputerGroupMember> ComputerGroupMembers => Set<ComputerGroupMember>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentHeartbeat> AgentHeartbeats => Set<AgentHeartbeat>();

    // Inventory
    public DbSet<InstalledSoftware> InstalledSoftwares => Set<InstalledSoftware>();

    // Deployments
    public DbSet<DeploymentJob> DeploymentJobs => Set<DeploymentJob>();
    public DbSet<DeploymentJobTarget> DeploymentJobTargets => Set<DeploymentJobTarget>();
    public DbSet<DeploymentLog> DeploymentLogs => Set<DeploymentLog>();

    // Compliance
    public DbSet<SoftwarePolicy> SoftwarePolicies => Set<SoftwarePolicy>();
    public DbSet<SoftwarePolicyItem> SoftwarePolicyItems => Set<SoftwarePolicyItem>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from current assembly (will pick up all IEntityTypeConfiguration<T>)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
