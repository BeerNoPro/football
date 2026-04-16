namespace FootballBlog.Web.Services;

/// <summary>
/// Scoped service lưu JWT token để DelegatingHandler dùng khi gọi API từ admin pages (InteractiveServer).
/// Populate từ AuthenticationState.User claim "jwt_token" trong mỗi component OnInitializedAsync.
/// </summary>
public class JwtTokenStore
{
    public string? Token { get; set; }
}
