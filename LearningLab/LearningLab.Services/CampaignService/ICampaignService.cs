using LearningLab.Data.Models;
using LearningLab.Data.Models.DTOs.Campaign;

namespace LearningLab.Services.CampaignService;

public interface ICampaignService
{
    Task<ServiceResult<CampaignResponse>> CreateCampaignAsync(
        Guid userId,
        CreateCampaignRequest request,
        byte[]? campaignPictureBytes,
        string? campaignPictureContentType,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyList<CampaignResponse>>> GetAvailableCampaignsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
