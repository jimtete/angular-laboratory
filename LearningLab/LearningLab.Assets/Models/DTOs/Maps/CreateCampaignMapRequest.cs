using LearningLab.Data.Models.Campaign.Maps;

namespace LearningLab.Assets.Models.DTOs.Maps;

public sealed class CreateCampaignMapRequest
{
    public int? ParentMapId { get; init; }

    public MapCategory Category { get; init; }

    public int ImageWidthPixels { get; init; }

    public int ImageHeightPixels { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
