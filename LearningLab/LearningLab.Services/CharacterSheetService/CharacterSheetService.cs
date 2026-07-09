using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Character;
using LearningLab.Data.Repositories.CharacterSheetRepository;
using CharacterAction = LearningLab.Data.Models.Character.Action;
using CharacterSheet = LearningLab.Data.Models.Character.CharacterSheet;

namespace LearningLab.Services.CharacterSheetService;

public sealed class CharacterSheetService : ICharacterSheetService
{
    private readonly ICharacterSheetRepository _characterSheetRepository;

    public CharacterSheetService(ICharacterSheetRepository characterSheetRepository)
    {
        _characterSheetRepository = characterSheetRepository;
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
}
