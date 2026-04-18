using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FootballBlog.Web.Components.Pages.Admin;

/// <summary>
/// Base class cho tất cả admin pages — tự động đọc JWT từ request cookies.
/// JwtAuthHandler sẽ tự động attach token vào Authorization header khi gọi API.
/// </summary>
public abstract class AdminPageBase : ComponentBase
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;

    protected int CurrentUserId { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        // Token được lưu ở secure HTTP-only cookie "jwt_token" trong Login.razor
        // JwtAuthHandler sẽ đọc từ cookie và thêm vào Authorization header tự động
        int.TryParse(authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
        CurrentUserId = userId;
        await OnAdminInitializedAsync();
    }

    protected virtual Task OnAdminInitializedAsync() => Task.CompletedTask;
}
