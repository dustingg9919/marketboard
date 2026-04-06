import { Routes } from '@angular/router';
import { AiHookComponent } from './pages/ai-hook/ai-hook.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { LoginComponent } from './pages/login/login.component';
import { ResumeComponent } from './pages/resume/resume.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'resume', component: ResumeComponent },
  { path: 'dashboard-legacy', component: DashboardComponent },
  { path: 'ai-hook', component: AiHookComponent },
  // NOTE: Mặc định vào resume (HTML tĩnh), không gọi backend.
  { path: '', pathMatch: 'full', redirectTo: 'resume' },
  { path: '**', redirectTo: 'resume' }
];
