import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../api.service';
import { ApiAccount, DashboardSummaryResponse, MarketCard, NewsArticle } from '../../api.types';

@Component({
  selector: 'app-ai-hook',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './ai-hook.component.html',
  styleUrl: './ai-hook.component.scss'
})
export class AiHookComponent {
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
}
