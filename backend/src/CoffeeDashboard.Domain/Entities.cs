namespace CoffeeDashboard.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class MarketCard
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? SecondaryValue { get; set; }
    public string? SecondaryUnit { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class NewsArticle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}

public class MarketAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MarketSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MarketAssetId { get; set; }
    public MarketAsset? MarketAsset { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? SecondaryValue { get; set; }
    public string? SecondaryUnit { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApiAccountRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class AiHookAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int ExpirationTimes { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class AiHookPaymentPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int UsageLimit { get; set; }
    public string? ApiLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class ResumeInfo
{
    public string ObjectKey { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
