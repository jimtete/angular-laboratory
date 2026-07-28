using LearningLab.Data.Models.Campaign.Presentation;

namespace LearningLab.Data.Repositories.CampaignPresentationRepository;

public interface ICampaignPresentationRepository
{
    Task<CampaignPresentation?> GetByCampaignSessionIdAsync(
        int campaignSessionId,
        CancellationToken cancellationToken = default);

    Task<CampaignPresentationEntry?> GetLatestEntryAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignPresentationStoryBeatSelection>> ListStoryBeatSelectionsAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default);

    Task<CampaignPresentationStoryBeatSelection?> GetStoryBeatSelectionAsync(
        int campaignPresentationId,
        Guid storyBlockId,
        int orderIndex,
        CancellationToken cancellationToken = default);

    Task<int?> GetLatestEntrySequenceAsync(
        int campaignPresentationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignPresentation presentation,
        CancellationToken cancellationToken = default);

    Task AddEntryAsync(
        CampaignPresentationEntry entry,
        CancellationToken cancellationToken = default);

    Task AddStoryBeatSelectionAsync(
        CampaignPresentationStoryBeatSelection selection,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
