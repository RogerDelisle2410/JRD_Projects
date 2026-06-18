////using JRD_Projects.Data;
////using JRD_Projects.Models;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using Newtonsoft.Json.Linq;

////namespace JRD_Projects.Controllers
////{
////    [ApiController]
////    [Route("api/[controller]")]
////    public class VisitorController : ControllerBase
////    {
////        private readonly AppDbContext _db;
////        private readonly IHttpClientFactory _http;

////        public VisitorController(AppDbContext db, IHttpClientFactory http)
////        {
////            _db = db;
////            _http = http;
////        }

////        [HttpPost("visit")]
////        public async Task<IActionResult> Visit([FromBody] JObject body)
////        {
////            bool isOwner = false;

////            // SAFE PARSING — never throws
////            if (body != null && body.TryGetValue("owner", out var ownerToken))
////                isOwner = ownerToken.Value<bool>();

////            var vc = await _db.VisitorCount.FirstOrDefaultAsync();
////            if (vc == null)
////            {
////                vc = new VisitorCount
////                {
////                    count = 0,
////                    today_visits = 0
////                };
////                _db.VisitorCount.Add(vc);
////            }

////            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
////            string userAgent = Request.Headers["User-Agent"].ToString();

////            // Reset daily counter
////            if (vc.last_visit?.Date != DateTime.UtcNow.Date)
////                vc.today_visits = 0;

////            if (!isOwner)
////            {
////                vc.count++;
////                vc.today_visits++;
////            }

////            vc.last_visit = DateTime.UtcNow;
////            vc.last_ip = ip;
////            vc.last_user_agent = userAgent;

////            // Log entry
////            _db.VisitorLog.Add(new VisitorLog
////            {
////                Ip = ip,
////                UserAgent = userAgent,
////                IsOwner = isOwner,
////                Location = "Unknown",
////                Timestamp = DateTime.UtcNow
////            });

////            await _db.SaveChangesAsync();
////            return Ok(new { status = "ok" });
////        }

////        [HttpGet("debug-ip")]
////        public IActionResult DebugIp()
////        {
////            var remote = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
////            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();

////            return Ok(new
////            {
////                remoteIp = remote,
////                forwardedFor = forwarded
////            });
////        }


////        [HttpGet("getcount")]
////        public async Task<IActionResult> GetCount()
////        {
////            var vc = await _db.VisitorCount.FirstOrDefaultAsync();
////            if (vc == null)
////            {
////                vc = new VisitorCount { count = 0, today_visits = 0 };
////                _db.VisitorCount.Add(vc);
////                await _db.SaveChangesAsync();
////            }

////            return Ok(new
////            {
////                total = vc.count,
////                today = vc.today_visits,
////                lastVisit = vc.last_visit,
////                lastIp = vc.last_ip,
////                lastUserAgent = vc.last_user_agent
////            });
////        }

////        [HttpGet("visitlog")]
////        public async Task<IActionResult> VisitLog()
////        {
////            Console.WriteLine("REMOTE IP: " + HttpContext.Connection.RemoteIpAddress);
////            Console.WriteLine("X-FORWARDED-FOR: " + HttpContext.Request.Headers["X-Forwarded-For"]);

////            if (!IsOwnerRequest())
////                return Forbid();

////            var cutoff = DateTime.UtcNow.AddDays(-10);

////            var logs = await _db.VisitorLog
////                .Where(x => x.Timestamp >= cutoff)
////                .OrderByDescending(x => x.Timestamp)
////                .ToListAsync();

////            var result = logs.Select(l => new {
////                id = l.Id,
////                timestamp = l.Timestamp,
////                ip = l.Ip,
////                user_agent = l.UserAgent,
////                is_owner = l.IsOwner,
////                location = l.Location
////            });

////            return Ok(result);
////        } 
////        private bool IsOwnerRequest()
////        {
////            // 1. Check X-Forwarded-For (Azure real client IP)
////            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();

////            if (!string.IsNullOrEmpty(forwarded))
////            {
////                // forwarded looks like: "70.73.121.127:58793"
////                var realIp = forwarded.Split(',')[0].Split(':')[0].Trim();

