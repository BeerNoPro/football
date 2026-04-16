using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FootballBlog.API.Common;
using FootballBlog.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration config,
    ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>POST /api/auth/login — trả JWT token khi xác thực thành công</summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        logger.LogInformation("Login attempt for {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login failed — user not found: {Email}", request.Email);
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Email hoặc mật khẩu không đúng."));
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed — wrong password for {Email}", request.Email);
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Email hoặc mật khẩu không đúng."));
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        logger.LogInformation("Login success for {Email}", request.Email);
        return Ok(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto(
            Token: token,
            Email: user.Email!,
            DisplayName: user.UserName!,
            Roles: roles.ToList())));
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var key = config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key chưa cấu hình");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(string Token, string Email, string DisplayName, List<string> Roles);
