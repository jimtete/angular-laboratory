using LearningLab.Data.Models;
using LearningLab.Data.Models.AccessControl;
using LearningLab.Data.Models.DTOs.Campaign;
using LearningLab.Data.Repositories.CampaignRepository;
using LearningLab.Data.Repositories.UserRepository;
using CampaignModel = LearningLab.Data.Models.Campaign.Campaign;

namespace LearningLab.Services.CampaignService;

public sealed class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly IUserRepository _userRepository;

    public CampaignService(
        ICampaignRepository campaignRepository,
        IUserRepository userRepository)
    {
        _campaignRepository = campaignRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<CampaignResponse>> CreateCampaignAsync(
        Guid userId,
        CreateCampaignRequest request,
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

        var campaign = new CampaignModel
        {
            CampaignId = Guid.NewGuid(),
            GameMasterId = userId,
            CampaignName = request.CampaignName,
            Version = request.Version,
            CampaignPictureUrl = request.CampaignPictureUrl
        };

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

        var campaigns = await _campaignRepository.ListAsync(cancellationToken);

        return new ServiceResult<IReadOnlyList<CampaignResponse>>(
            ApplicationStatusCode.Success,
            campaigns.Select(ToResponse).ToList());
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
            CampaignPictureUrl = campaign.CampaignPictureUrl
        };
    }
}
