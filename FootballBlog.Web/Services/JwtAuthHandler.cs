using System.Net.Http.Headers;

namespace FootballBlog.Web.Services;

/// <summary>
/// DelegatingHandler tự động thêm JWT Bearer token vào mọi request của AdminApiClient.
/// </summary>
public class JwtAuthHandler(JwtTokenStore tokenStore) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(tokenStore.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
