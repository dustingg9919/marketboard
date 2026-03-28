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
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './ai-hook.component.html',
  styleUrl: './ai-hook.component.scss'
})
export class AiHookComponent {
  readonly hookButtons = [
    'Generate Prompt',
    'Summarize News',
    'Market Signal',
    'Price Alert',
    'Risk Snapshot',
    'Weekly Brief'
  ];

  readonly quickActions = [
    'Create Hook',
    'Test Payload',
    'Copy JSON',
    'Save Template'
  ];

  readonly jsonPreview = `{
  "hook": "coffee-market",
  "priority": "high",
  "channels": ["dashboard", "telegram"],
  "template": "Daily brief",
  "schedule": "09:00 Asia/Bangkok"
}`;
}
