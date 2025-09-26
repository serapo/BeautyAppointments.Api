using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BeautyAppointments.Api.Data;
using BeautyAppointments.Api.Domain;
using Microsoft.AspNetCore.Authorization;



namespace BeautyAppointments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }
    // POST api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        // 1) Basit validasyon
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Email ve Password gerekli." });

        // 2) E-posta benzersiz mi?
        var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
        if (exists) return Conflict(new { message = "Email zaten kayıtlı." });

        // 3) Şifreyi hash'le
        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        // 4) DB'ye kaydet
        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = hash,
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created("", new { message = "Kayıt oluşturuldu." });
    }

    // POST api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        // 1) Kullanıcı var mı?
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user == null) return Unauthorized(new { message = "Geçersiz email/şifre." });

        // 2) Şifre doğru mu?
        var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        if (!ok) return Unauthorized(new { message = "Geçersiz email/şifre." });

        // 3) Token üret
        var token = GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> DevResetPassword( [FromServices] IWebHostEnvironment env,[FromBody] ResetDto dto)
    {
        if (!env.IsDevelopment()) return Forbid(); // PROD'ta kapalı

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user is null) return NotFound("Kullanıcı bulunamadı.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        return Ok("Şifre güncellendi.");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if(user is null) return Unauthorized();

        return Ok(new {user.Id, user.FullName, user.Email, user.Role } );

    }
    // JWT üretimi
    private string GenerateToken(User user)
    {
        var key = _config["Jwt:Key"] ?? "dev-super-secret-key-change-me";
        var issuer = _config["Jwt:Issuer"] ?? "BeautyAppointments";
        var audience = _config["Jwt:Audience"] ?? "BeautyAppointmentsUsers";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 120;

        Console.WriteLine($"[JWT-GEN] issuer={issuer}, audience={audience}, keylen={key?.Length}, keyhead={key?[..Math.Min(4, key.Length)]}");

        var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
        SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var minutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var parsed) ? parsed : 120;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record ResetDto(string Email, string NewPassword);
