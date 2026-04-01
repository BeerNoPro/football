namespace FootballBlog.Core.Models;

public class ApplicationUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Author"; // Admin | Author

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
