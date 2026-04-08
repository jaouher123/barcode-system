// src/app/components/history/history.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ScanResult, HistoryFilter } from '../../models/scan.model';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss'
})
export class HistoryComponent implements OnInit {
  private api = inject(ApiService);

  scans: ScanResult[] = [];
  loading = false;

  filter: HistoryFilter = {
    searchCode: '',
    result: '',
    page: 1,
    pageSize: 50
  };

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.getHistory(this.filter).subscribe({
      next: data => { this.scans = data; this.loading = false; },
      error: ()   => { this.loading = false; }
    });
  }

  onSearch(): void {
    this.filter.page = 1;
    this.load();
  }

  nextPage(): void {
    this.filter.page++;
    this.load();
  }

  prevPage(): void {
    if (this.filter.page > 1) { this.filter.page--; this.load(); }
  }

  exportExcel(): void {
    this.api.exportExcel(this.filter);
  }
}
