using BarcodeSystem.Api.Data;
using BarcodeSystem.Api.Hubs;
using BarcodeSystem.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BarcodeSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(
    BarcodeDbContext db,
    IHubContext<BarcodeHub> hubContext) : ControllerBase
{
    // GET api/devices
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var devices = await db.Devices
            .Select(d => new DeviceDto(d.Id, d.Name, d.Location, d.IpAddress, d.IsOnline, d.LastSeenAt))
            .ToListAsync();
        return Ok(devices);
    }

    // PUT api/devices/{id}/heartbeat
    // Called by each WiFi scanner to signal it is online
    [HttpPut("{id}/heartbeat")]
    public async Task<IActionResult> Heartbeat(int id)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return NotFound();

        device.IsOnline   = true;
        device.LastSeenAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var broadcast = new DeviceStatusBroadcast(id, true, device.LastSeenAt);
        await hubContext.Clients.All.SendAsync("DeviceStatus", broadcast);

        return Ok();
    }

    // PUT api/devices/{id}/offline
    [HttpPut("{id}/offline")]
    public async Task<IActionResult> SetOffline(int id)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return NotFound();

        device.IsOnline = false;
        await db.SaveChangesAsync();

        var broadcast = new DeviceStatusBroadcast(id, false, device.LastSeenAt);
        await hubContext.Clients.All.SendAsync("DeviceStatus", broadcast);

        return Ok();
    }
}
