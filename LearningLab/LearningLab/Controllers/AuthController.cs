using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("hello-world")]
    public ActionResult<string> Get()
    {
        return "Hello World";
    }
}
