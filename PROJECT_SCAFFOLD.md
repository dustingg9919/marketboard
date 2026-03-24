# Coffee Dashboard MVP Scaffold

## 1) Monorepo structure

```text
coffee-dashboard/
├─ .gitignore
├─ README.md
├─ docker-compose.yml                  # optional local SQL Server/dev infra
├─ docs/
│  ├─ architecture.md
│  ├─ api-contract.md
│  └─ database.md
├─ backend/
│  ├─ CoffeeDashboard.sln
│  └─ src/
│     ├─ CoffeeDashboard.Api/
│     │  ├─ Controllers/
│     │  │  ├─ AuthController.cs
│     │  │  ├─ DashboardController.cs
│     │  │  ├─ MarketsController.cs
│     │  │  ├─ NewsController.cs
│     │  │  └─ AdminController.cs
│     │  ├─ Middleware/
│     │  ├─ Extensions/
│     │  ├─ Program.cs
│     │  └─ appsettings.json
│     ├─ CoffeeDashboard.Application/
│     │  ├─ DTOs/
│     │  │  ├─ Auth/
│     │  │  ├─ Dashboard/
│     │  │  ├─ Markets/
│     │  │  └─ News/
│     │  ├─ Interfaces/
│     │  ├─ Services/
│     │  ├─ Validators/
│     │  └─ Mappings/
│     ├─ CoffeeDashboard.Domain/
│     │  ├─ Entities/
│     │  ├─ Enums/
│     │  ├─ ValueObjects/
│     │  └─ Common/
│     ├─ CoffeeDashboard.Infrastructure/
│     │  ├─ Persistence/
│     │  │  ├─ AppDbContext.cs
│     │  │  ├─ Configurations/
│     │  │  └─ Migrations/
│     │  ├─ Repositories/
│     │  ├─ Integrations/
│     │  │  ├─ Coffee/
│     │  │  ├─ Fx/
│     │  │  ├─ Crypto/
│     │  │  └─ News/
│     │  └─ Logging/
│     └─ CoffeeDashboard.Worker/
│        ├─ Jobs/
│        │  ├─ CoffeeDomesticJob.cs
│        │  ├─ UsdVndRateJob.cs
│        │  ├─ LondonCoffeeJob.cs
│        │  ├─ CryptoMarketJob.cs
│        │  ├─ VnExpressNewsJob.cs
│        │  └─ DailySummaryJob.cs
│        ├─ Scheduling/
│        └─ Program.cs
├─ frontend/
│  └─ coffee-dashboard-ui/
│     ├─ angular.json
│     ├─ package.json
│     └─ src/
│        ├─ app/
│        │  ├─ core/
│        │  │  ├─ auth/
│        │  │  ├─ guards/
│        │  │  ├─ interceptors/
│        │  │  └─ services/
│        │  ├─ shared/
│        │  │  ├─ components/
│        │  │  ├─ models/
│        │  │  └─ utils/
│        │  ├─ features/
│        │  │  ├─ login/
│        │  │  ├─ dashboard/
│        │  │  ├─ markets/
│        │  │  ├─ news/
│        │  │  └─ admin/
│        │  ├─ app-routing.module.ts
│        │  ├─ app.component.ts
│        │  └─ app.module.ts
│        └─ environments/
└─ scripts/
   ├─ init-repo.ps1
   ├─ init-backend.ps1
   └─ init-frontend.ps1
```

---

## 2) Backend project responsibilities

### CoffeeDashboard.Api
- Expose REST endpoints
- JWT auth setup
- Swagger
- Exception handling / middleware
- Thin controllers only

### CoffeeDashboard.Application
- Use-case services
- DTOs
- Validation rules
- Mapping between entities and response models
- Service interfaces used by API and Worker

### CoffeeDashboard.Domain
- Core entities
- Enums
- Domain rules
- No infrastructure dependencies

