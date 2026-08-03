using LearningLab.Assets.Models.DTOs.Maps;
using LearningLab.Data.Models;

namespace LearningLab.Assets.Services;

public interface IMapService
{
    Task<ServiceResult<IReadOnlyList<MapResponse>>> GetCampaignMapsAsync(
        Guid userId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MapResponse>> CreateCampaignMapAsync(
        Guid userId,
        Guid campaignId,
        CreateCampaignMapRequest request,
        byte[]? mapFileBytes,
        string? mapFileContentType,
        CancellationToken cancellationToken = default);
}
