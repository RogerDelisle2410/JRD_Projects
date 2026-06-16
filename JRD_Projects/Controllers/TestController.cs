using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JRD_Projects.Data;

namespace JRD_Projects.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TestController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            try
            {
                // Test VisitorCount
                var vc = await _db.VisitorCount.FirstOrDefaultAsync();

                // Test VisitorLog (just count rows)
                int logCount = await _db.VisitorLog.CountAsync();

                return Ok(new
                {
                    status = "OK",
                    message = "Database connection successful.",
                    visitorCountRow = vc,
                    visitorLogEntries = logCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "ERROR",
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}
