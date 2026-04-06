import { Routes } from '@angular/router';
import { AiHookComponent } from './pages/ai-hook/ai-hook.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: HomeComponent },
  { path: 'dashboard-legacy', component: DashboardComponent },
  { path: 'ai-hook', component: AiHookComponent },
  // NOTE: Giữ redirect về login để auto-login hoạt động và dễ rollback.
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
