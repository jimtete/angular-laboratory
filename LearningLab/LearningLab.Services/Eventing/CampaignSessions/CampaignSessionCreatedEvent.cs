using LearningLab.Data.Models.DTOs.Campaign.Sessions;

namespace LearningLab.Services.Eventing.CampaignSessions;

public sealed class CampaignSessionCreatedEvent : IApplicationEvent
{
    public CampaignSessionCreatedEvent(CampaignSessionResponse session)
    {
        Session = session;
    }

    public CampaignSessionResponse Session { get; }

    public Guid CampaignId => Session.CampaignId;
}