### CoffeeDashboard.Infrastructure
- EF Core + SQL Server
- DbContext and entity configurations
- Repository implementations
- External integrations (CoinGecko, VNExpress RSS, coffee source, FX source)

### CoffeeDashboard.Worker
- Scheduled jobs
- Retry/logging around data ingestion
- Daily summary generation

---

## 3) Domain entities to create first

### Users
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

### DataSource
```csharp
public class DataSource
{
    public Guid Id { get; set; }
    public string SourceCode { get; set; } = default!;
    public string SourceName { get; set; } = default!;
    public string SourceType { get; set; } = default!;
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RefreshIntervalMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### MarketInstrument
```csharp
public class MarketInstrument
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string? Currency { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
```

### MarketSnapshot
```csharp
public class MarketSnapshot
{
    public long Id { get; set; }
    public Guid SourceId { get; set; }
    public Guid InstrumentId { get; set; }
    public string? Region { get; set; }
    public decimal Value { get; set; }
    public decimal? OpenValue { get; set; }
    public decimal? HighValue { get; set; }
    public decimal? LowValue { get; set; }
    public decimal? PreviousCloseValue { get; set; }
    public decimal? ChangeValue { get; set; }
    public decimal? ChangePercent { get; set; }
    public DateTime CapturedAt { get; set; }
    public string? RawPayload { get; set; }
    public DateTime CreatedAt { get; set; }

    public DataSource Source { get; set; } = default!;
    public MarketInstrument Instrument { get; set; } = default!;
}
```

### CoffeeDomesticPrice
```csharp
public class CoffeeDomesticPrice
{
    public long Id { get; set; }
    public Guid SourceId { get; set; }
    public string Province { get; set; } = default!;
    public decimal PriceValue { get; set; }
    public string Currency { get; set; } = "VND";
    public string Unit { get; set; } = "kg";
    public DateTime CapturedAt { get; set; }
    public string? RawText { get; set; }
    public string? RawPayload { get; set; }
    public DateTime CreatedAt { get; set; }

    public DataSource Source { get; set; } = default!;
}
```

### NewsArticle
```csharp
public class NewsArticle
{
    public Guid Id { get; set; }
    public Guid SourceId { get; set; }
    public string? Category { get; set; }
    public string Title { get; set; } = default!;
    public string? Summary { get; set; }
    public string Url { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CrawledAt { get; set; }
    public string ContentHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public DataSource Source { get; set; } = default!;
}
```

### JobRun
```csharp
public class JobRun
{
    public long Id { get; set; }
    public string JobName { get; set; } = default!;
    public Guid? SourceId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = default!;
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### DailySummary
```csharp
public class DailySummary
{
    public long Id { get; set; }
    public DateOnly SummaryDate { get; set; }
    public decimal? CoffeeDomesticAverage { get; set; }
    public decimal? CoffeeDomesticChange { get; set; }
    public decimal? UsdVndValue { get; set; }
    public decimal? UsdVndChange { get; set; }
    public decimal? LondonCoffeeValue { get; set; }
    public decimal? LondonCoffeeChange { get; set; }
    public decimal? BtcValue { get; set; }
    public decimal? BtcChangePercent { get; set; }
    public string? TopAltcoinsJson { get; set; }
    public int NewsCount { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

---

## 4) DTOs to scaffold first

### DashboardSummaryResponse
```csharp
public class DashboardSummaryResponse
{
    public DateTime LastUpdatedAt { get; set; }
    public CoffeeSummaryDto Coffee { get; set; } = new();
    public FxSummaryDto Fx { get; set; } = new();
    public CommoditySummaryDto LondonCoffee { get; set; } = new();
    public CryptoSummaryDto Crypto { get; set; } = new();
    public List<NewsItemDto> LatestNews { get; set; } = new();
}
```

### LoginRequest
```csharp
public class LoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
```

### LoginResponse
```csharp
public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Role { get; set; } = default!;
}
```

---

## 5) Service interfaces to create first

```csharp
public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}

public interface IMarketService
{
    Task<object> GetLatestOverviewAsync(CancellationToken cancellationToken);
    Task<object> GetHistoryAsync(string symbol, int days, CancellationToken cancellationToken);
}

public interface INewsService
{
    Task<IReadOnlyList<NewsItemDto>> GetLatestAsync(int take, CancellationToken cancellationToken);
}
```

---

## 6) Controllers to create first

### AuthController
Routes:
- `POST /api/auth/login`
- `GET /api/auth/me`

### DashboardController
Routes:
- `GET /api/dashboard/summary`
- `GET /api/dashboard/last-updated`

### MarketsController
Routes:
- `GET /api/markets/overview`
- `GET /api/markets/history?symbol=BTCUSDT&days=7`
- `GET /api/markets/coffee-domestic`
- `GET /api/markets/crypto`

### NewsController
Routes:
- `GET /api/news`
- `GET /api/news/latest`

### AdminController
Routes:
- `GET /api/admin/jobs`
- `POST /api/admin/jobs/{jobName}/run`

---

## 7) Worker jobs - first pass

Implement jobs in this order:

1. `CryptoMarketJob`
   - easiest source
   - validates worker pipeline

2. `VnExpressNewsJob`
   - prefer RSS first
   - hash dedupe articles

3. `UsdVndRateJob`
   - simple external source

4. `CoffeeDomesticJob`
   - most fragile parser

5. `LondonCoffeeJob`
   - depends on source selection

6. `DailySummaryJob`
   - aggregate all collected data

---

## 8) Angular module scaffold

### core/
- auth service
- auth guard
- jwt interceptor
- api base service

### shared/
- reusable cards
- loading spinner
- error state component
- interfaces/models

### features/login/
Files:
- login.component.ts
- login.component.html
- login.component.scss
- login-routing.module.ts

### features/dashboard/
Files:
- dashboard.component.ts
- dashboard.component.html
- dashboard.service.ts
- components/
  - summary-cards/
  - coffee-widget/
  - crypto-widget/
  - fx-widget/
  - news-widget/

### features/markets/
Files:
- markets.component.ts
- history-chart.component.ts

### features/news/
Files:
- news-list.component.ts
- news-detail.component.ts

### features/admin/
Files:
- jobs-monitor.component.ts
- system-log.component.ts

---

## 9) Frontend route plan

```ts
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', canActivate: [AuthGuard], component: DashboardComponent },
  { path: 'markets', canActivate: [AuthGuard], component: MarketsComponent },
  { path: 'news', canActivate: [AuthGuard], component: NewsListComponent },
  { path: 'admin', canActivate: [AuthGuard], component: JobsMonitorComponent },
  { path: '**', redirectTo: '' }
];
```

---

## 10) Git/GitHub initialization plan

### Initial branches
- `main`
- `develop`

### First feature branches
- `feature/backend-solution-init`
- `feature/database-schema`
- `feature/frontend-shell`
- `feature/auth-login`
- `feature/crypto-job`
- `feature/news-job`

### First commits
1. `chore: initialize monorepo structure`
2. `feat: add backend solution and core projects`
3. `feat: add angular app shell`
4. `feat: add initial sql server entities and db context`
5. `feat: add login api and angular login page`

---

## 11) MVP build order

### Sprint 1
- init git repo + GitHub
- create monorepo
- create backend solution
- create Angular app shell
- create DB schema + first migration

### Sprint 2
- login API + login page
- dashboard shell + static widgets
- crypto job
- news RSS job

### Sprint 3
- USD/VND job
- coffee domestic job
- market history API
- charts frontend

### Sprint 4
- london coffee job
- daily summary job
- admin jobs monitor
- polish + error handling

---

## 12) Practical recommendation

If time is limited, do **not** start with coffee scraping first.
Start with:
- auth
- dashboard shell
- crypto source
- VNExpress RSS

That gives a visible working product quickly, then add fragile sources later.
