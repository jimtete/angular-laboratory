using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Auth;
using LearningLab.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<Guid>>> RegisterUser(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authService.RegisterUserAsync(request, cancellationToken);

            return Created(string.Empty, new ApiResponse<Guid>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "User registered successfully.",
                Data = userId
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ApiResponse<Guid>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = exception.Message,
                Data = default
            });
        }
    }

    [HttpGet("hello-world")]
    public ActionResult<ApiResponse<string>> Get()
    {
        return Ok(new ApiResponse<string>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Request completed successfully.",
            Data = "Hello World"
        });
    }
}
