// src/app/services/api.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ScanBroadcast, Stats, HourlyScan, Device, HistoryFilter, ScanResult
} from '../models/scan.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = 'http://localhost:5000/api';
  private http = inject(HttpClient);

  // ── Scans ──────────────────────────────────────────────────────────────────

  compare(barcode1: string, barcode2: string, deviceId?: number, userId?: number): Observable<ScanBroadcast> {
    return this.http.post<ScanBroadcast>(`${this.base}/scans/compare`, {
      barcode1, barcode2, deviceId, userId
    });
  }

  getHistory(filter: HistoryFilter): Observable<ScanResult[]> {
    let params = new HttpParams()
      .set('page', filter.page)
      .set('pageSize', filter.pageSize);

    if (filter.searchCode) params = params.set('searchCode', filter.searchCode);
    if (filter.result)     params = params.set('result', filter.result);
    if (filter.deviceId)   params = params.set('deviceId', filter.deviceId);
    if (filter.dateFrom)   params = params.set('dateFrom', filter.dateFrom);
    if (filter.dateTo)     params = params.set('dateTo', filter.dateTo);

    return this.http.get<ScanResult[]>(`${this.base}/scans/history`, { params });
  }

  getStats(): Observable<Stats> {
    return this.http.get<Stats>(`${this.base}/scans/stats`);
  }

  getHourly(): Observable<HourlyScan[]> {
    return this.http.get<HourlyScan[]>(`${this.base}/scans/hourly`);
  }

  exportExcel(filter: Partial<HistoryFilter> = {}): void {
    let params = new HttpParams();
    if (filter.searchCode) params = params.set('searchCode', filter.searchCode);
    if (filter.result)     params = params.set('result', filter.result);
    if (filter.dateFrom)   params = params.set('dateFrom', filter.dateFrom!);
    if (filter.dateTo)     params = params.set('dateTo', filter.dateTo!);

    this.http.get(`${this.base}/scans/export`, {
      params, responseType: 'blob'
    }).subscribe(blob => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `barcodes_${new Date().toISOString().slice(0,10)}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    });
  }

  // ── Devices ────────────────────────────────────────────────────────────────

  getDevices(): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.base}/devices`);
  }
}