////                if (realIp == "70.73.121.127")
////                    return true;
////            }

////            // 2. Fallback to direct connection IP (local dev)
////            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

////            if (ip == "127.0.0.1" || ip == "::1")
////                return true;

////            return false;
////        }

////    }
////}
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
        private readonly IHttpClientFactory _http;

        // Calgary timezone (Edmonton ID)
        private static readonly TimeZoneInfo CalgaryTZ =
            TimeZoneInfo.FindSystemTimeZoneById("America/Edmonton");

        public VisitorController(AppDbContext db, IHttpClientFactory http)
        {
            _db = db;
            _http = http;
        }

        // Convert UTC → Calgary + format
        private string? FormatCalgary(DateTime? utc)
        {
            if (utc == null)
                return null;

            var local = TimeZoneInfo.ConvertTimeFromUtc(utc.Value, CalgaryTZ);
            return local.ToString("yyyy-MM-dd HH:mm");
        }

        [HttpPost("visit")]
        public async Task<IActionResult> Visit([FromBody] JObject body)
        {
            bool isOwner = false;

            if (body != null && body.TryGetValue("owner", out var ownerToken))
                isOwner = ownerToken.Value<bool>();

            var vc = await _db.VisitorCount.FirstOrDefaultAsync();
            if (vc == null)
            {
                vc = new VisitorCount
                {
                    count = 0,
                    today_visits = 0
                };
                _db.VisitorCount.Add(vc);
            }

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            string userAgent = Request.Headers["User-Agent"].ToString();

            // Reset daily counter at midnight UTC
            if (vc.last_visit?.Date != DateTime.UtcNow.Date)
                vc.today_visits = 0;

            if (!isOwner)
            {
                vc.count++;
                vc.today_visits++;
            }

            vc.last_visit = DateTime.UtcNow;
            vc.last_ip = ip;
            vc.last_user_agent = userAgent;

            // Log entry
            _db.VisitorLog.Add(new VisitorLog
            {
                Ip = ip,
                UserAgent = userAgent,
                IsOwner = isOwner,
                Location = "Unknown",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok(new { status = "ok" });
        }

        [HttpGet("debug-ip")]
        public IActionResult DebugIp()
        {
            var remote = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();

            return Ok(new
            {
                remoteIp = remote,
                forwardedFor = forwarded
            });
        }

        [HttpGet("getcount")]
        public async Task<IActionResult> GetCount()
        {
            var vc = await _db.VisitorCount.FirstOrDefaultAsync();
            if (vc == null)
            {
                vc = new VisitorCount { count = 0, today_visits = 0 };
                _db.VisitorCount.Add(vc);
                await _db.SaveChangesAsync();
            }

            string formattedLastVisit = vc.last_visit.HasValue
                ? FormatCalgary(vc.last_visit.Value)
                : null;

            return Ok(new
            {
                total = vc.count,
                today = vc.today_visits,
                lastVisit = formattedLastVisit,
                lastIp = vc.last_ip,
                lastUserAgent = vc.last_user_agent
            });
        }

        [HttpGet("visitlog")]
        public async Task<IActionResult> VisitLog()
        {
            if (!IsOwnerRequest())
                return Forbid();

            var cutoff = DateTime.UtcNow.AddDays(-10);

            var logs = await _db.VisitorLog
                .Where(x => x.Timestamp >= cutoff)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            var result = logs.Select(l => new
            {
                id = l.Id,
                timestamp = FormatCalgary(l.Timestamp),
                ip = l.Ip,
                user_agent = l.UserAgent,
                is_owner = l.IsOwner,
                location = l.Location
            });

            return Ok(result);
        }

        private bool IsOwnerRequest()
        {
            // 1. Check X-Forwarded-For (Azure real client IP)
            var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].ToString();

            if (!string.IsNullOrEmpty(forwarded))
            {
                var realIp = forwarded.Split(',')[0].Split(':')[0].Trim();

                if (realIp == "70.73.121.127")
                    return true;
            }

            // 2. Local dev
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            if (ip == "127.0.0.1" || ip == "::1")
                return true;

            return false;
        }
    }
}
