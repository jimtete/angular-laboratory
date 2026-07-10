using LearningLab.Data.Models;
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
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RegisterUser(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterUserAsync(request, cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Created(string.Empty, new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "User registered successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.UsernameAlreadyExists => Conflict(new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status409Conflict,
                Message = "Username is already taken.",
                Data = default
            }),
            ApplicationStatusCode.DefaultRoleNotFound => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<AuthResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "The default Player role is not configured.",
                    Data = default
                }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                Data = default
            })
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> LoginUser(
        LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginUserAsync(request, cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User logged in successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.InvalidCredentials => Unauthorized(new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid username or password.",
                Data = default
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<AuthResponse>
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred.",
                Data = default
            })
        };
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
