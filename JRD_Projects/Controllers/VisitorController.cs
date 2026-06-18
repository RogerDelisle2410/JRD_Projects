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

        private string? FormatCalgary(DateTime? utc)
        {
            if (utc == null)
                return null;

            var local = TimeZoneInfo.ConvertTimeFromUtc(utc.Value, CalgaryTZ);
            return local.ToString("yyyy-MM-dd HH:mm");
        }

        // ⭐ ALWAYS LOG VISITS — NO OWNER CHECK
        [HttpPost("visit")]
        public async Task<IActionResult> Visit([FromBody] JObject? body)
        {
            Console.WriteLine("VISIT HIT");

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            string userAgent = Request.Headers["User-Agent"].ToString();

            _db.VisitorLog.Add(new VisitorLog
            {
                Ip = ip,
                UserAgent = userAgent,
                IsOwner = false,
                Location = "Unknown",
                Timestamp = DateTime.UtcNow
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

        // ⭐ Remove owner logic entirely
        // ⭐ Remove visitlog endpoint if not needed
    }
}


//using JRD_Projects.Data;
//using JRD_Projects.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json.Linq;

//namespace JRD_Projects.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class VisitorController : ControllerBase
//    {
//        private readonly AppDbContext _db;
//        private readonly IHttpClientFactory _http;

//        // Calgary timezone (Edmonton ID)
//        private static readonly TimeZoneInfo CalgaryTZ =
//            TimeZoneInfo.FindSystemTimeZoneById("America/Edmonton");

//        public VisitorController(AppDbContext db, IHttpClientFactory http)
//        {
//            _db = db;
//            _http = http;
//        }

//        // Convert UTC → Calgary + format
//        private string? FormatCalgary(DateTime? utc)
//        {
//            if (utc == null)
//                return null;

//            var local = TimeZoneInfo.ConvertTimeFromUtc(utc.Value, CalgaryTZ);
//            return local.ToString("yyyy-MM-dd HH:mm");
//        }

//        [HttpPost("visit")]
//        public async Task<IActionResult> Visit([FromBody] JObject? body)  
//        {
//            Console.WriteLine("VISIT HIT");
//            Console.WriteLine("Owner? " + IsOwnerRequest());
//            Console.WriteLine("IP: " + HttpContext.Connection.RemoteIpAddress);
//            Console.WriteLine("Forwarded: " + HttpContext.Request.Headers["X-Forwarded-For"]);

//            bool isOwner = false;

//            if (body != null && body.TryGetValue("owner", out var ownerToken))
//                isOwner = ownerToken.Value<bool>();

//            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
//            string userAgent = Request.Headers["User-Agent"].ToString();

//            if (!IsOwnerRequest())
//            {
//                _db.VisitorLog.Add(new VisitorLog
//                {
//                    Ip = ip,
//                    UserAgent = userAgent,
//                    IsOwner = isOwner,
//                    Location = "Unknown",
//                    Timestamp = DateTime.UtcNow
//                });

//                await _db.SaveChangesAsync();
//            }

//            return Ok(new { status = "ok" });
//        }


//        [HttpGet("debug-ip")]
//        public IActionResult DebugIp()
//        {
//            var remote = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
//            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();

//            return Ok(new
//            {
//                remoteIp = remote,
//                forwardedFor = forwarded
//            });
//        }

//        [HttpGet("getcount")]
//        public async Task<IActionResult> GetCount()
//        {
//            var total = await _db.VisitorLog.CountAsync();

//            var todayUtc = DateTime.UtcNow.Date;

//            var today = await _db.VisitorLog
//                .Where(x => x.Timestamp >= todayUtc &&
//                            x.Timestamp < todayUtc.AddDays(1))
//                .CountAsync();

//            return Ok(new
//            {
//                total,
//                today
//            });
//        }
//        [HttpGet("isowner")]
//        public IActionResult IsOwner()
//        {
//            return Ok(new { isOwner = IsOwnerRequest() });
//        }

//        [HttpGet("visitlog")]
//        public async Task<IActionResult> VisitLog()
//        {
//            if (!IsOwnerRequest())
//                return Forbid();

//            var cutoff = DateTime.UtcNow.AddDays(-10);

//            var logs = await _db.VisitorLog
//                .Where(x => x.Timestamp >= cutoff)
//                .OrderByDescending(x => x.Timestamp)
//                .ToListAsync();

//            var result = logs.Select(l => new
//            {
//                id = l.Id,
//                timestamp = FormatCalgary(l.Timestamp),
//                ip = l.Ip,
//                user_agent = l.UserAgent,
//                is_owner = l.IsOwner,
//                location = l.Location
//            });

//            return Ok(result);
//        }

//        private bool IsOwnerRequest()
//        {
//            // Read headers
//            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
//            var remote = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

//            Console.WriteLine("=== OWNER CHECK ===");
//            Console.WriteLine("Forwarded: " + forwarded);
//            Console.WriteLine("Remote:    " + remote);

//            // Determine real IP
//            string realIp = null;

//            if (!string.IsNullOrWhiteSpace(forwarded))
//                realIp = forwarded.Split(',')[0].Split(':')[0].Trim();

//            if (string.IsNullOrWhiteSpace(realIp))
//                realIp = remote;

//            // Normalize IPv6-mapped IPv4
//            if (realIp != null && realIp.Contains("::ffff:"))
//                realIp = realIp.Replace("::ffff:", "");

//            Console.WriteLine("Real IP:   " + realIp);
//            Console.WriteLine("===================");

//            // ⭐ Your real home IP
//            if (realIp == "70.73.121.127")
//                return true;

//            // ⭐ Allow localhost for development
//            if (realIp == "127.0.0.1" || realIp == "::1")
//                return true;

//            return false;
//        }

//    }
//}
