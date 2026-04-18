using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace FootballBlog.Web.Services;

/// <summary>
/// DelegatingHandler tự động thêm JWT Bearer token vào mọi request của AdminApiClient.
/// Token được đọc từ HTTP-only cookie "jwt_token" (được set ở Login.razor).
/// </summary>
public class JwtAuthHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies.TryGetValue("jwt_token", out var token) == true && !string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
