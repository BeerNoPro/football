using Microsoft.AspNetCore.Http;

namespace FootballBlog.Web.Services;

/// <summary>
/// DelegatingHandler để intercept 401 Unauthorized response.
/// Khi API trả 401 → xóa cookie jwt_token + redirect tới /admin/login?expired=true.
/// </summary>
public class ApiUnauthorizedHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // Nếu API trả 401 → session hết hạn hoặc token invalid
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                // Xóa cookie jwt_token (và auth cookie nếu cần)
                httpContext.Response.Cookies.Delete("jwt_token");

                // Lấy current URL để redirect lại sau khi login
                var returnUrl = httpContext.Request.PathBase + httpContext.Request.Path;

                if (!string.IsNullOrEmpty(httpContext.Request.QueryString.Value))
                {
                    returnUrl += httpContext.Request.QueryString.Value;
                }

                // Redirect tới login page với flag expired=true (show message cho user)
                httpContext.Response.Redirect($"/admin/login?expired=true&returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
        }

        return response;
    }
}
