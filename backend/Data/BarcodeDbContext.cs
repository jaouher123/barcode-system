using Microsoft.EntityFrameworkCore;

namespace BarcodeSystem.Api.Data;

// ─── Entity Models ────────────────────────────────────────────────────────────

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Operator"; // Operator | Supervisor | Administrator
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Scan> Scans { get; set; } = [];
}

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? IpAddress { get; set; }
    public bool IsOnline { get; set; } = false;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Scan> Scans { get; set; } = [];
}

public class Scan
{
    public long Id { get; set; }
    public string Barcode1 { get; set; } = string.Empty;
    public string Barcode2 { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty; // OK | NOK
    public int? DeviceId { get; set; }
    public Device? Device { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}

// ─── DbContext ────────────────────────────────────────────────────────────────

public class BarcodeDbContext(DbContextOptions<BarcodeDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Scan> Scans => Set<Scan>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Scan>(e =>
        {
            e.HasIndex(s => s.ScannedAt);
            e.HasIndex(s => s.Result);
            e.Property(s => s.Result).HasMaxLength(10);
        });

        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Role).HasMaxLength(50);
        });
    }
}
