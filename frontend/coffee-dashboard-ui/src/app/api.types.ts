export interface LoginResponse {
  accessToken: string;
  username: string;
  role: string;
}

export interface MarketCard {
  code: string;
  label: string;
  value: number | null;
  unit: string;
  secondaryValue?: number | null;
  secondaryUnit?: string | null;
  change?: number | null;
  changePercent?: number | null;
  updatedAt?: string;
}

export interface NewsArticle {
  id?: string;
  category: string;
  title: string;
  summary: string;
  url: string;
  publishedAt: string;
}

export interface DashboardSummaryResponse {
  lastUpdatedAt: string;
  markets: MarketCard[];
  latestNews: NewsArticle[];
}

export interface ApiAccount {
  name: string;
  status: string;
  current: boolean;
}

export interface AiHookAccount {
  username: string;
  apiKey?: string | null;
  paymentType: string;
  expirationDate: string;
  expirationTimes: number;
  bankAccount?: string | null;
  bankName?: string | null;
}
