using LearningLab.Services.Eventing;
using LearningLab.Services.Eventing.CampaignSessions;
using LearningLab.Sockets.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets.CampaignSessions;

public sealed class CampaignSessionUpdatedSocketEventHandler
    : IApplicationEventHandler<CampaignSessionUpdatedEvent>
{
    private readonly IHubContext<CampaignSessionsHub> _hubContext;

    public CampaignSessionUpdatedSocketEventHandler(IHubContext<CampaignSessionsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task HandleAsync(
        CampaignSessionUpdatedEvent applicationEvent,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(SocketGroupNames.CampaignSessions(applicationEvent.CampaignId))
            .SendAsync(
                CampaignSessionSocketClientEvents.CampaignSessionUpdated,
                applicationEvent.Session,
                cancellationToken);
    }
}
