using JRD_Projects.Data;
using JRD_Projects.Models;
using JRD_Projects.Services; // Add this line if EmailService is in the Services namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace JRD_Projects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly EmailService? _email;

        // Calgary timezone (Edmonton ID)
        private static readonly TimeZoneInfo CalgaryTZ =
            TimeZoneInfo.FindSystemTimeZoneById("America/Edmonton");

        // ⭐ ALWAYS LOG VISITS — NO OWNER CHECK 
        [HttpPost("visit")]
        public async Task<IActionResult> Visit()
        {
            Console.WriteLine("VISIT HIT");

            // Get IP (but do NOT store it)
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || ip == "::1")
                ip = "127.0.0.1";

            // Lookup location
            string location = await LookupLocation(ip);

            DateTime calgaryTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CalgaryTZ);

            // Store ONLY location + timestamp
            _db.VisitorLog.Add(new VisitorLog
            {
                Location = location,
                Timestamp = calgaryTime
            });

            await _db.SaveChangesAsync();

            // Email notification
            string message = $"New visit at {calgaryTime:yyyy-MM-dd HH:mm:ss}\nLocation: {location}";
            _email.Send("New Visitor", message);

            return Ok(new { status = "ok" });
        }

        public VisitorController(AppDbContext db, EmailService email)
        {
            _db = db;
            _email = email;
        }


        [HttpGet("getcount")]
        public async Task<IActionResult> GetCount()
        {
            var total = await _db.VisitorLog.CountAsync();

            var todayUtc = DateTime.UtcNow.Date;

            var today = await _db.VisitorLog
                .Where(x => x.Timestamp >= todayUtc &&
                            x.Timestamp < todayUtc.AddDays(1))
                .CountAsync();

            return Ok(new
            {
                total,
                today
            });
        }

        private async Task<string> LookupLocation(string ip)
        {
            try
            {
                var cleanIp = ip.Split(':')[0];
                using var http = new HttpClient();
                var json = await http.GetStringAsync($"http://ip-api.com/json/{cleanIp}");
                var data = JObject.Parse(json);

                var city = data["city"]?.ToString();
                var country = data["country"]?.ToString();

                if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(country))
                    return $"{city}, {country}";

                if (!string.IsNullOrEmpty(country))
                    return country;

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

    }
}