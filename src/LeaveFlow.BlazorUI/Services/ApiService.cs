using System.Net.Http.Headers;
using System.Net.Http.Json;
using LeaveFlow.Application.DTOs;
using LeaveFlow.Domain.Entities;

namespace LeaveFlow.BlazorUI.Services;

public class ApiService(HttpClient http, AuthService auth)
{
    private async Task AuthorizeAsync()
    {
        var token = await auth.GetTokenAsync();
        http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    // ── Auth ────────────────────────────────────────────────────────────────
    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var res = await http.PostAsJsonAsync("api/auth/login", req);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<LoginResponse>() : null;
    }

    // ── Leave Requests ───────────────────────────────────────────────────────
    public async Task<List<LeaveRequestDto>> GetMyRequestsAsync()
    {
        await AuthorizeAsync();
        return await http.GetFromJsonAsync<List<LeaveRequestDto>>("api/leaverequests/my") ?? [];
    }

    public async Task<List<LeaveBalanceDto>> GetBalancesAsync(int year = 0)
    {
        await AuthorizeAsync();
        var y = year == 0 ? DateTime.UtcNow.Year : year;
        return await http.GetFromJsonAsync<List<LeaveBalanceDto>>($"api/leaverequests/balances?year={y}") ?? [];
    }

    public async Task<(bool ok, string error)> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        await AuthorizeAsync();
        var key = Guid.NewGuid().ToString();
        http.DefaultRequestHeaders.Remove("Idempotency-Key");
        http.DefaultRequestHeaders.Add("Idempotency-Key", key);
        var res = await http.PostAsJsonAsync("api/leaverequests", dto);
        http.DefaultRequestHeaders.Remove("Idempotency-Key");
        return res.IsSuccessStatusCode
            ? (true, string.Empty)
            : (false, await res.Content.ReadAsStringAsync());
    }

    public async Task<(bool ok, string error)> CancelLeaveRequestAsync(int id)
    {
        await AuthorizeAsync();
        var res = await http.PutAsync($"api/leaverequests/{id}/cancel", null);
        return res.IsSuccessStatusCode ? (true, string.Empty) : (false, await res.Content.ReadAsStringAsync());
    }

    // ── Manager ──────────────────────────────────────────────────────────────
    public async Task<List<LeaveRequestDto>> GetPendingRequestsAsync()
    {
        await AuthorizeAsync();
        return await http.GetFromJsonAsync<List<LeaveRequestDto>>("api/leaverequests/pending") ?? [];
    }

    public async Task<(bool ok, string error)> ReviewLeaveRequestAsync(int id, ReviewLeaveRequestDto dto)
    {
        await AuthorizeAsync();
        var res = await http.PutAsJsonAsync($"api/leaverequests/{id}/review", dto);
        return res.IsSuccessStatusCode ? (true, string.Empty) : (false, await res.Content.ReadAsStringAsync());
    }

    // ── Admin ─────────────────────────────────────────────────────────────────
    public async Task<List<AuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 20)
    {
        await AuthorizeAsync();
        return await http.GetFromJsonAsync<List<AuditLog>>($"api/admin/auditlogs?page={page}&pageSize={pageSize}") ?? [];
    }
}
