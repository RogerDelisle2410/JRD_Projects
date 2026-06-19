using Microsoft.AspNetCore.Mvc;
using JRD_Projects.Services;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet("email")]
    public IActionResult TestEmail([FromServices] EmailService email)
    {
        email.Send("Gmail SMTP Test", "If you see this, Gmail SMTP is working!");
        return Ok("Email sent!");
    }
}
