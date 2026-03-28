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
  showPaywall = false;
  showLogin = false;
  showExpired = false;
  showQr = false;
  expiredMessage = 'Tài khoản của bạn đã hết hạn';
  qrTitle = '';
  qrImage = '';
  resultText = '';
  isLoading = false;

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

  closePaywall(): void {
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

  async handleAction(actionLabel = ''): Promise<void> {
    if (!this.account) {
      this.showPaywall = true;
      return;
    }

    if (!this.aiForm.apiKey.trim()) {
      this.resultText = 'Vui lòng nhập Gemini API Key trước.';
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
      this.isLoading = true;
      this.resultText = '';

      const prompt = this.buildPrompt(actionLabel);
      const response = await fetch(
        `https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=${encodeURIComponent(
          this.aiForm.apiKey.trim()
        )}`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            contents: [
              {
                role: 'user',
                parts: [{ text: prompt }]
              }
            ]
          })
        }
      );

      if (!response.ok) {
        this.resultText = 'Gọi Gemini thất bại. Kiểm tra API Key hoặc quota.';
        return;
      }

      const data = await response.json();
      const text = data?.candidates?.[0]?.content?.parts?.map((p: any) => p.text).join('') ?? '';
      this.resultText = text || 'Không có nội dung trả về.';
    } catch {
      this.resultText = 'Có lỗi khi gọi Gemini.';
    } finally {
      this.isLoading = false;
    }
  }

  buildPrompt(actionLabel: string): string {
    return `Bạn là trợ lý viết kịch bản/marketing.

Yêu cầu: ${actionLabel || 'Tạo hook'}
Tiêu đề chính: ${this.aiForm.idea}
Tỷ lệ: ${this.aiForm.ratio}
Ngôn ngữ voice: ${this.aiForm.voice}
Giới tính voice: ${this.aiForm.gender}

Hãy trả về nội dung rõ ràng, có cấu trúc, ngắn gọn và dễ triển khai.`;
  }

  closeExpired(): void {
    this.showExpired = false;
  }

  openQr(plan: 'trial' | 'expand' | 'pro'): void {
    this.showQr = true;
    if (plan === 'trial') {
      this.qrTitle = 'Thanh toán · Dùng thử';
      this.qrImage = '/assets/qr/trial.jpg';
    } else if (plan === 'expand') {
      this.qrTitle = 'Thanh toán · Mở rộng';
      this.qrImage = '/assets/qr/expand.jpg';
    } else {
      this.qrTitle = 'Thanh toán · Chuyên Nghiệp';
      this.qrImage = '/assets/qr/pro.jpg';
    }
  }

  closeQr(): void {
    this.showQr = false;
  }
}
