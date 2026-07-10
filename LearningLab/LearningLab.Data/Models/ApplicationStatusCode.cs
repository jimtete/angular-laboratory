namespace LearningLab.Data.Models;

public enum ApplicationStatusCode
{
    Success = 0,
    UsernameAlreadyExists = 1001,
    InvalidCredentials = 1002,
    CharacterSheetNotFound = 2001,
    ProfilePictureRequired = 3002,
    ProfilePictureTooLarge = 3003,
    UnsupportedProfilePictureFormat = 3004
}
