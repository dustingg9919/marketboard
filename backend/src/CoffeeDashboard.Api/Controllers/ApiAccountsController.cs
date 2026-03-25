using Microsoft.AspNetCore.Mvc;

namespace CoffeeDashboard.Api.Controllers;

[ApiController]
[Route("api/api-accounts")]
public class ApiAccountsController(ApiAccountStore store) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(store.GetAll());

    [HttpPost]
    public IActionResult Add([FromBody] ApiAccountCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var account = store.AddOrUpdate(request.Name.Trim(), request.Status?.Trim() ?? "Unknown");
        return Ok(account);
    }

    [HttpPut]
    public IActionResult Update([FromBody] ApiAccountUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var updated = store.Update(request.Name.Trim(), request.Status?.Trim() ?? "Unknown");
        if (updated == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        return Ok(updated);
    }

    [HttpDelete]
    public IActionResult Delete([FromBody] ApiAccountDeleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var removed = store.Delete(request.Name.Trim());
        if (!removed)
        {
            return NotFound(new { message = "Account not found" });
        }

        return Ok(store.GetAll());
    }

    [HttpPut("current")]
    public IActionResult SetCurrent([FromBody] ApiAccountCurrentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        var updated = store.SetCurrent(request.Name.Trim());
        if (!updated)
        {
            return NotFound(new { message = "Account not found" });
        }

        return Ok(store.GetAll());
    }
}

public record ApiAccountCreateRequest(string Name, string? Status);
public record ApiAccountUpdateRequest(string Name, string? Status);
public record ApiAccountDeleteRequest(string Name);
public record ApiAccountCurrentRequest(string Name);

public record ApiAccountDto(string Name, string Status, bool Current);

public class ApiAccountStore
{
    private readonly object _lock = new();
    private readonly List<ApiAccountDto> _accounts = new();

    public IReadOnlyList<ApiAccountDto> GetAll()
    {
        lock (_lock)
        {
            return _accounts.Select(a => new ApiAccountDto(a.Name, a.Status, a.Current)).ToList();
        }
    }

    public ApiAccountDto AddOrUpdate(string name, string status)
    {
        lock (_lock)
        {
            var existing = _accounts.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _accounts.Remove(existing);
                var updated = new ApiAccountDto(existing.Name, status, existing.Current);
                _accounts.Add(updated);
                return updated;
            }

            var account = new ApiAccountDto(name, status, _accounts.Count == 0);
            _accounts.Add(account);
            return account;
        }
    }

    public ApiAccountDto? Update(string name, string status)
    {
        lock (_lock)
        {
            var existing = _accounts.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing == null) return null;

            _accounts.Remove(existing);
            var updated = new ApiAccountDto(existing.Name, status, existing.Current);
            _accounts.Add(updated);
            return updated;
        }
    }

    public bool Delete(string name)
    {
        lock (_lock)
        {
            var existing = _accounts.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing == null) return false;

            var wasCurrent = existing.Current;
            _accounts.Remove(existing);

            if (wasCurrent && _accounts.Count > 0)
            {
                var first = _accounts[0];
                _accounts[0] = first with { Current = true };
            }

            return true;
        }
    }

    public bool SetCurrent(string name)
    {
        lock (_lock)
        {
            var found = false;
            for (var i = 0; i < _accounts.Count; i++)
            {
                var account = _accounts[i];
                var isCurrent = account.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                if (isCurrent) found = true;
                _accounts[i] = account with { Current = isCurrent };
            }
            return found;
        }
    }
}
