using LearningLab.Data.Models.DTOs.Campaign.Story;

namespace LearningLab.Data.Repositories.StoryBeatRoleplayingNpcQueryRepository;

public interface IStoryBeatRoleplayingNpcQueryRepository
{
    Task<IReadOnlyList<CampaignNpcResponse>> ListByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);
}
