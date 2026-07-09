using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Character;
using LearningLab.Services.CharacterSheetService;
using LearningLab.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningLab.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CharacterController : ControllerBase
{
    private readonly ICharacterSheetService _characterSheetService;

    public CharacterController(ICharacterSheetService characterSheetService)
    {
        _characterSheetService = characterSheetService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CharacterSheetResponse>>> FetchCharacterSheet(
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse();
        }

        var result = await _characterSheetService.GetCharacterSheetAsync(
            userId.Value,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CharacterSheetResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Character sheet fetched successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.CharacterSheetNotFound => NotFound(
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Character sheet was not found.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CharacterSheetResponse>>> SaveCharacterSheet(
        UpdateCharacterSheetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse();
        }

        var result = await _characterSheetService.UpdateCharacterSheetAsync(
            userId.Value,
            request,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CharacterSheetResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Character sheet saved successfully.",
                Data = result.Data
            }),
            ApplicationStatusCode.CharacterSheetNotFound => NotFound(
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Character sheet was not found.",
                    Data = null
                }),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "An unexpected error occurred.",
                    Data = null
                })
        };
    }

    private UnauthorizedObjectResult InvalidUserClaimResponse()
    {
        return Unauthorized(new ApiResponse<CharacterSheetResponse>
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            Message = "The access token does not contain a valid user identifier.",
            Data = null
        });
    }
}
