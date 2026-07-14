using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Data.Repositories.CampaignQueryRepository;

public interface ICampaignQueryRepository
{
    Task<IReadOnlyList<CampaignResponse>> GetByGameMasterIdAsync(
        Guid gameMasterId,
        CancellationToken cancellationToken = default);
}
