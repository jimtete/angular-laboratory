using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Repositories.CampaignQuestRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignQuestService;

public sealed class CampaignQuestService : ICampaignQuestService
{
    private readonly ICampaignQuestRepository _campaignQuestRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public CampaignQuestService(
        ICampaignQuestRepository campaignQuestRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _campaignQuestRepository = campaignQuestRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignQuestResponse>>> GetCampaignQuestsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<CampaignQuestResponse>>(
                validationStatusCode.Value);
        }

        var quests = await _campaignQuestRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignQuestResponse>>(
            ApplicationStatusCode.Success,
            quests.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<CampaignQuestResponse>> CreateCampaignQuestAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignQuestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryBuildCampaignQuest(
                campaignId,
                request,
                DateTimeOffset.UtcNow,
                out var quest))
        {
            return new ServiceResult<CampaignQuestResponse>(
                ApplicationStatusCode.InvalidCampaignQuest);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<CampaignQuestResponse>(
                validationStatusCode.Value);
        }

        await _campaignQuestRepository.AddAsync(
            quest,
            cancellationToken);
        await _campaignQuestRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignQuestResponse>(
            ApplicationStatusCode.Success,
            ToResponse(quest));
    }

    private async Task<ApplicationStatusCode?> ValidateMasterCampaignAccessAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return ApplicationStatusCode.UserNotFound;
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return ApplicationStatusCode.CampaignMasterRoleRequired;
        }

        var campaign = await _campaignRepository.GetByIdForGameMasterAsync(
            campaignId,
            userId,
            cancellationToken);

        return campaign is null
            ? ApplicationStatusCode.CampaignNotFound
            : null;
    }

    private static bool TryBuildCampaignQuest(
        Guid campaignId,
        CreateCampaignQuestRequest? request,
        DateTimeOffset timestamp,
        out CampaignQuest quest)
    {
        quest = new CampaignQuest();

        var title = request?.Title?.Trim();
        var description = request?.Description?.Trim();
        var givenBy = request?.GivenBy?.Trim();
        var reward = request?.Reward?.Trim();

        if (request is null
            || !Enum.IsDefined(request.Type)
            || string.IsNullOrWhiteSpace(title)
            || string.IsNullOrWhiteSpace(description)
            || string.IsNullOrWhiteSpace(givenBy)
            || string.IsNullOrWhiteSpace(reward)
            || request.Tasks.Count == 0
            || request.Tasks.Any(task => task is null))
        {
            return false;
        }

        var questId = Guid.NewGuid();
        var tasks = new List<CampaignQuestTask>();

        foreach (var taskRequest in request.Tasks)
        {
            var taskTitle = taskRequest.Title?.Trim();
            var taskDescription = taskRequest.Description?.Trim();

            if (string.IsNullOrWhiteSpace(taskTitle)
                || string.IsNullOrWhiteSpace(taskDescription))
            {
                return false;
            }

            tasks.Add(new CampaignQuestTask
            {
                QuestTaskId = Guid.NewGuid(),
                QuestId = questId,
                Title = taskTitle,
                Description = taskDescription,
                DateCompleted = taskRequest.DateCompleted,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
        }

        quest = new CampaignQuest
        {
            QuestId = questId,
            CampaignId = campaignId,
            Type = request.Type,
            Title = title,
            Description = description,
            GivenBy = givenBy,
            Reward = reward,
            CompletedAt = request.CompletedAt,
            Tasks = tasks,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };

        return true;
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static CampaignQuestResponse ToResponse(CampaignQuest quest)
    {
        return new CampaignQuestResponse
        {
            QuestId = quest.QuestId,
            CampaignId = quest.CampaignId,
            Type = quest.Type,
            Title = quest.Title,
            Description = quest.Description,
            GivenBy = quest.GivenBy,
            Reward = quest.Reward,
            CompletedAt = quest.CompletedAt,
            Tasks = quest.Tasks
                .OrderBy(task => task.DateCompleted != null)
                .ThenBy(task => task.Title)
                .Select(ToResponse)
                .ToList(),
            CreatedAt = quest.CreatedAt,
            UpdatedAt = quest.UpdatedAt
        };
    }

    private static CampaignQuestTaskResponse ToResponse(CampaignQuestTask task)
    {
        return new CampaignQuestTaskResponse
        {
            QuestTaskId = task.QuestTaskId,
            QuestId = task.QuestId,
            Title = task.Title,
            Description = task.Description,
            DateCompleted = task.DateCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
