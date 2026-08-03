namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class UpdateStoreItemPurchaseRequest
{
    public long StoreItemId { get; init; }

    public int TimesSold { get; init; }
}
