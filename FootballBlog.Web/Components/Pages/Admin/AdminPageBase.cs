using System.Security.Claims;
using FootballBlog.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FootballBlog.Web.Components.Pages.Admin;

/// <summary>
/// Base class cho tất cả admin pages — tự động lấy JWT từ auth state và populate JwtTokenStore.
/// </summary>
public abstract class AdminPageBase : ComponentBase
{
    [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] protected JwtTokenStore TokenStore { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthProvider.GetAuthenticationStateAsync();
        TokenStore.Token = authState.User.FindFirst("jwt_token")?.Value;
        await OnAdminInitializedAsync();
    }

    protected virtual Task OnAdminInitializedAsync() => Task.CompletedTask;
}
