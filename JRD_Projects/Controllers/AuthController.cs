using JRD_Projects.Data;
using JRD_Projects.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace JRD_Projects.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly IConfiguration _config = config;

        // -------------------------
        // REGISTER
        // -------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {

            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { detail = "Email and password are required." });

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { detail = "Email already registered." });

            var user = new User
            {
                Email = dto.Email.Trim(),
                Hashed_Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Created_At = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { detail = "User registered successfully." });

            //return Ok(new { message = "User registered successfully." });
        }

        // -------------------------
        // LOGIN
        // -------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { detail = "Email and password are required." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { detail = "Invalid email or password." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Hashed_Password))
                return Unauthorized(new { detail = "Invalid email or password." });

            var token = GenerateJwtToken(user.Email);

            return Ok(new
            {
                token,   // ⭐ THIS IS THE ONLY CORRECT PROPERTY NAME
                email = user.Email
            });
        }


        [HttpGet("me")]
        public IActionResult Me()
        {
            var identity = HttpContext.User.Identity;

            if (identity == null || !identity.IsAuthenticated)
                return Unauthorized();

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new { email });
        }

        // -------------------------
        // JWT CREATION
        // -------------------------
        private string GenerateJwtToken(string email)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, email)   // ⭐ NEW — REQUIRED
    };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        } 
    }

    // -------------------------
    // DTOs
    // -------------------------
    public class RegisterDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
