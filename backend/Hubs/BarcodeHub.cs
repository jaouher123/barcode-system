using Microsoft.AspNetCore.SignalR;

namespace BarcodeSystem.Api.Hubs;

/// <summary>
/// SignalR hub — clients join groups by role or deviceId.
/// Server broadcasts scan results and device status in real time.
/// </summary>
public class BarcodeHub : Hub
{
    // ── Client-callable methods ──────────────────────────────────────────────

    /// <summary>Join a named group (e.g. "operators", "supervisors", "device-3")</summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    // ── Server → Client event names (keep in sync with Angular service) ──────
    //
    // "ScanResult"      — broadcast after every comparison
    //     payload: ScanBroadcast { Scan, UpdatedStats }
    //
    // "DeviceStatus"    — broadcast when a scanner connects/disconnects
    //     payload: DeviceStatusBroadcast { DeviceId, IsOnline, LastSeenAt }
    //
    // "StatsUpdate"     — periodic stats refresh (every 30 s from a background service)
    //     payload: StatsDto
}

/// <summary>
/// Strongly-typed client interface for BarcodeHub.
/// Use IHubContext<BarcodeHub, IBarcodeClient> in controllers/services.
/// </summary>
public interface IBarcodeClient
{
    Task ScanResult(object payload);
    Task DeviceStatus(object payload);
    Task StatsUpdate(object payload);
}
