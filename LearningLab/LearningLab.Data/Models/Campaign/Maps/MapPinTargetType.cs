namespace LearningLab.Data.Models.Campaign.Maps;

public enum MapPinTargetType
{
    Placeholder = 0,
    StoryBlock = 1,
    Map = 2,
    Store = 3,
    PlayersPosition = 4,

    [Obsolete("Use PlayersPosition.")]
    PlayerPosition = PlayersPosition
}
