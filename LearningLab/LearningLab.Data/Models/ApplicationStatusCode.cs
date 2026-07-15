namespace LearningLab.Data.Models;

public enum ApplicationStatusCode
{
    Success = 0,
    UsernameAlreadyExists = 1001,
    InvalidCredentials = 1002,
    DefaultRoleNotFound = 1003,
    CharacterSheetNotFound = 2001,
    UserNotFound = 3001,
    ProfilePictureRequired = 3002,
    ProfilePictureTooLarge = 3003,
    UnsupportedProfilePictureFormat = 3004,
    CampaignMasterRoleRequired = 4001,
    CampaignPictureTooLarge = 4002,
    UnsupportedCampaignPictureFormat = 4003,
    CampaignNotFound = 4004,
    InvalidCampaignSettings = 4005,
    InvalidCampaignInvite = 4006,
    CampaignInvitePlayerRoleRequired = 4007,
    CampaignInviteAlreadyExists = 4008,
    CampaignParticipantAlreadyExists = 4009,
    CampaignInviteNotFound = 4010,
    CampaignPlayerLimitReached = 4011,
    CampaignParticipantNotFound = 4012,
    InvalidCampaignMemberNickname = 4013
}
