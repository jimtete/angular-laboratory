using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Services.Eventing.CampaignSessions;

public sealed class CampaignSessionUpdatedEvent : IApplicationEvent
{
    public CampaignSessionUpdatedEvent(CampaignSessionResponse session)
    {
        Session = session;
    }

    public CampaignSessionResponse Session { get; }

    public Guid CampaignId => Session.CampaignId;
}
