using CoffeeDashboard.Domain.Entities;
using CoffeeDashboard.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/api-accounts")]
public class ApiAccountsController(DashboardDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var accounts = await dbContext.ApiAccounts
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(accounts.Select(a => new ApiAccountDto(a.Name, a.Status, a.IsCurrent)));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ApiAccountCreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var name = request.Name.Trim();
        var status = request.Status?.Trim() ?? "Unknown";

        var existing = await dbContext.ApiAccounts
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);

        if (existing != null)
        {
            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var isCurrent = !await dbContext.ApiAccounts.AnyAsync(cancellationToken);
            dbContext.ApiAccounts.Add(new ApiAccountRecord
            {
                Name = name,
                Status = status,
                IsCurrent = isCurrent
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var saved = await dbContext.ApiAccounts
            .FirstAsync(x => x.Name == name, cancellationToken);
        return Ok(new ApiAccountDto(saved.Name, saved.Status, saved.IsCurrent));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ApiAccountUpdateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var existing = await dbContext.ApiAccounts
            .FirstOrDefaultAsync(x => x.Name == request.Name.Trim(), cancellationToken);

        if (existing == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        existing.Status = request.Status?.Trim() ?? existing.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new ApiAccountDto(existing.Name, existing.Status, existing.IsCurrent));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] ApiAccountDeleteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var existing = await dbContext.ApiAccounts
            .FirstOrDefaultAsync(x => x.Name == request.Name.Trim(), cancellationToken);

        if (existing == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        var wasCurrent = existing.IsCurrent;
        dbContext.ApiAccounts.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasCurrent)
        {
            var fallback = await dbContext.ApiAccounts
                .OrderBy(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
            if (fallback != null)
            {
                fallback.IsCurrent = true;
                fallback.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var accounts = await dbContext.ApiAccounts
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(accounts.Select(a => new ApiAccountDto(a.Name, a.Status, a.IsCurrent)));
    }

    [HttpPut("current")]
    public async Task<IActionResult> SetCurrent([FromBody] ApiAccountCurrentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var name = request.Name.Trim();
        var accounts = await dbContext.ApiAccounts.ToListAsync(cancellationToken);

        if (accounts.Count == 0)
        {
            return NotFound(new { message = "Account not found" });
        }

        if (!accounts.Any(a => a.Name == name))
        {
            return NotFound(new { message = "Account not found" });
        }

        foreach (var account in accounts)
        {
            account.IsCurrent = account.Name == name;
            account.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(accounts.Select(a => new ApiAccountDto(a.Name, a.Status, a.IsCurrent)));
    }
}

public record ApiAccountCreateRequest(string Name, string? Status);
public record ApiAccountUpdateRequest(string Name, string? Status);
public record ApiAccountDeleteRequest(string Name);
public record ApiAccountCurrentRequest(string Name);

public record ApiAccountDto(string Name, string Status, bool Current);
