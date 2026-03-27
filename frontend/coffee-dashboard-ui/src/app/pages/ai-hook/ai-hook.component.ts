import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

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
}
