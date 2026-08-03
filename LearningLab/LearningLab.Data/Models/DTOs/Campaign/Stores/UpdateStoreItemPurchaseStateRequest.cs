namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class UpdateStoreItemPurchaseStateRequest
{
    public IReadOnlyList<UpdateStoreItemPurchaseRequest> Items { get; init; } = [];
}
