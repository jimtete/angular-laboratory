namespace LearningLab.Data.Models.Campaign.Stores;

public class StoreItem
{
    public long StoreItemId { get; set; }

    public StoreEntry Store { get; set; } = null!;

    public int StoreId { get; set; }

    public int? Quantity { get; set; } = null;

    public int TimesSold { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public string? ItemDescription { get; set; }

    public int ItemPrice { get; set; }

    public int ItemPriceDiscount { get; set; } = 0;

    public int ItemPricePercentageDiscount { get; set; } = 0;
}
