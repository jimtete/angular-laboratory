using LearningLab.Data.Models.Assets;

namespace LearningLab.Data.Models.Campaign.Maps;

public class Map
{
    public int Id { get; set; }

    public int? ParentMapId { get; set; }

    public Map? ParentMap { get; set; }

    public List<Map> ChildMaps { get; set; } = [];

    public int AssetId { get; set; }

    public Asset Asset { get; set; } = null!;

    public MapCategory Category { get; set; }

    public int ImageWidthPixels { get; set; }

    public int ImageHeightPixels { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<MapCampaign> Campaigns { get; set; } = [];

    public List<MapPin> Pins { get; set; } = [];

    public List<MapPinConnection> PinConnections { get; set; } = [];
}
