// src/app/components/dashboard/dashboard.component.ts
import { Component, OnInit, OnDestroy, inject, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import { ApiService } from '../../services/api.service';
import { RealtimeService } from '../../services/realtime.service';
import { Stats, HourlyScan, Device, ScanResult } from '../../models/scan.model';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  private api      = inject(ApiService);
  private realtime = inject(RealtimeService);
  private subs: Subscription[] = [];

  @ViewChild('chartHour')  chartHourRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('chartError') chartErrorRef!: ElementRef<HTMLCanvasElement>;

  stats: Stats = { total: 0, totalOK: 0, totalNOK: 0, errorRate: 0 };
  devices: Device[] = [];
  activityFeed: ScanResult[] = [];

  private chartHour?: Chart;
  private chartError?: Chart;
  private hourlyData: HourlyScan[] = [];

  ngOnInit(): void {
    this.api.getStats().subscribe(s => (this.stats = s));
    this.api.getDevices().subscribe(d => (this.devices = d));
    this.api.getHourly().subscribe(h => {
      this.hourlyData = h;
      this.updateCharts();
    });

    // Real-time updates
    this.subs.push(
      this.realtime.scanResult$.subscribe(broadcast => {
        this.stats = broadcast.updatedStats;
        this.activityFeed = [broadcast.scan, ...this.activityFeed].slice(0, 8);
        this.patchHourlyData(broadcast.scan);
        this.updateCharts();
      }),
      this.realtime.deviceStatus$.subscribe(update => {
        const d = this.devices.find(x => x.id === update.deviceId);
        if (d) { d.isOnline = update.isOnline; d.lastSeenAt = update.lastSeenAt; }
      })
    );
  }

  ngAfterViewInit(): void {
    this.initCharts();
  }

  private initCharts(): void {
    const baseOpts: any = {
      responsive: true, maintainAspectRatio: false,
      plugins: { legend: { display: false } }
    };

    this.chartHour = new Chart(this.chartHourRef.nativeElement, {
      type: 'bar',
      data: {
        labels: this.buildLabels(),
        datasets: [{ data: this.buildHourValues(), backgroundColor: '#85B7EB', borderRadius: 4 }]
      },
      options: { ...baseOpts, scales: { y: { beginAtZero: true } } }
    });

    this.chartError = new Chart(this.chartErrorRef.nativeElement, {
      type: 'line',
      data: {
        labels: this.buildLabels(),
        datasets: [{
          data: this.buildErrorRates(), borderColor: '#E24B4A',
          backgroundColor: 'rgba(226,75,74,0.1)', tension: 0.4, fill: true
        }]
      },
      options: { ...baseOpts, scales: { y: { beginAtZero: true, max: 100 } } }
    });
  }

  private buildLabels(): string[] {
    return this.hourlyData.map(h => `${String(h.hour).padStart(2, '0')}h`);
  }

  private buildHourValues(): number[] {
    return this.hourlyData.map(h => h.total);
  }

  private buildErrorRates(): number[] {
    return this.hourlyData.map(h =>
      h.total > 0 ? Math.round((h.totalNOK / h.total) * 100) : 0);
  }

  private updateCharts(): void {
    if (!this.chartHour || !this.chartError) return;
    this.chartHour.data.labels  = this.buildLabels();
    this.chartHour.data.datasets[0].data = this.buildHourValues();
    this.chartError.data.labels = this.buildLabels();
    this.chartError.data.datasets[0].data = this.buildErrorRates();
    this.chartHour.update();
    this.chartError.update();
  }

  private patchHourlyData(scan: ScanResult): void {
    const hour = new Date(scan.scannedAt).getHours();
    let entry = this.hourlyData.find(h => h.hour === hour);
    if (!entry) {
      entry = { hour, total: 0, totalOK: 0, totalNOK: 0 };
      this.hourlyData.push(entry);
      this.hourlyData.sort((a, b) => a.hour - b.hour);
    }
    entry.total++;
    if (scan.result === 'OK') entry.totalOK++; else entry.totalNOK++;
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.chartHour?.destroy();
    this.chartError?.destroy();
  }
}
