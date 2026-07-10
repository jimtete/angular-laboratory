using LearningLab.Data.Models.Character;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;

namespace LearningLab.Data.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public CharacterSheet? CharacterSheet { get; set; }
    public List<UserRole> UserRoles { get; set; } = [];
    public List<Campaign.Campaign> OwnedCampaigns { get; set; } = [];
}
