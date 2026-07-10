using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Character;

namespace LearningLab.Services.CharacterSheetService;

public interface ICharacterSheetService
{
    Task<ServiceResult<CharacterSheetResponse>> GetCharacterSheetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CharacterSheetResponse>> UpdateCharacterSheetAsync(
        Guid userId,
        UpdateCharacterSheetRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CharacterSheetResponse>> UpdateCharacterPortraitAsync(
        Guid userId,
        byte[] imageBytes,
        string? contentType,
        CancellationToken cancellationToken = default);
}
