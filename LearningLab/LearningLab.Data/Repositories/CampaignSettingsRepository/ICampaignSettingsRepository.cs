using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Repositories.CampaignSettingsRepository;

public interface ICampaignSettingsRepository
{
    Task<CampaignSettings?> GetByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCampaignIdAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        CampaignSettings campaignSettings,
        CancellationToken cancellationToken = default);

    void Update(CampaignSettings campaignSettings);

    void Remove(CampaignSettings campaignSettings);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
