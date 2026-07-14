using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs;
using LearningLab.Data.Models.DTOs.Character;
using LearningLab.Infrastructure.StaticAssets;
using LearningLab.Parsers;
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
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrls(result.Data)
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
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrls(result.Data)
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

    [HttpPost("profile-picture")]
    public async Task<ActionResult<ApiResponse<CharacterSheetResponse>>> UploadProfilePicture(
        [FromForm] IFormFile? profilePicture,
        CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(User);

        if (userId is null)
        {
            return InvalidUserClaimResponse();
        }

        var imageBytes = await MediaParser.ReadProfilePictureBytesAsync(profilePicture, cancellationToken);
        var result = await _characterSheetService.UpdateCharacterPortraitAsync(
            userId.Value,
            imageBytes,
            profilePicture?.ContentType,
            cancellationToken);

        return result.StatusCode switch
        {
            ApplicationStatusCode.Success => Ok(new ApiResponse<CharacterSheetResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Character profile picture uploaded successfully.",
                Data = result.Data is null
                    ? null
                    : WithPublicAssetUrls(result.Data)
            }),
            ApplicationStatusCode.ProfilePictureRequired => BadRequest(
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "A profile picture file is required.",
                    Data = null
                }),
            ApplicationStatusCode.ProfilePictureTooLarge => StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status413PayloadTooLarge,
                    Message = "Profile picture must be 5 MB or smaller.",
                    Data = null
                }),
            ApplicationStatusCode.UnsupportedProfilePictureFormat => StatusCode(
                StatusCodes.Status415UnsupportedMediaType,
                new ApiResponse<CharacterSheetResponse>
                {
                    StatusCode = StatusCodes.Status415UnsupportedMediaType,
                    Message = "Profile picture must be a JPEG image.",
                    Data = null
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

    private CharacterSheetResponse WithPublicAssetUrls(CharacterSheetResponse characterSheet)
    {
        return new CharacterSheetResponse
        {
            UserId = characterSheet.UserId,
            PortraitUrl = Request.ToPublicStaticAssetUrl(characterSheet.PortraitUrl),
            Background = characterSheet.Background,
            Information = characterSheet.Information,
            FirstName = characterSheet.FirstName,
            LastName = characterSheet.LastName,
            CharacterClass = characterSheet.CharacterClass,
            Nationality = characterSheet.Nationality,
            Height = characterSheet.Height,
            Weight = characterSheet.Weight,
            Actions = characterSheet.Actions,
            Traits = characterSheet.Traits,
            Equipment = characterSheet.Equipment,
            LogicRating = characterSheet.LogicRating,
            PsycheRating = characterSheet.PsycheRating,
            PhysicalRating = characterSheet.PhysicalRating,
            MotoricsRating = characterSheet.MotoricsRating
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
