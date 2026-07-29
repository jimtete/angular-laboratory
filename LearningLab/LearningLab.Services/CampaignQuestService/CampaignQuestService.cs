using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign.Quests;
using LearningLab.Data.Models.DTOs.Campaign.Quests;
using LearningLab.Data.Repositories.CampaignQuestRepository;
using LearningLab.Data.Repositories.CampaignQuestTaskRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.StoryBeatQuestTaskRepository;
using LearningLab.Data.Repositories.StoryBeatRepository;
using LearningLab.Data.Repositories.UserRepository;

namespace LearningLab.Services.CampaignQuestService;

public sealed class CampaignQuestService : ICampaignQuestService
{
    private readonly ICampaignQuestRepository _campaignQuestRepository;
    private readonly ICampaignQuestTaskRepository _campaignQuestTaskRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IStoryBeatQuestTaskRepository _storyBeatQuestTaskRepository;
    private readonly IStoryBeatRepository _storyBeatRepository;
    private readonly IUserRepository _userRepository;

    public CampaignQuestService(
        ICampaignQuestRepository campaignQuestRepository,
        ICampaignQuestTaskRepository campaignQuestTaskRepository,
        ICampaignRepository campaignRepository,
        IStoryBeatQuestTaskRepository storyBeatQuestTaskRepository,
        IStoryBeatRepository storyBeatRepository,
        IUserRepository userRepository)
    {
        _campaignQuestRepository = campaignQuestRepository;
        _campaignQuestTaskRepository = campaignQuestTaskRepository;
        _campaignRepository = campaignRepository;
        _storyBeatQuestTaskRepository = storyBeatQuestTaskRepository;
        _storyBeatRepository = storyBeatRepository;
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

    public async Task<ServiceResult<CampaignQuestResponse>> UpdateCampaignQuestAsync(
        Guid userId,
        Guid campaignId,
        Guid questId,
        UpdateCampaignQuestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (questId == Guid.Empty
            || !TryBuildCampaignQuestUpdate(
                questId,
                request,
                DateTimeOffset.UtcNow,
                out var updatedQuest))
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

        var quest = await _campaignQuestRepository.GetByCampaignIdAndQuestIdAsync(
            campaignId,
            questId,
            cancellationToken);

        if (quest is null)
        {
            return new ServiceResult<CampaignQuestResponse>(
                ApplicationStatusCode.CampaignQuestNotFound);
        }

        quest.Type = updatedQuest.Type;
        quest.Title = updatedQuest.Title;
        quest.Description = updatedQuest.Description;
        quest.GivenBy = updatedQuest.GivenBy;
        quest.Reward = updatedQuest.Reward;
        quest.CompletedAt = updatedQuest.CompletedAt;
        quest.UpdatedAt = updatedQuest.UpdatedAt;

        quest.Tasks.Clear();
        foreach (var task in updatedQuest.Tasks)
        {
            quest.Tasks.Add(task);
        }

        await _campaignQuestRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<CampaignQuestResponse>(
            ApplicationStatusCode.Success,
            ToResponse(quest));
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>> GetStoryBeatQuestTasksAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        CancellationToken cancellationToken = default)
    {
        if (storyBeatId == Guid.Empty)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
                ApplicationStatusCode.InvalidCampaignQuestTask);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
                validationStatusCode.Value);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        var links = await _storyBeatQuestTaskRepository.ListByStoryBeatIdAsync(
            storyBeatId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
            ApplicationStatusCode.Success,
            links.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>> GetCampaignStoryBeatQuestTasksAsync(
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
            return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
                validationStatusCode.Value);
        }

        var links = await _storyBeatQuestTaskRepository.ListByCampaignIdAsync(
            campaignId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<StoryBeatQuestTaskResponse>>(
            ApplicationStatusCode.Success,
            links.Select(ToResponse).ToList());
    }

