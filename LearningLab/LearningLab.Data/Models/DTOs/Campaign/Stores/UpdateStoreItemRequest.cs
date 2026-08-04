using System.ComponentModel.DataAnnotations;

namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class UpdateStoreItemRequest
{
    public long? StoreItemId { get; init; }

    public int? Quantity { get; init; }

    [MaxLength(256)]
    public string? ItemName { get; init; }

    [MaxLength(4096)]
    public string? ItemDescription { get; init; }

    public int ItemPrice { get; init; }

    public int ItemPriceDiscount { get; init; }

    public int ItemPricePercentageDiscount { get; init; }
}
