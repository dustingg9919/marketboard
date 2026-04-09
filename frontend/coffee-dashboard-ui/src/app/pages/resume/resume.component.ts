import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../api.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
}

@Component({
  selector: 'app-resume',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './resume.component.html',
  styleUrl: './resume.component.scss'
})
export class ResumeComponent {
  isChatOpen = true;
  isSending = false;
  chatInput = '';
  messages: ChatMessage[] = [
    {
      role: 'assistant',
      text: 'Chào bạn! Mình là trợ lý của anh Nguyên 👋 Bạn muốn tìm hiểu kỹ năng, dự án hay kinh nghiệm nào?'
    }
  ];

  constructor(private readonly apiService: ApiService) {}

  toggleChat(): void {
    this.isChatOpen = !this.isChatOpen;
  }

  async sendMessage(): Promise<void> {
    const content = this.chatInput.trim();
    if (!content || this.isSending) return;

    this.messages.push({ role: 'user', text: content });
    this.chatInput = '';
    this.isSending = true;

    try {
      const history = this.messages.map(m => ({ role: m.role, text: m.text }));
      const response = await this.apiService.resumeChat(content, history);
      const text = response?.text || 'Xin lỗi, hiện tại trợ lý chưa trả lời được.';
      this.messages.push({ role: 'assistant', text });
    } catch {
      this.messages.push({ role: 'assistant', text: 'Có lỗi khi kết nối trợ lý. Vui lòng thử lại.' });
    } finally {
      this.isSending = false;
    }
  }
}
