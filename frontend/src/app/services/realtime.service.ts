// src/app/services/realtime.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ScanBroadcast, Stats } from '../models/scan.model';

@Injectable({ providedIn: 'root' })
export class RealtimeService implements OnDestroy {
  private hub!: signalR.HubConnection;

  readonly scanResult$  = new Subject<ScanBroadcast>();
  readonly deviceStatus$ = new Subject<{ deviceId: number; isOnline: boolean; lastSeenAt?: string }>();
  readonly statsUpdate$  = new Subject<Stats>();

  connected = false;

  constructor() {
    this.connect();
  }

  private connect(): void {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/hubs/barcode')
      .withAutomaticReconnect()
      .build();

    // ── Register server-push handlers ────────────────────────────────────────
    this.hub.on('ScanResult',   (payload: ScanBroadcast) => this.scanResult$.next(payload));
    this.hub.on('DeviceStatus', (payload: any)           => this.deviceStatus$.next(payload));
    this.hub.on('StatsUpdate',  (payload: Stats)         => this.statsUpdate$.next(payload));

    this.hub.onreconnected(() => (this.connected = true));
    this.hub.onreconnecting(() => (this.connected = false));
    this.hub.onclose(() => (this.connected = false));

    this.hub.start()
      .then(() => (this.connected = true))
      .catch(err => console.error('SignalR connection error:', err));
  }

  /** Join a group (e.g. "operators", "supervisors") */
  joinGroup(groupName: string): void {
    if (this.connected) this.hub.invoke('JoinGroup', groupName);
  }

  ngOnDestroy(): void {
    this.hub.stop();
  }
}
