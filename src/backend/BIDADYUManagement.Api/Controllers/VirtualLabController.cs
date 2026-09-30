using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VirtualLabController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VirtualLabController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("generate-lab")]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin)]
    public async Task<IActionResult> GenerateVirtualLab([FromBody] GenerateLabRequest request)
    {
        if (request.ComputerCount <= 0 || request.ComputerCount > 100)
            return BadRequest(new { Message = "Bir defada 1 ile 100 arasında sanal bilgisayar oluşturabilirsiniz." });

        var group = new ComputerGroup
        {
            Id = Guid.NewGuid(),
            Name = request.LabName,
            Description = request.Description ?? $"{request.LabName} Sanal Laboratuvarı",
            Building = request.Building,
            Floor = request.Floor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _context.ComputerGroups.AddAsync(group);

        var random = new Random();
        var generatedComputers = new List<Computer>();

        for (int i = 1; i <= request.ComputerCount; i++)
        {
            string pcName = $"{request.Prefix}-{i:D2}";
            string macAddress = $"52:54:00:{random.Next(10, 99)}:{random.Next(10, 99)}:{random.Next(10, 99)}";
            string ipAddress = $"192.168.10.{random.Next(10, 250)}";

            var computer = new Computer
            {
                Id = Guid.NewGuid(),
                Hostname = pcName,
                MacAddress = macAddress,
                IpAddress = ipAddress,
                OsVersion = "Windows 11 Pro 64-bit (Sanal İstemci)",
                Status = ComputerStatus.Online,
                CreatedAt = DateTime.UtcNow
            };
            generatedComputers.Add(computer);
            await _context.Computers.AddAsync(computer);

            // Laboratuvara üye yap
            await _context.ComputerGroupMembers.AddAsync(new ComputerGroupMember
            {
                ComputerId = computer.Id,
                GroupId = group.Id,
                AddedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = $"{request.LabName} başarıyla oluşturuldu ve {request.ComputerCount} sanal bilgisayar eklendi.",
            GroupId = group.Id,
            Computers = generatedComputers.Select(c => new { c.Id, c.Hostname, c.IpAddress, c.Status })
        });
    }
}

public class GenerateLabRequest
{
    public string LabName { get; set; } = string.Empty;
    public string Prefix { get; set; } = "PC-LAB";
    public int ComputerCount { get; set; } = 15;
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Description { get; set; }
}
