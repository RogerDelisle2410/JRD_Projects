using JRD_Projects.Data;
using JRD_Projects.Models;
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

        // Calgary timezone (Edmonton ID)
        private static readonly TimeZoneInfo CalgaryTZ =
            TimeZoneInfo.FindSystemTimeZoneById("America/Edmonton");

        public VisitorController(AppDbContext db)
        {
            _db = db;
        } 

        // ⭐ ALWAYS LOG VISITS — NO OWNER CHECK
        [HttpPost("visit")]
        public async Task<IActionResult> Visit([FromBody] JObject? body)
        {
            Console.WriteLine("VISIT HIT");

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            string userAgent = Request.Headers["User-Agent"].ToString();
            string location = await LookupLocation(ip);

            // Convert UTC → Calgary BEFORE saving
            DateTime calgaryTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CalgaryTZ);

            _db.VisitorLog.Add(new VisitorLog
            {
                Ip = ip,
                UserAgent = userAgent,
                IsOwner = false,
                Location = location,
                Timestamp = calgaryTime   // <-- Calgary time stored in DB
            });

            await _db.SaveChangesAsync();

            return Ok(new { status = "ok" });
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
 