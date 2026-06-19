using JRD_Projects.Data;
using JRD_Projects.Models;
using JRD_Projects.Services;
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
        private readonly EmailService _email;

        public VisitorController(AppDbContext db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        private static readonly TimeZoneInfo CalgaryTZ =
            TimeZoneInfo.FindSystemTimeZoneById("America/Edmonton");

        // ⭐ LOG VISIT (no body expected)
        [HttpPost("visit")]
        public async Task<IActionResult> Visit()
        {
            Console.WriteLine("VISIT HIT");

            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrWhiteSpace(ip) || ip == "::1")
                ip = "127.0.0.1";

            string location = await LookupLocation(ip);

            DateTime calgaryTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CalgaryTZ);

            var entry = new VisitorLog
            {
                Location = location,
                Timestamp = calgaryTime
            };

            _db.VisitorLog.Add(entry);
            await _db.SaveChangesAsync();

            string message = $"New visit at {calgaryTime:yyyy-MM-dd HH:mm:ss}\nLocation: {location}";
            _email.Send(location, message);

            return Ok(new { status = "ok", visitorId = entry.Id });
        }

        // ⭐ LOG PROJECT CLICK
        [HttpPost("logClick")]
        public async Task<IActionResult> LogClick([FromBody] ProjectClickDto dto)
        {
            Console.WriteLine($"CLICK: {dto.Project} for visitor {dto.VisitorId}");

            var visitor = await _db.VisitorLog.FindAsync(dto.VisitorId);
            if (visitor == null)
                return NotFound("Visitor not found");

            switch (dto.Project)
            {
                case "AI Docu Chat": visitor.ClickAIDocuChat = true; break;
                case "Angular": visitor.ClickAngular = true; break;
                case "Delivery Simulation": visitor.ClickDeliverySim = true; break;
                case "React": visitor.ClickReact = true; break;
                case "Elevator Simulator": visitor.ClickElevator = true; break;
                case "Subway Simulator": visitor.ClickSubway = true; break;
                case "Python Project": visitor.ClickPython = true; break;
                case "JavaScript Puzzle": visitor.ClickJSPuzzle = true; break;
                case "BattleShips": visitor.ClickBattleships = true; break;
                default:
                    return BadRequest("Unknown project name");
            }

            await _db.SaveChangesAsync();
            return Ok(new { status = "updated" });
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

            return Ok(new { total, today });
        }

        private static async Task<string> LookupLocation(string ip)
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

                return "Localhost";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    public class ProjectClickDto
    {
        public int VisitorId { get; set; }
        public string Project { get; set; } = "";
    }
}
