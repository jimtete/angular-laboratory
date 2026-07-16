using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.CampaignSessions;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets.CampaignSessions;

public sealed class CampaignSessionCreatedSocketEventHandler
    : IApplicationEventHandler<CampaignSessionCreatedEvent>
{
    private readonly IHubContext<CampaignSessionsHub> _hubContext;

    public CampaignSessionCreatedSocketEventHandler(IHubContext<CampaignSessionsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task HandleAsync(
        CampaignSessionCreatedEvent applicationEvent,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(SocketGroupNames.CampaignSessions(applicationEvent.CampaignId))
            .SendAsync(
                CampaignSessionSocketClientEvents.CampaignSessionCreated,
                applicationEvent.Session,
                cancellationToken);
    }
}
