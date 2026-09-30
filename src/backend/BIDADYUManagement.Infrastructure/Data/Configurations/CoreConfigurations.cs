using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BIDADYUManagement.Infrastructure.Data.Configurations;

public class ComputerGroupMemberConfiguration : IEntityTypeConfiguration<ComputerGroupMember>
{
    public void Configure(EntityTypeBuilder<ComputerGroupMember> builder)
    {
        builder.HasKey(cgm => new { cgm.ComputerId, cgm.GroupId });

        builder.HasOne(cgm => cgm.Computer)
            .WithMany(c => c.GroupMemberships)
            .HasForeignKey(cgm => cgm.ComputerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cgm => cgm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(cgm => cgm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComputerConfiguration : IEntityTypeConfiguration<Computer>
{
    public void Configure(EntityTypeBuilder<Computer> builder)
    {
        builder.HasIndex(c => c.Hostname);
        builder.HasIndex(c => c.MacAddress);
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasIndex(a => a.DeviceId).IsUnique();
        
        builder.HasOne(a => a.Computer)
            .WithOne(c => c.Agent)
            .HasForeignKey<Agent>(a => a.ComputerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SoftwareConfiguration : IEntityTypeConfiguration<Software>
{
    public void Configure(EntityTypeBuilder<Software> builder)
    {
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.HasIndex(s => s.Name);
    }
}

public class SoftwarePackageConfiguration : IEntityTypeConfiguration<SoftwarePackage>
{
    public void Configure(EntityTypeBuilder<SoftwarePackage> builder)
    {
        builder.HasOne(sp => sp.SoftwareVersion)
            .WithMany(sv => sv.Packages)
            .HasForeignKey(sp => sp.SoftwareVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
