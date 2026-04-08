namespace BarcodeSystem.Api.Models;

// ─── Requests ─────────────────────────────────────────────────────────────────

public record CompareScanRequest(
    string Barcode1,
    string Barcode2,
    int? DeviceId,
    int? UserId
);

public record HistoryFilterRequest(
    string? SearchCode,
    string? Result,       // OK | NOK | null = all
    int? DeviceId,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 50
);

// ─── Responses ────────────────────────────────────────────────────────────────

public record ScanResultDto(
    long Id,
    string Barcode1,
    string Barcode2,
    string Result,
    int? DeviceId,
    string? DeviceName,
    int? UserId,
    string? Username,
    DateTime ScannedAt
);

public record StatsDto(
    int Total,
    int TotalOK,
    int TotalNOK,
    double ErrorRate
);

public record HourlyScanDto(
    int Hour,
    int Total,
    int TotalOK,
    int TotalNOK
);

public record DeviceDto(
    int Id,
    string Name,
    string? Location,
    string? IpAddress,
    bool IsOnline,
    DateTime? LastSeenAt
);

// ─── SignalR broadcast payloads ───────────────────────────────────────────────

public record ScanBroadcast(ScanResultDto Scan, StatsDto UpdatedStats);
public record DeviceStatusBroadcast(int DeviceId, bool IsOnline, DateTime? LastSeenAt);
