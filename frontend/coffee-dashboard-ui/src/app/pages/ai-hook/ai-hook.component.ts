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
  resultHtml = '';
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

  videoForm = {
    prompt: ''
  };

  videoFile: File | null = null;

  account: AiHookAccount | null = null;

  readonly actionButtons = [
    { label: 'TẠO HOOK', variant: 'primary' },
    { label: 'KỊCH BẢN 60S', variant: 'secondary' },
    { label: 'TỐI ƯU SEO', variant: 'info' },
    { label: 'KỊCH BẢN LIVE', variant: 'danger' },
    { label: 'PROMPT ẢNH & VIDEO', variant: 'purple' },
    { label: 'VẼ ẢNH HOOK', variant: 'warning' },
    { label: 'QUAY VIDEO MÔ TẢ SẢN PHẨM', variant: 'info' }
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
    this.isLoading = true;
    try {
      const account = await this.apiService.aiHookLogin(this.loginForm.username, this.loginForm.password);
      this.account = account as AiHookAccount;
      this.aiForm.apiKey = account.apiKey ?? '';
      this.showPaywall = false;
      this.showLogin = false;
    } catch {
      alert('Sai tài khoản hoặc mật khẩu');
    } finally {
      this.isLoading = false;
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
      this.resultHtml = this.wrapLine('Vui lòng nhập Gemini API Key trước.');
      return;
    }

    this.isLoading = true;
    try {
      const result = await this.apiService.aiHookConsume(this.account.username);
      if (result.expired) {
        this.expiredMessage = result.message ?? 'Tài khoản của bạn đã hết hạn';
        this.showExpired = true;
        return;
      }

      await this.saveKey();
      this.resultHtml = '';

      const prompt = this.buildPrompt(actionLabel);
      const response = await fetch(
        `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=${encodeURIComponent(
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
        this.resultHtml = this.wrapLine('Gọi Gemini thất bại. Kiểm tra API Key hoặc quota.');
        return;
      }

      const data = await response.json();
      const text = data?.candidates?.[0]?.content?.parts?.map((p: any) => p.text).join('') ?? '';
      this.resultHtml = this.formatResultHtml(text || 'Không có nội dung trả về.');
    } catch {
      this.resultHtml = this.wrapLine('Có lỗi khi gọi Gemini.');
    } finally {
      this.isLoading = false;
    }
  }

  onVideoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.videoFile = null;
      return;
    }
    this.videoFile = input.files[0];
  }

  async handleVideoDescribe(): Promise<void> {
    if (!this.account) {
      this.showPaywall = true;
      return;
    }

    if (!this.aiForm.apiKey.trim()) {
      this.resultHtml = this.wrapLine('Vui lòng nhập Gemini API Key trước.');
      return;
    }

    if (!this.videoFile) {
      this.resultHtml = this.wrapLine('Vui lòng chọn video sản phẩm (10-30s).');
      return;
    }

    this.isLoading = true;
    try {
      await this.saveKey();
      const result = await this.apiService.aiHookDescribeVideo(
        this.account.username,
        this.videoForm.prompt.trim() || null,
        this.videoFile
      );

      if (result.expired) {
        this.expiredMessage = result.message ?? 'Tài khoản của bạn đã hết hạn';
        this.showExpired = true;
        return;
      }

      this.resultHtml = this.formatResultHtml(result.text || 'Không có nội dung trả về.');
    } catch {
      this.resultHtml = this.wrapLine('Có lỗi khi gọi Gemini qua API hook.');
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

  formatResultHtml(raw: string): string {
    const sanitized = raw.replace(/<br\s*\/?\s*>/gi, '\n');
    const lines = sanitized.split('\n');
    const headings = ['Mở bài', 'Nội dung', 'Kết thúc', 'Hook', 'CTA', 'Ý tưởng', 'Gợi ý', 'Tiêu đề'];

    return lines
      .map(line => {
        const trimmed = line.trim();
        if (!trimmed) return '<div class="line spacer"></div>';

        const headingMatch = trimmed.match(/^#{2,3}\s*(.+)$/);
        if (headingMatch) {
          return `<h2>${this.escapeHtml(headingMatch[1])}</h2>`;
        }

        const matched = headings.find(h => trimmed.toLowerCase().startsWith(h.toLowerCase()));
        if (matched) {
          const rest = trimmed.slice(matched.length).trim().replace(/^[:\-–]+\s*/, '');
          return `<h2>${this.escapeHtml(matched)}</h2>${this.wrapLine(rest)}`;
        }

        return this.wrapLine(trimmed);
      })
      .join('');
  }

  wrapLine(text: string): string {
    if (!text) return '';
    return `<div class="line">${this.formatInline(text)}</div>`;
  }

  formatInline(text: string): string {
    let escaped = this.escapeHtml(text);
    escaped = escaped.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    return escaped;
  }

  escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
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
