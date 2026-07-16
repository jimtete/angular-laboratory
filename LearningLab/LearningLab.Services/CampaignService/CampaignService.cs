using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.Campaign;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Repositories.CampaignQueryRepository;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;
using LearningLab.Services.Configuration;
using Microsoft.Extensions.Options;
using CampaignModel = LearningLab.Data.Models.Campaign.Campaign;
using CampaignSettings = LearningLab.Data.Models.Campaign.CampaignSettings;

namespace LearningLab.Services.CampaignService;

public sealed class CampaignService : ICampaignService
{
    private readonly ICampaignQueryRepository _campaignQueryRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;
    private readonly CampaignPictureStorageOptions _campaignPictureStorageOptions;

    public CampaignService(
        ICampaignQueryRepository campaignQueryRepository,
        ICampaignRepository campaignRepository,
        IUserRepository userRepository,
        IOptions<CampaignPictureStorageOptions> campaignPictureStorageOptions)
    {
        _campaignQueryRepository = campaignQueryRepository;
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
        _campaignPictureStorageOptions = campaignPictureStorageOptions.Value;
    }

    public async Task<ServiceResult<CampaignResponse>> CreateCampaignAsync(
        Guid userId,
        CreateCampaignRequest request,
        byte[]? campaignPictureBytes,
        string? campaignPictureContentType,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<CampaignResponse>(
                ApplicationStatusCode.UserNotFound);
        }

        if (!HasRole(user, AccessRoleNames.Master))
        {
            return new ServiceResult<CampaignResponse>(
                ApplicationStatusCode.CampaignMasterRoleRequired);
        }

        if (campaignPictureBytes is not null)
        {
            if (campaignPictureBytes.LongLength > _campaignPictureStorageOptions.MaxFileSizeBytes)
            {
                return new ServiceResult<CampaignResponse>(
                    ApplicationStatusCode.CampaignPictureTooLarge);
            }

            if (!IsJpeg(campaignPictureBytes, campaignPictureContentType))
            {
                return new ServiceResult<CampaignResponse>(
                    ApplicationStatusCode.UnsupportedCampaignPictureFormat);
            }
        }

        var campaignId = Guid.NewGuid();
        var campaign = new CampaignModel
        {
            CampaignId = campaignId,
            GameMasterId = userId,
            CampaignName = request.CampaignName,
            Version = request.Version,
            DateCreated = DateTimeOffset.UtcNow,
            Settings = new CampaignSettings
            {
                CampaignId = campaignId,
                MaxNumberOfPlayers = 1,
                PassiveSkillsCheck = PassiveSkillsCheck.Manual
            }
        };

        if (campaignPictureBytes is not null)
        {
            campaign.CampaignPictureUrl = await StoreCampaignPictureAsync(
                campaignId,
                campaignPictureBytes,
                cancellationToken);
        }

        await _campaignRepository.AddAsync(campaign, cancellationToken);
        await _campaignRepository.SaveChangesAsync(cancellationToken);

        campaign.GameMaster = user;

        return new ServiceResult<CampaignResponse>(
            ApplicationStatusCode.Success,
            ToResponse(campaign));
    }

    public async Task<ServiceResult<IReadOnlyList<CampaignResponse>>> GetAvailableCampaignsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return new ServiceResult<IReadOnlyList<CampaignResponse>>(
                ApplicationStatusCode.UserNotFound);
        }

        var campaigns = await _campaignQueryRepository.GetByGameMasterIdAsync(
            userId,
            cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignResponse>>(
            ApplicationStatusCode.Success,
            campaigns);
    }

    private static bool HasRole(User user, string roleName)
    {
        return user.UserRoles.Any(userRole =>
            string.Equals(
                userRole.Role.Name,
                roleName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static CampaignResponse ToResponse(CampaignModel campaign)
    {
        return new CampaignResponse
        {
            CampaignId = campaign.CampaignId,
            GameMasterId = campaign.GameMasterId,
            GameMasterUsername = campaign.GameMaster.Username,
            CampaignName = campaign.CampaignName,
            Version = campaign.Version,
            CampaignPictureUrl = campaign.CampaignPictureUrl,
            DateCreated = campaign.DateCreated
        };
    }

    private async Task<string> StoreCampaignPictureAsync(
        Guid campaignId,
        byte[] campaignPictureBytes,
        CancellationToken cancellationToken)
    {
        var campaignFolderName = campaignId.ToString("D");
        var fileName = $"campaign_picture_{Guid.NewGuid():N}.jpg";
        var campaignAssetDirectory = Path.Combine(
            _campaignPictureStorageOptions.RootPath,
            "campaigns",
            campaignFolderName);
        var filePath = Path.Combine(campaignAssetDirectory, fileName);

        Directory.CreateDirectory(campaignAssetDirectory);
        await File.WriteAllBytesAsync(filePath, campaignPictureBytes, cancellationToken);

        var requestPath = _campaignPictureStorageOptions.RequestPath.TrimEnd('/');
        return $"{requestPath}/campaigns/{campaignFolderName}/{fileName}";
    }

    private static bool IsJpeg(byte[] imageBytes, string? contentType)
    {
        if (!string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return imageBytes.Length >= 4
            && imageBytes[0] == 0xFF
            && imageBytes[1] == 0xD8
            && imageBytes[^2] == 0xFF
            && imageBytes[^1] == 0xD9;
    }
}
