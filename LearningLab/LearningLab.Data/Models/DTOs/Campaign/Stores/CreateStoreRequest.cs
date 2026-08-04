using System.ComponentModel.DataAnnotations;
using LearningLab.Data.Models.Campaign.Stores;

namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class CreateStoreRequest
{
    public StoreType StoreType { get; init; }

    [MaxLength(256)]
    public string? StoreLocation { get; init; }

    [MaxLength(256)]
    public string? StoreName { get; init; }

    [MaxLength(4096)]
    public string? StoreDescription { get; init; }

    public int StoreDiscountPercentage { get; init; }

    public IReadOnlyList<CreateStoreItemRequest> Items { get; init; } = [];
}