    public async Task<ServiceResult<StoryBeatQuestTaskResponse>> LinkQuestTaskToStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        if (storyBeatId == Guid.Empty || questTaskId == Guid.Empty)
        {
            return new ServiceResult<StoryBeatQuestTaskResponse>(
                ApplicationStatusCode.InvalidCampaignQuestTask);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<StoryBeatQuestTaskResponse>(
                validationStatusCode.Value);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<StoryBeatQuestTaskResponse>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        var questTask = await _campaignQuestTaskRepository.GetByCampaignIdAndTaskIdAsync(
            campaignId,
            questTaskId,
            cancellationToken);

        if (questTask is null)
        {
            return new ServiceResult<StoryBeatQuestTaskResponse>(
                ApplicationStatusCode.CampaignQuestTaskNotFound);
        }

        var existingAssignment = await _storyBeatQuestTaskRepository.GetByCampaignIdAndQuestTaskIdAsync(
            campaignId,
            questTaskId,
            cancellationToken);

        if (existingAssignment is not null)
        {
            if (existingAssignment.StoryBeatId == storyBeatId)
            {
                return new ServiceResult<StoryBeatQuestTaskResponse>(
                    ApplicationStatusCode.StoryBeatQuestTaskAlreadyExists);
            }

            return new ServiceResult<StoryBeatQuestTaskResponse>(
                ApplicationStatusCode.CampaignQuestTaskAlreadyAssignedToStoryBeat,
                ToResponse(existingAssignment));
        }

        var link = new StoryBeatQuestTask
        {
            StoryBeatId = storyBeatId,
            StoryBeat = storyBeat,
            QuestTaskId = questTaskId,
            QuestTask = questTask,
            LinkedAt = DateTimeOffset.UtcNow
        };

        await _storyBeatQuestTaskRepository.AddAsync(link, cancellationToken);
        await _storyBeatQuestTaskRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<StoryBeatQuestTaskResponse>(
            ApplicationStatusCode.Success,
            ToResponse(link));
    }

    public async Task<ServiceResult<object>> UnlinkQuestTaskFromStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        Guid storyBeatId,
        Guid questTaskId,
        CancellationToken cancellationToken = default)
    {
        if (storyBeatId == Guid.Empty || questTaskId == Guid.Empty)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.InvalidCampaignQuestTask);
        }

        var validationStatusCode = await ValidateMasterCampaignAccessAsync(
            userId,
            campaignId,
            cancellationToken);

        if (validationStatusCode is not null)
        {
            return new ServiceResult<object>(
                validationStatusCode.Value);
        }

        var storyBeat = await _storyBeatRepository.GetByCampaignIdAndStoryBeatIdAsync(
            campaignId,
            storyBeatId,
            cancellationToken);

        if (storyBeat is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.StoryBeatNotFound);
        }

        var questTask = await _campaignQuestTaskRepository.GetByCampaignIdAndTaskIdAsync(
            campaignId,
            questTaskId,
            cancellationToken);

        if (questTask is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.CampaignQuestTaskNotFound);
        }

        var link = await _storyBeatQuestTaskRepository.GetByStoryBeatIdAndQuestTaskIdAsync(
            storyBeatId,
            questTaskId,
            cancellationToken);

        if (link is null)
        {
            return new ServiceResult<object>(
                ApplicationStatusCode.StoryBeatQuestTaskNotFound);
        }

        _storyBeatQuestTaskRepository.Remove(link);
        await _storyBeatQuestTaskRepository.SaveChangesAsync(cancellationToken);

        return new ServiceResult<object>(ApplicationStatusCode.Success);
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

    private static bool TryBuildCampaignQuestUpdate(
        Guid questId,
        UpdateCampaignQuestRequest? request,
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
            Type = request.Type,
            Title = title,
            Description = description,
            GivenBy = givenBy,
            Reward = reward,
            CompletedAt = request.CompletedAt,
            Tasks = tasks,
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

    private static StoryBeatQuestTaskResponse ToResponse(StoryBeatQuestTask link)
    {
        return new StoryBeatQuestTaskResponse
        {
            StoryBeatId = link.StoryBeatId,
            StoryBlockId = link.StoryBeat.StoryBlockId,
            QuestTaskId = link.QuestTaskId,
            QuestId = link.QuestTask.QuestId,
            StoryBeatTitle = link.StoryBeat.Title,
            StoryBlockTitle = link.StoryBeat.StoryBlock.Title,
            StoryBlockOrderIndex = link.StoryBeat.StoryBlock.OrderIndex,
            StoryBeatOrderIndex = link.StoryBeat.OrderIndex,
            StoryBeatSecondaryOrderIndex = link.StoryBeat.SecondaryOrderIndex,
            Title = link.QuestTask.Title,
            Description = link.QuestTask.Description,
            DateCompleted = link.QuestTask.DateCompleted,
            LinkedAt = link.LinkedAt
        };
    }
}
