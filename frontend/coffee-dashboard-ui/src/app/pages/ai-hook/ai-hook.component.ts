import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../api.service';
import { AiHookAccount } from '../../api.types';

@Component({
  selector: 'app-ai-hook',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './ai-hook.component.html',
  styleUrl: './ai-hook.component.scss'
})
export class AiHookComponent {
  showApiGuide = false;
  showPaywall = true;
  showLogin = false;
  showExpired = false;
  expiredMessage = 'Tài khoản của bạn đã hết hạn';

  loginForm = {
    username: '',
    password: ''
  };

  aiForm = {
    apiKey: '',
    idea: '',
    ratio: 'Dọc 9:16 (TikTok / Reels)',
    voice: 'Tiếng Việt',
    gender: 'Nam (Male)'
  };

  account: AiHookAccount | null = null;

  readonly actionButtons = [
    { label: 'TẠO HOOK', variant: 'primary' },
    { label: 'KỊCH BẢN 60S', variant: 'secondary' },
    { label: 'TỐI ƯU SEO', variant: 'info' },
    { label: 'KỊCH BẢN LIVE', variant: 'danger' },
    { label: 'PROMPT ẢNH & VIDEO', variant: 'purple' },
    { label: 'VẼ ẢNH HOOK', variant: 'warning' }
  ];

  readonly ratioOptions = ['Dọc 9:16 (TikTok / Reels)', 'Ngang 16:9 (YouTube)', 'Vuông 1:1'];
  readonly voiceOptions = ['Tiếng Việt', 'English', '日本語'];
  readonly genderOptions = ['Nam (Male)', 'Nữ (Female)', 'Trung tính'];

  constructor(private readonly apiService: ApiService) {}

  openApiGuide(): void {
    this.showApiGuide = true;
  }

  closeApiGuide(): void {
    this.showApiGuide = false;
    window.open('https://aistudio.google.com/api-keys', '_blank');
  }

  showLoginModal(): void {
    this.showLogin = true;
    this.showPaywall = false;
  }

  async login(): Promise<void> {
    try {
      const account = await this.apiService.aiHookLogin(this.loginForm.username, this.loginForm.password);
      this.account = account as AiHookAccount;
      this.aiForm.apiKey = account.apiKey ?? '';
      this.showPaywall = false;
      this.showLogin = false;
    } catch {
      alert('Sai tài khoản hoặc mật khẩu');
    }
  }

  async saveKey(): Promise<void> {
    if (!this.account) return;
    try {
      const account = await this.apiService.aiHookSaveKey(this.account.username, this.aiForm.apiKey || null);
      this.account = account as AiHookAccount;
    } catch {
      // ignore for demo
    }
  }

  async handleAction(): Promise<void> {
    if (!this.account) {
      this.showPaywall = true;
      return;
    }

    try {
      const result = await this.apiService.aiHookConsume(this.account.username);
      if (result.expired) {
        this.expiredMessage = result.message ?? 'Tài khoản của bạn đã hết hạn';
        this.showExpired = true;
        return;
      }

      await this.saveKey();
      alert('Đã gửi prompt demo tới Gemini (mock).');
    } catch {
      // ignore for demo
    }
  }

  closeExpired(): void {
    this.showExpired = false;
  }
}
