using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign.Presentation;
using LearningLab.Presentation.Models;

namespace LearningLab.Presentation.Hub;

public interface IPresentationModeHub
{
    Task<ServiceResult<CampaignPresentationResponse>> GetPresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignPresentationResponse>> InitiatePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignPresentationResponse>> DisablePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeWorkspaceResponse>> GetPresentationModeWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBlockResponse>> GetPresentationModeStoryBlockAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        Guid storyBlockId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeWorkspaceResponse>> EnablePresentationModeAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        InitiatePresentationModeRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeWorkspaceResponse>> DisablePresentationModeWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeWorkspaceResponse>> PresentStoryBeatWorkspaceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBeatPlayedResponse>> FinishStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        FinishPresentationStoryBeatRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkStoryBeatReferenceAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationStoryBeatReferenceRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkRoleplayingInformationGivenAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingInformationRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> MarkRoleplayingNpcInteractionGivenAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        MarkPresentationRoleplayingNpcInteractionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PresentationModeStoryBeatReferenceMarkedResponse>> TakeDecisionOptionAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        TakePresentationDecisionOptionRequest? request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CampaignPresentationResponse>> PresentStoryBeatAsync(
        Guid userId,
        Guid campaignId,
        int sessionId,
        PresentStoryBeatRequest? request,
        CancellationToken cancellationToken = default);
}
