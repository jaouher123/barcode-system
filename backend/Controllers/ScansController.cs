using BarcodeSystem.Api.Hubs;
using BarcodeSystem.Api.Models;
using BarcodeSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BarcodeSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScansController(
    IScanService scanService,
    IHubContext<BarcodeHub> hubContext) : ControllerBase
{
    // POST api/scans/compare
    [HttpPost("compare")]
    public async Task<IActionResult> Compare([FromBody] CompareScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Barcode1) || string.IsNullOrWhiteSpace(request.Barcode2))
            return BadRequest("Les deux codes-barres sont requis.");

        var scan  = await scanService.CompareAsync(request);
        var stats = await scanService.GetTodayStatsAsync();

        // Broadcast to all connected clients in real time
        await hubContext.Clients.All.SendAsync("ScanResult", new ScanBroadcast(scan, stats));

        return Ok(new ScanBroadcast(scan, stats));
    }

    // GET api/scans/history
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] HistoryFilterRequest filter)
    {
        var history = await scanService.GetHistoryAsync(filter);
        return Ok(history);
    }

    // GET api/scans/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await scanService.GetTodayStatsAsync();
        return Ok(stats);
    }

    // GET api/scans/hourly
    [HttpGet("hourly")]
    public async Task<IActionResult> Hourly()
    {
        var hourly = await scanService.GetHourlyScansAsync();
        return Ok(hourly);
    }

    // GET api/scans/export
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] HistoryFilterRequest filter)
    {
        var bytes = await scanService.ExportExcelAsync(filter);
        var fileName = $"barcodes_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
