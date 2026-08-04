using LearningLab.Data.Models.Campaign.Stores;

namespace LearningLab.Data.Models.DTOs.Campaign.Stores;

public sealed class UpdateStoreLockStateRequest
{
    public StoreLockState LockState { get; init; }
}
