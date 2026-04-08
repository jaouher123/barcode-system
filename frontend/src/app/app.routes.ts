// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { OperatorComponent }  from './components/operator/operator.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { HistoryComponent }   from './components/history/history.component';

export const routes: Routes = [
  { path: '',          redirectTo: 'operateur', pathMatch: 'full' },
  { path: 'operateur', component: OperatorComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'historique', component: HistoryComponent },
];
