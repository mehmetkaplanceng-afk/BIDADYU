using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIDADYUManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PackageStorageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: true),
                    EntityId = table.Column<string>(type: "TEXT", nullable: true),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComputerGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Building = table.Column<string>(type: "TEXT", nullable: true),
                    Floor = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Computers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Hostname = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    MacAddress = table.Column<string>(type: "TEXT", nullable: true),
                    OperatingSystem = table.Column<string>(type: "TEXT", nullable: true),
                    OsVersion = table.Column<string>(type: "TEXT", nullable: true),
                    OsArchitecture = table.Column<string>(type: "TEXT", nullable: true),
                    CpuInfo = table.Column<string>(type: "TEXT", nullable: true),
                    RamGb = table.Column<int>(type: "INTEGER", nullable: true),
                    DiskInfo = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    Department = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Computers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: true),
                    EntityId = table.Column<string>(type: "TEXT", nullable: true),
                    ActionUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IconName = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoftwarePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnforced = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwarePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwarePolicies_ComputerGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ComputerGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComputerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", nullable: false),
                    AgentVersion = table.Column<string>(type: "TEXT", nullable: false),
                    RegistrationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiTokenHash = table.Column<string>(type: "TEXT", nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastHeartbeatAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegistrationIp = table.Column<string>(type: "TEXT", nullable: true),
                    LastKnownIp = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComputerGroupMembers",
                columns: table => new
                {
                    ComputerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AddedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerGroupMembers", x => new { x.ComputerId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_ComputerGroupMembers_ComputerGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ComputerGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputerGroupMembers_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Softwares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Platform = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    HomepageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IconUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Softwares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Softwares_SoftwareCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SoftwareCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedReason = table.Column<string>(type: "TEXT", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedByIp = table.Column<string>(type: "TEXT", nullable: true),
                    RevokedByIp = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentHeartbeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    CpuUsagePercent = table.Column<float>(type: "REAL", nullable: true),
                    RamUsagePercent = table.Column<float>(type: "REAL", nullable: true),
                    DiskUsagePercent = table.Column<float>(type: "REAL", nullable: true),
                    RamTotalMb = table.Column<long>(type: "INTEGER", nullable: true),
                    RamFreeMb = table.Column<long>(type: "INTEGER", nullable: true),
                    AgentVersion = table.Column<string>(type: "TEXT", nullable: true),
                    SystemUptimeSeconds = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentHeartbeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentHeartbeats_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SoftwareId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReleaseNotes = table.Column<string>(type: "TEXT", nullable: true),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareVersions_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstalledSoftwares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComputerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SoftwareName = table.Column<string>(type: "TEXT", nullable: false),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    InstallLocation = table.Column<string>(type: "TEXT", nullable: true),
                    UninstallString = table.Column<string>(type: "TEXT", nullable: true),
                    InstalledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsStillInstalled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MatchedSoftwareId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MatchedVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstalledSoftwares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstalledSoftwares_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstalledSoftwares_SoftwareVersions_MatchedVersionId",
                        column: x => x.MatchedVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InstalledSoftwares_Softwares_MatchedSoftwareId",
                        column: x => x.MatchedSoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SoftwarePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SoftwareVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    InstallerType = table.Column<int>(type: "INTEGER", nullable: false),
                    SilentInstallArgs = table.Column<string>(type: "TEXT", nullable: true),
                    SilentUninstallArgs = table.Column<string>(type: "TEXT", nullable: true),
                    InstallTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Sha256Hash = table.Column<string>(type: "TEXT", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Architecture = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwarePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwarePackages_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwarePolicyItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SoftwareId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequiredVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwarePolicyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwarePolicyItems_SoftwarePolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "SoftwarePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwarePolicyItems_SoftwareVersions_RequiredVersionId",
                        column: x => x.RequiredVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SoftwarePolicyItems_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeploymentJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftwareId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SoftwareVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PackageId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalTargets = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PendingCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentJobs_SoftwarePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "SoftwarePackages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeploymentJobs_SoftwareVersions_SoftwareVersionId",
                        column: x => x.SoftwareVersionId,
                        principalTable: "SoftwareVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeploymentJobs_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeploymentJobTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComputerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ExitCode = table.Column<int>(type: "INTEGER", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AgentPickedUpAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentJobTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentJobTargets_Computers_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Computers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeploymentJobTargets_DeploymentJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "DeploymentJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeploymentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobTargetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeploymentLogs_DeploymentJobTargets_JobTargetId",
                        column: x => x.JobTargetId,
                        principalTable: "DeploymentJobTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentHeartbeats_AgentId",
                table: "AgentHeartbeats",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ComputerId",
                table: "Agents",
                column: "ComputerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agents_DeviceId",
                table: "Agents",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComputerGroupMembers_GroupId",
                table: "ComputerGroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Computers_Hostname",
                table: "Computers",
                column: "Hostname");

            migrationBuilder.CreateIndex(
                name: "IX_Computers_MacAddress",
                table: "Computers",
                column: "MacAddress");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentJobs_PackageId",
                table: "DeploymentJobs",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentJobs_SoftwareId",
                table: "DeploymentJobs",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentJobs_SoftwareVersionId",
                table: "DeploymentJobs",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentJobTargets_ComputerId",
                table: "DeploymentJobTargets",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentJobTargets_JobId",
                table: "DeploymentJobTargets",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentLogs_JobTargetId",
                table: "DeploymentLogs",
                column: "JobTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledSoftwares_ComputerId",
                table: "InstalledSoftwares",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledSoftwares_MatchedSoftwareId",
                table: "InstalledSoftwares",
                column: "MatchedSoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_InstalledSoftwares_MatchedVersionId",
                table: "InstalledSoftwares",
                column: "MatchedVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePackages_SoftwareVersionId",
                table: "SoftwarePackages",
                column: "SoftwareVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePolicies_GroupId",
                table: "SoftwarePolicies",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePolicyItems_PolicyId",
                table: "SoftwarePolicyItems",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePolicyItems_RequiredVersionId",
                table: "SoftwarePolicyItems",
                column: "RequiredVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePolicyItems_SoftwareId",
                table: "SoftwarePolicyItems",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_CategoryId",
                table: "Softwares",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Softwares_Name",
                table: "Softwares",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareVersions_SoftwareId",
                table: "SoftwareVersions",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentHeartbeats");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ComputerGroupMembers");

            migrationBuilder.DropTable(
                name: "DeploymentLogs");

            migrationBuilder.DropTable(
                name: "InstalledSoftwares");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SoftwarePolicyItems");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "DeploymentJobTargets");

            migrationBuilder.DropTable(
                name: "SoftwarePolicies");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Computers");

            migrationBuilder.DropTable(
                name: "DeploymentJobs");

            migrationBuilder.DropTable(
                name: "ComputerGroups");

            migrationBuilder.DropTable(
                name: "SoftwarePackages");

            migrationBuilder.DropTable(
                name: "SoftwareVersions");

            migrationBuilder.DropTable(
                name: "Softwares");

            migrationBuilder.DropTable(
                name: "SoftwareCategories");
        }
    }
}
