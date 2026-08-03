namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class StoreItemResponse
{
    public long StoreItemId { get; init; }

    public int StoreId { get; init; }

    public int? Quantity { get; init; }

    public int TimesSold { get; init; }

    public string ItemName { get; init; } = string.Empty;

    public string? ItemDescription { get; init; }

    public int ItemPrice { get; init; }

    public int ItemPriceDiscount { get; init; }

    public int ItemPricePercentageDiscount { get; init; }
}
