
using System;
using Dungeoner.Server.Events.NetworkEventHandlers;

namespace Dungeoner.Server.Events;

[NetworkEventModel("Ping")]
public record PingModel(Guid Id) : NetworkEventModel;