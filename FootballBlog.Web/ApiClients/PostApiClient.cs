namespace FootballBlog.Web.ApiClients;

public class PostApiClient : IPostApiClient
{
    private readonly HttpClient _httpClient;

    public PostApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
