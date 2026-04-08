// src/app/app.component.ts
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="app-shell">
      <nav class="app-nav">
        <span class="brand">&#9642; BarcodeSystem</span>
        <a routerLink="/operateur"  routerLinkActive="active">Interface opérateur</a>
        <a routerLink="/dashboard"  routerLinkActive="active">Dashboard</a>
        <a routerLink="/historique" routerLinkActive="active">Historique</a>
      </nav>
      <main class="app-content">
        <router-outlet />
      </main>
    </div>
  `
})
export class AppComponent {}
