using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApp.API.Models;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ProfileRepository _profileRepo;
    private readonly IConfiguration _configuration;

    public AuthController(ProfileRepository profileRepo, IConfiguration configuration)
    {
        _profileRepo = profileRepo;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existing = await _profileRepo.GetByLoginAsync(dto.Login);
        if (existing != null)
            return BadRequest(new { message = "Логин уже занят" });

        var profile = new Profile(dto.Login, dto.Password, dto.FirstName, dto.LastName, dto.BirthYear);
        await _profileRepo.AddAsync(profile);

        var token = GenerateJwtToken(profile);
        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = profile.Id,
            Name = profile.Name
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var profile = await _profileRepo.GetByLoginAsync(dto.Login);
        if (profile == null || !profile.CheckPassword(dto.Password))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = GenerateJwtToken(profile);
        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = profile.Id,
            Name = profile.Name
        });
    }

    private string GenerateJwtToken(Profile profile)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT Key not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new Claim(ClaimTypes.Name, profile.Login)
        };

        var expireMinutes = double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}