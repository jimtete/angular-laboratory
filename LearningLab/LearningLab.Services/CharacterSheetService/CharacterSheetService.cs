using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Character;
using LearningLab.Data.Repositories.CharacterSheetRepository;
using LearningLab.Services.Configuration;
using Microsoft.Extensions.Options;
using CharacterAction = LearningLab.Data.Models.Character.Action;
using CharacterSheet = LearningLab.Data.Models.Character.CharacterSheet;

namespace LearningLab.Services.CharacterSheetService;

public sealed class CharacterSheetService : ICharacterSheetService
{
    private readonly ICharacterSheetRepository _characterSheetRepository;
    private readonly ProfilePictureStorageOptions _profilePictureStorageOptions;

    public CharacterSheetService(
        ICharacterSheetRepository characterSheetRepository,
        IOptions<ProfilePictureStorageOptions> profilePictureStorageOptions)
    {
        _characterSheetRepository = characterSheetRepository;
        _profilePictureStorageOptions = profilePictureStorageOptions.Value;
    }

    public async Task<ServiceResult<CharacterSheetResponse>> GetCharacterSheetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var characterSheet = await _characterSheetRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return characterSheet is null
            ? new ServiceResult<CharacterSheetResponse>(ApplicationStatusCode.CharacterSheetNotFound)
            : new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.Success,
                ToResponse(characterSheet));
    }

    public async Task<ServiceResult<CharacterSheetResponse>> UpdateCharacterSheetAsync(
        Guid userId,
        UpdateCharacterSheetRequest request,
        CancellationToken cancellationToken = default)
    {
        var characterSheet = await _characterSheetRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (characterSheet is null)
        {
            return new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.CharacterSheetNotFound);
        }

        characterSheet.PortraitUrl = request.PortraitUrl;
        characterSheet.Background = request.Background;
        characterSheet.Information = request.Information;
        characterSheet.FirstName = request.FirstName;
        characterSheet.LastName = request.LastName;
        characterSheet.CharacterClass = request.CharacterClass;
        characterSheet.Nationality = request.Nationality;
        characterSheet.Height = request.Height;
        characterSheet.Weight = request.Weight;
        characterSheet.Actions = request.Actions
            .Select(action => new CharacterAction
            {
                ActionType = action.ActionType,
                Title = action.Title,
                Description = action.Description
            })
            .ToList();
        characterSheet.Traits = [.. request.Traits];
        characterSheet.Equipment = [.. request.Equipment];
        characterSheet.LogicRating = request.LogicRating;
        characterSheet.PsycheRating = request.PsycheRating;
        characterSheet.PhysicalRating = request.PhysicalRating;
        characterSheet.MotoricsRating = request.MotoricsRating;

        await _characterSheetRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CharacterSheetResponse>(
            ApplicationStatusCode.Success,
            ToResponse(characterSheet));
    }

    public async Task<ServiceResult<CharacterSheetResponse>> UpdateCharacterPortraitAsync(
        Guid userId,
        byte[] imageBytes,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (imageBytes.Length == 0)
        {
            return new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.ProfilePictureRequired);
        }

        if (imageBytes.LongLength > _profilePictureStorageOptions.MaxFileSizeBytes)
        {
            return new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.ProfilePictureTooLarge);
        }

        if (!IsJpeg(imageBytes, contentType))
        {
            return new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.UnsupportedProfilePictureFormat);
        }

        var characterSheet = await _characterSheetRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (characterSheet is null)
        {
            return new ServiceResult<CharacterSheetResponse>(
                ApplicationStatusCode.CharacterSheetNotFound);
        }

        var profilePictureId = Guid.NewGuid();
        var userFolderName = userId.ToString("D");
        var fileName = $"profile_pic_{profilePictureId:N}.jpg";
        var userAssetDirectory = Path.Combine(
            _profilePictureStorageOptions.RootPath,
            "users",
            userFolderName);
        var filePath = Path.Combine(userAssetDirectory, fileName);

        Directory.CreateDirectory(userAssetDirectory);
        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

        var requestPath = _profilePictureStorageOptions.RequestPath.TrimEnd('/');
        characterSheet.PortraitUrl = $"{requestPath}/users/{userFolderName}/{fileName}";

        await _characterSheetRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CharacterSheetResponse>(
            ApplicationStatusCode.Success,
            ToResponse(characterSheet));
    }

    private static CharacterSheetResponse ToResponse(CharacterSheet characterSheet)
    {
        return new CharacterSheetResponse
        {
            UserId = characterSheet.UserId,
            PortraitUrl = characterSheet.PortraitUrl,
            Background = characterSheet.Background,
            Information = characterSheet.Information,
            FirstName = characterSheet.FirstName,
            LastName = characterSheet.LastName,
            CharacterClass = characterSheet.CharacterClass,
            Nationality = characterSheet.Nationality,
            Height = characterSheet.Height,
            Weight = characterSheet.Weight,
            Actions = characterSheet.Actions
                .Select(action => new CharacterActionDto
                {
                    ActionType = action.ActionType,
                    Title = action.Title,
                    Description = action.Description
                })
                .ToList(),
            Traits = [.. characterSheet.Traits],
            Equipment = [.. characterSheet.Equipment],
            LogicRating = characterSheet.LogicRating,
            PsycheRating = characterSheet.PsycheRating,
            PhysicalRating = characterSheet.PhysicalRating,
            MotoricsRating = characterSheet.MotoricsRating
        };
    }

    private static bool IsJpeg(byte[] imageBytes, string? contentType)
    {
        if (!string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return imageBytes.Length >= 4
            && imageBytes[0] == 0xFF
            && imageBytes[1] == 0xD8
            && imageBytes[^2] == 0xFF
            && imageBytes[^1] == 0xD9;
    }
}
