using LearningLab.Data.Models.AccessControl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LearningLab.Sockets;

[Authorize(Roles = AccessRoleNames.MasterOrPlayer)]
public sealed class NotificationsHub : Hub
{
}
