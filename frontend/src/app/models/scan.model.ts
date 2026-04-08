// src/app/models/scan.model.ts

export interface ScanResult {
  id: number;
  barcode1: string;
  barcode2: string;
  result: 'OK' | 'NOK';
  deviceId?: number;
  deviceName?: string;
  userId?: number;
  username?: string;
  scannedAt: string;
}

export interface Stats {
  total: number;
  totalOK: number;
  totalNOK: number;
  errorRate: number;
}

export interface HourlyScan {
  hour: number;
  total: number;
  totalOK: number;
  totalNOK: number;
}

export interface Device {
  id: number;
  name: string;
  location?: string;
  ipAddress?: string;
  isOnline: boolean;
  lastSeenAt?: string;
}

export interface ScanBroadcast {
  scan: ScanResult;
  updatedStats: Stats;
}

export interface HistoryFilter {
  searchCode?: string;
  result?: 'OK' | 'NOK' | '';
  deviceId?: number;
  dateFrom?: string;
  dateTo?: string;
  page: number;
  pageSize: number;
}
