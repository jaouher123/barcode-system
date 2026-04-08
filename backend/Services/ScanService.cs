using BarcodeSystem.Api.Data;
using BarcodeSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BarcodeSystem.Api.Services;

public interface IScanService
{
    Task<ScanResultDto> CompareAsync(CompareScanRequest request);
    Task<List<ScanResultDto>> GetHistoryAsync(HistoryFilterRequest filter);
    Task<StatsDto> GetTodayStatsAsync();
    Task<List<HourlyScanDto>> GetHourlyScansAsync();
    Task<byte[]> ExportExcelAsync(HistoryFilterRequest filter);
}

public class ScanService(BarcodeDbContext db) : IScanService
{
    public async Task<ScanResultDto> CompareAsync(CompareScanRequest req)
    {
        var result = req.Barcode1 == req.Barcode2 ? "OK" : "NOK";

        var scan = new Scan
        {
            Barcode1  = req.Barcode1,
            Barcode2  = req.Barcode2,
            Result    = result,
            DeviceId  = req.DeviceId,
            UserId    = req.UserId,
            ScannedAt = DateTime.UtcNow
        };

        db.Scans.Add(scan);
        await db.SaveChangesAsync();

        var device = req.DeviceId.HasValue
            ? await db.Devices.FindAsync(req.DeviceId.Value)
            : null;
        var user = req.UserId.HasValue
            ? await db.Users.FindAsync(req.UserId.Value)
            : null;

        return ToDto(scan, device, user);
    }

    public async Task<List<ScanResultDto>> GetHistoryAsync(HistoryFilterRequest f)
    {
        var query = db.Scans
            .Include(s => s.Device)
            .Include(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.SearchCode))
            query = query.Where(s =>
                s.Barcode1.Contains(f.SearchCode) || s.Barcode2.Contains(f.SearchCode));

        if (!string.IsNullOrWhiteSpace(f.Result))
            query = query.Where(s => s.Result == f.Result);

        if (f.DeviceId.HasValue)
            query = query.Where(s => s.DeviceId == f.DeviceId);

        if (f.DateFrom.HasValue)
            query = query.Where(s => s.ScannedAt >= f.DateFrom.Value);

        if (f.DateTo.HasValue)
            query = query.Where(s => s.ScannedAt <= f.DateTo.Value);

        return await query
            .OrderByDescending(s => s.ScannedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(s => ToDto(s, s.Device, s.User))
            .ToListAsync();
    }

    public async Task<StatsDto> GetTodayStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var scans = await db.Scans
            .Where(s => s.ScannedAt >= today)
            .ToListAsync();

        int total = scans.Count;
        int ok    = scans.Count(s => s.Result == "OK");
        int nok   = scans.Count(s => s.Result == "NOK");

        return new StatsDto(total, ok, nok,
            total > 0 ? Math.Round((double)nok / total * 100, 2) : 0);
    }

    public async Task<List<HourlyScanDto>> GetHourlyScansAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await db.Scans
            .Where(s => s.ScannedAt >= today)
            .GroupBy(s => s.ScannedAt.Hour)
            .Select(g => new HourlyScanDto(
                g.Key,
                g.Count(),
                g.Count(s => s.Result == "OK"),
                g.Count(s => s.Result == "NOK")))
            .OrderBy(h => h.Hour)
            .ToListAsync();
    }

    public async Task<byte[]> ExportExcelAsync(HistoryFilterRequest filter)
    {
        var scans = await GetHistoryAsync(filter with { PageSize = 10000, Page = 1 });

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Historique");

        // Header row
        var headers = new[] { "ID", "Barcode 1", "Barcode 2", "Résultat", "Appareil", "Opérateur", "Date/Heure" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Data rows
        for (int r = 0; r < scans.Count; r++)
        {
            var s = scans[r];
            ws.Cell(r + 2, 1).Value = s.Id;
            ws.Cell(r + 2, 2).Value = s.Barcode1;
            ws.Cell(r + 2, 3).Value = s.Barcode2;
            ws.Cell(r + 2, 4).Value = s.Result;
            ws.Cell(r + 2, 5).Value = s.DeviceName ?? "";
            ws.Cell(r + 2, 6).Value = s.Username ?? "";
            ws.Cell(r + 2, 7).Value = s.ScannedAt.ToString("yyyy-MM-dd HH:mm:ss");

            // Color rows
            var color = s.Result == "OK"
                ? ClosedXML.Excel.XLColor.LightGreen
                : ClosedXML.Excel.XLColor.LightSalmon;
            ws.Row(r + 2).Style.Fill.BackgroundColor = color;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static ScanResultDto ToDto(Scan s, Device? d, User? u) => new(
        s.Id, s.Barcode1, s.Barcode2, s.Result,
        s.DeviceId, d?.Name, s.UserId, u?.Username, s.ScannedAt);
}
