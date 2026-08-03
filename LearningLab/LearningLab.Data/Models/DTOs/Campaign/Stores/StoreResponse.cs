using LearningLab.Data.Models.Campaign.Stores;

namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class StoreResponse
{
    public int StoreId { get; init; }

    public Guid CampaignId { get; init; }

    public StoreType StoreType { get; init; }

    public string StoreLocation { get; init; } = string.Empty;

    public string? StoreName { get; init; }

    public string? StoreDescription { get; init; }

    public IReadOnlyList<StoreItemResponse> Items { get; init; } = [];
}
