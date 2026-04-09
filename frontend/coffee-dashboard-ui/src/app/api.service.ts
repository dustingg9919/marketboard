import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { ApiAccount, DashboardSummaryResponse, LoginResponse } from './api.types';

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

  async getApiAccounts(): Promise<ApiAccount[]> {
    const response = await fetch(`${this.baseUrl}/api-accounts`);

    if (!response.ok) {
      throw new Error('API account load failed');
    }

    return response.json() as Promise<ApiAccount[]>;
  }

  async addApiAccount(name: string, status: string): Promise<ApiAccount> {
    const response = await fetch(`${this.baseUrl}/api-accounts`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, status })
    });

    if (!response.ok) {
      throw new Error('API account create failed');
    }

    return response.json() as Promise<ApiAccount>;
  }

  async setCurrentApiAccount(name: string): Promise<ApiAccount[]> {
    const response = await fetch(`${this.baseUrl}/api-accounts/current`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    });

    if (!response.ok) {
      throw new Error('API account update failed');
    }

    return response.json() as Promise<ApiAccount[]>;
  }

  async updateApiAccount(name: string, status: string): Promise<ApiAccount> {
    const response = await fetch(`${this.baseUrl}/api-accounts`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, status })
    });

    if (!response.ok) {
      throw new Error('API account update failed');
    }

    return response.json() as Promise<ApiAccount>;
  }

  async deleteApiAccount(name: string): Promise<ApiAccount[]> {
    const response = await fetch(`${this.baseUrl}/api-accounts`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name })
    });

    if (!response.ok) {
      throw new Error('API account delete failed');
    }

    return response.json() as Promise<ApiAccount[]>;
  }

  async aiHookLogin(username: string, password: string) {
    const response = await fetch(`${this.baseUrl}/ai-hook/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    return response.json();
  }

  async aiHookSaveKey(username: string, apiKey: string | null) {
    const response = await fetch(`${this.baseUrl}/ai-hook/save-key`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, apiKey })
    });

    if (!response.ok) {
      throw new Error('Save key failed');
    }

    return response.json();
  }

  async aiHookConsume(username: string) {
    const response = await fetch(`${this.baseUrl}/ai-hook/consume`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username })
    });

    if (!response.ok) {
      throw new Error('Consume failed');
    }

    return response.json();
  }

  async aiHookDescribeVideo(username: string, prompt: string | null, videoFile: File) {
    const formData = new FormData();
    formData.append('username', username);
    if (prompt) {
      formData.append('prompt', prompt);
    }
    formData.append('video', videoFile);

    const response = await fetch(`${this.baseUrl}/ai-hook/describe-video`, {
      method: 'POST',
      body: formData
    });

    if (!response.ok) {
      throw new Error('Describe video failed');
    }

    return response.json();
  }

}
