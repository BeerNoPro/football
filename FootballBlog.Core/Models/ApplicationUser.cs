using Microsoft.AspNetCore.Identity;

namespace FootballBlog.Core.Models;

public class ApplicationUser : IdentityUser<int>
{
    // Id, UserName, Email, PasswordHash, SecurityStamp v.v. đã có trong IdentityUser<int>
    // Role được quản lý qua AspNetRoles — không cần field riêng

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
