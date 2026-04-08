// src/app/components/operator/operator.component.ts
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { RealtimeService } from '../../services/realtime.service';
import { ScanResult, Stats, ScanBroadcast } from '../../models/scan.model';

type ScanState = 'waiting-first' | 'waiting-second' | 'result';

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operator.component.html',
  styleUrl: './operator.component.scss'
})
export class OperatorComponent implements OnInit, OnDestroy {
  private api      = inject(ApiService);
  private realtime = inject(RealtimeService);
  private sub!: Subscription;

  // ── State ──────────────────────────────────────────────────────────────────
  state: ScanState = 'waiting-first';
  barcode1 = '';
  barcode2 = '';
  lastScan?: ScanResult;
  stats: Stats = { total: 0, totalOK: 0, totalNOK: 0, errorRate: 0 };
  recentHistory: ScanResult[] = [];
  loading = false;

  ngOnInit(): void {
    this.api.getStats().subscribe(s => (this.stats = s));
    this.api.getHistory({ page: 1, pageSize: 10 })
      .subscribe(h => (this.recentHistory = h));

    // Listen for real-time scan results (from other scanners on same network)
    this.sub = this.realtime.scanResult$.subscribe(broadcast => {
      this.stats = broadcast.updatedStats;
      this.recentHistory = [broadcast.scan, ...this.recentHistory].slice(0, 10);
    });
  }

  onBarcode1Enter(): void {
    if (!this.barcode1.trim()) return;
    this.state = 'waiting-second';
  }

  onBarcode2Enter(): void {
    if (!this.barcode2.trim()) return;
    this.compare();
  }

  compare(): void {
    this.loading = true;
    this.api.compare(this.barcode1.trim(), this.barcode2.trim())
      .subscribe({
        next: (res: ScanBroadcast) => {
          this.lastScan = res.scan;
          this.stats    = res.updatedStats;
          this.state    = 'result';
          this.loading  = false;
          this.recentHistory = [res.scan, ...this.recentHistory].slice(0, 10);
          // Auto-reset after 3 seconds
          setTimeout(() => this.reset(), 3000);
        },
        error: () => (this.loading = false)
      });
  }

  reset(): void {
    this.barcode1 = '';
    this.barcode2 = '';
    this.lastScan = undefined;
    this.state    = 'waiting-first';
  }

  exportExcel(): void {
    this.api.exportExcel();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
