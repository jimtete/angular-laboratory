using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models.Campaign.Stores;

public class StoreEntry
{
    public int StoreId { get; set; }

    public StoreType StoreType { get; set; }

    public StoreLockState LockState { get; set; } = StoreLockState.Locked;

    public Campaign Campaign { get; set; } = null!;

    public Guid CampaignId { get; set; }

    public string StoreLocation { get; set; } = string.Empty;

    public string? StoreName { get; set; }

    public string? StoreDescription { get; set; }

    public int StoreDiscountPercentage { get; set; } = 0;

    public List<StoreItem> Items { get; set; } = [];
}
