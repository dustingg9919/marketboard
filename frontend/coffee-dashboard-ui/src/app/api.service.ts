import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { DashboardSummaryResponse, LoginResponse } from './api.types';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  async login(username: string, password: string): Promise<LoginResponse> {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ username, password })
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    return response.json() as Promise<LoginResponse>;
  }

  async getDashboardSummary(): Promise<DashboardSummaryResponse> {
    const response = await fetch(`${this.baseUrl}/dashboard/summary`);

    if (!response.ok) {
      throw new Error('Dashboard load failed');
    }

    return response.json() as Promise<DashboardSummaryResponse>;
  }
}
