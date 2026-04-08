using BarcodeSystem.Api.Data;
using BarcodeSystem.Api.Hubs;
using BarcodeSystem.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddDbContext<BarcodeDbContext>(opt =>
    opt.UseSqlite("Data Source=barcode.db"));

builder.Services.AddScoped<IScanService, ScanService>();

builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Barcode System API", Version = "v1" });
});

// CORS — allow Angular dev server
builder.Services.AddCors(opt =>
    opt.AddPolicy("Angular", policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()   // required for SignalR
    )
);

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Angular");
app.UseAuthorization();
app.MapControllers();

// SignalR hub endpoint
app.MapHub<BarcodeHub>("/hubs/barcode");

// Créer automatiquement la base SQLite au démarrage (aucune migration nécessaire)
using var scope = app.Services.CreateScope();
var dbCtx = scope.ServiceProvider.GetRequiredService<BarcodeDbContext>();
await dbCtx.Database.EnsureCreatedAsync();

// Insérer les appareils par défaut si la table est vide
if (!dbCtx.Devices.Any())
{
    dbCtx.Devices.AddRange(
        new BarcodeSystem.Api.Data.Device { Name = "Scanner A", Location = "Hall 1 - Ligne 1", IpAddress = "192.168.1.101", IsOnline = true },
        new BarcodeSystem.Api.Data.Device { Name = "Scanner B", Location = "Hall 1 - Ligne 2", IpAddress = "192.168.1.102", IsOnline = true },
        new BarcodeSystem.Api.Data.Device { Name = "Scanner C", Location = "Hall 2 - Ligne 1", IpAddress = "192.168.1.103", IsOnline = false },
        new BarcodeSystem.Api.Data.Device { Name = "Scanner D", Location = "Hall 2 - Ligne 2", IpAddress = "192.168.1.104", IsOnline = true }
    );
    await dbCtx.SaveChangesAsync();
}

app.Run();
