using LearningLab.Data.Models;
using LearningLab.Data.Models.Campaign.Sessions;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Services.CampaignSessionService;

public interface ICampaignSessionService
{
    Task<ServiceResult<IReadOnlyList<CampaignSessionResponse>>> GetAvailableCampaignSessionsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateCampaignSessionAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        UpdateCampaignSessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionDateAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        DateTimeOffset? sessionDate,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignSessionDescriptionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? description,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<SessionNoteResponse>>> GetSessionNotesAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignMemberInformationResponse>>> GetSessionPlayersAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateGenericSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? content,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateItemFoundSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        string? content,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateImportantChoiceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateImportantChoiceSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateCampaignMilestoneSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateCampaignMilestoneSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateLevelUpOrMechanicsChangeSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CreateLevelUpOrMechanicsChangeSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateStoryBeatPlayedSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        string? content = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> CreateStoryBeatReferenceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        SessionNoteStoryBeatReferenceType referenceType,
        Guid? referenceId = null,
        string? content = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> TakeDecisionStoryBeatOptionSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBeatId,
        Guid decisionOptionId,
        string? content = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateStoryBeatReferenceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        UpdateStoryBeatReferenceSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> AchieveCampaignMilestoneAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        AchieveCampaignMilestoneRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        string? content,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateImportantChoiceSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateImportantChoiceSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateCampaignMilestoneSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateCampaignMilestoneSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> UpdateLevelUpOrMechanicsChangeSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        UpdateLevelUpOrMechanicsChangeSessionNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignSessionResponse>> DeleteSessionNoteAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        int noteId,
        CancellationToken cancellationToken = default);
}
