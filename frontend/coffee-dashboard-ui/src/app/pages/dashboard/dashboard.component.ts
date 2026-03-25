import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ApiService } from '../../api.service';
import { ApiAccount, DashboardSummaryResponse, MarketCard, NewsArticle } from '../../api.types';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  lastUpdated = '';
  cards: MarketCard[] = [];
  news: NewsArticle[] = [];
  loading = true;
  error = '';

  readonly quickStats: MarketCard[] = [
    { code: 'OIL', label: 'Giá dầu', value: null, unit: '', changePercent: null },
    { code: 'GOLD', label: 'Giá vàng', value: null, unit: '', changePercent: null },
    { code: 'SILVER', label: 'Giá bạc', value: null, unit: '', changePercent: null },
    { code: 'VNINDEX', label: 'VN-Index', value: null, unit: '', changePercent: null }
  ];

  readonly menuItems = [
    { label: 'Dashboard', id: 'section-dashboard' },
    { label: 'Markets', id: 'section-markets' },
    { label: 'Crypto', id: 'section-crypto' },
    { label: 'News', id: 'section-news' },
    { label: 'Reports', id: 'section-reports' },
    { label: 'API', id: 'section-api' }
  ];
  activeMenu = 'Dashboard';
  private sectionObserver?: IntersectionObserver;

  apiAccounts: ApiAccount[] = [];
  apiForm = {
    name: '',
    status: ''
  };

  editing: ApiAccount | null = null;

  constructor(
    private readonly router: Router,
    private readonly apiService: ApiService,
    private readonly title: Title
  ) {}

  ngOnInit(): void {
    this.setTitle(this.activeMenu);
    void this.loadDashboard();
    void this.loadApiAccounts();
    this.initSectionObserver();
  }

  setTitle(menu: string): void {
    this.activeMenu = menu;
    this.title.setTitle(`${menu} · marketboard`);
  }

  scrollToSection(item: { label: string; id: string }): void {
    const el = document.getElementById(item.id);
    if (!el) return;
    el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    this.setTitle(item.label);
  }

  private initSectionObserver(): void {
    if (this.sectionObserver) return;
    const options: IntersectionObserverInit = {
      root: null,
      rootMargin: '-30% 0px -55% 0px',
      threshold: 0
    };

    this.sectionObserver = new IntersectionObserver(entries => {
      const visible = entries
        .filter(entry => entry.isIntersecting)
        .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

      if (!visible) return;
      const match = this.menuItems.find(item => item.id === visible.target.id);
      if (match) this.setTitle(match.label);
    }, options);

    setTimeout(() => {
      this.menuItems.forEach(item => {
        const el = document.getElementById(item.id);
        if (el) this.sectionObserver?.observe(el);
      });
    }, 0);
  }

  async loadDashboard(): Promise<void> {
    this.loading = true;
    this.error = '';

    try {
      const response: DashboardSummaryResponse = await this.apiService.getDashboardSummary();
      this.lastUpdated = new Date(response.lastUpdatedAt).toLocaleString('vi-VN');
      this.cards = response.markets;
      this.news = response.latestNews;
    } catch {
      this.error = 'Không tải được dashboard từ backend.';
    } finally {
      this.loading = false;
    }
  }

  get heroCards(): MarketCard[] {
    const mergedCards = [...this.cards, ...this.quickStats];
    const priority = ['GOLD', 'SILVER', 'OIL', 'USDVND', 'BTCUSDT'];

    return priority
      .map(code => mergedCards.find(card => card.code === code) ?? this.createPendingCard(code))
      .slice(0, 5);
  }

  get commodityCards(): MarketCard[] {
    const excluded = new Set(['GOLD', 'SILVER', 'OIL', 'USDVND', 'BTCUSDT']);
    return [...this.cards.filter(card => !excluded.has(card.code)), ...this.quickStats.filter(card => !excluded.has(card.code))];
  }

  get cryptoCards(): MarketCard[] {
    const cryptoCodes = ['BTCUSDT', 'ETHUSDT', 'BNBUSDT', 'SOLUSDT', 'XRPUSDT', 'ADAUSDT', 'DOGEUSDT'];
    const fromData = this.cards.filter(card => cryptoCodes.includes(card.code));
    const fallback = this.cards.filter(card => !this.quickStats.some(s => s.code === card.code));
    const source = fromData.length ? fromData : fallback;
    return source.slice(0, 5);
  }

  private createPendingCard(code: string): MarketCard {
    const map: Record<string, string> = {
      COFFEE_DOMESTIC: 'Cà phê nội địa',
      USDVND: 'USD/VND',
      LONDON_ROBUSTA: 'London Robusta',
      BTCUSDT: 'Bitcoin',
      GOLD: 'Giá vàng',
      SILVER: 'Giá bạc',
      OIL: 'Giá dầu'
    };

    return {
      code,
      label: map[code] ?? code,
      value: null,
      unit: '',
      changePercent: null
    };
  }

  isPending(card: MarketCard): boolean {
    return card.value === null || card.value === undefined || !card.unit;
  }

  formatValue(value: number | null, card?: MarketCard): string {
    if (value === null || value === undefined || (card && this.isPending(card))) {
      return 'Đang cập nhật...';
    }

    return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 }).format(value);
  }

  formatChange(card: MarketCard): string {
    if (this.isPending(card) || card.changePercent === null || card.changePercent === undefined) {
      return 'Đang cập nhật...';
    }

    const sign = card.changePercent > 0 ? '+' : '';
    return `${sign}${card.changePercent}%`;
  }

  getChangeClass(card: MarketCard): string {
    if (this.isPending(card) || card.changePercent === null || card.changePercent === undefined) {
      return 'neutral';
    }

    return card.changePercent >= 0 ? 'positive' : 'negative';
  }

  formatUnit(card: MarketCard): string {
    return this.isPending(card) ? 'Đang cập nhật...' : card.unit;
  }

  hasSecondaryValue(card: MarketCard): boolean {
    return !!card.secondaryUnit && card.secondaryValue !== null && card.secondaryValue !== undefined;
  }

  formatSecondaryValue(card: MarketCard): string {
    if (!this.hasSecondaryValue(card)) {
      return 'Đang cập nhật...';
    }

    return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 }).format(card.secondaryValue as number);
  }

  liveStatus(label: string): string {
    const liveItems = ['Crypto feed', 'VNExpress RSS'];
    return liveItems.includes(label) ? 'Live' : 'Đang cập nhật...';
  }

  logout(): void {
    localStorage.removeItem('coffee-dashboard-auth');
    void this.router.navigateByUrl('/login');
  }

  async loadApiAccounts(): Promise<void> {
    try {
      this.apiAccounts = await this.apiService.getApiAccounts();
    } catch {
      // keep silent for demo
    }
  }

  async addApiAccount(): Promise<void> {
    if (!this.apiForm.name.trim()) return;

    try {
      await this.apiService.addApiAccount(this.apiForm.name.trim(), this.apiForm.status.trim());
      this.apiForm = { name: '', status: '' };
      await this.loadApiAccounts();
    } catch {
      // ignore for demo
    }
  }

  startEdit(account: ApiAccount): void {
    this.editing = { ...account };
  }

  cancelEdit(): void {
    this.editing = null;
  }

  async saveEdit(): Promise<void> {
    if (!this.editing) return;

    try {
      await this.apiService.updateApiAccount(this.editing.name, this.editing.status);
      this.editing = null;
      await this.loadApiAccounts();
    } catch {
      // ignore for demo
    }
  }

  async deleteAccount(account: ApiAccount): Promise<void> {
    try {
      this.apiAccounts = await this.apiService.deleteApiAccount(account.name);
    } catch {
      // ignore for demo
    }
  }

  async setCurrentApiAccount(target: ApiAccount): Promise<void> {
    try {
      this.apiAccounts = await this.apiService.setCurrentApiAccount(target.name);
    } catch {
      // ignore for demo
    }
  }
}
