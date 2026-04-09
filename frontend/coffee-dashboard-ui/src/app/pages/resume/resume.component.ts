import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';

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
  private readonly apiKey = 'AIzaSyC45ufgYsXLFZG-pTvena8BVPemQXwOLj0';
  private readonly model = 'gemini-2.5-flash';

  isChatOpen = false;
  isSending = false;
  chatInput = '';
  messages: ChatMessage[] = [
    {
      role: 'assistant',
      text: 'Xin chào! Tôi là trợ lý của Phạm Thái Nguyên. Bạn muốn hỏi gì về kỹ năng hoặc dự án?'
    }
  ];

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
      const prompt =
        'Bạn là trợ lý của Phạm Thái Nguyên. Trả lời ngắn gọn, lịch sự, và tập trung vào kinh nghiệm, kỹ năng, và dự án trong CV. Nếu câu hỏi ngoài phạm vi, hãy trả lời ngắn gọn và đề nghị liên hệ qua email.';

      const contents = [
        { role: 'user', parts: [{ text: prompt }] },
        ...this.messages.map(m => ({ role: m.role, parts: [{ text: m.text }] })),
        { role: 'user', parts: [{ text: content }] }
      ];

      const response = await fetch(
        `https://generativelanguage.googleapis.com/v1beta/models/${this.model}:generateContent?key=${encodeURIComponent(
          this.apiKey
        )}`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ contents })
        }
      );

      if (!response.ok) {
        this.messages.push({ role: 'assistant', text: 'Có lỗi khi kết nối trợ lý. Vui lòng thử lại.' });
        return;
      }

      const data = await response.json();
      const text = data?.candidates?.[0]?.content?.parts?.map((p: any) => p.text).join('') ?? '';
      this.messages.push({ role: 'assistant', text: text || 'Xin lỗi, hiện tại trợ lý chưa trả lời được.' });
    } catch {
      this.messages.push({ role: 'assistant', text: 'Có lỗi khi kết nối trợ lý. Vui lòng thử lại.' });
    } finally {
      this.isSending = false;
    }
  }
}
