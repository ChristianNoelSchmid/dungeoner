using System;

namespace Dungeoner.Server.Events.NetworkEventHandlers;

public record HostCallback(
    Action<NetworkEventModel, bool> SendToCaller,
    Action<NetworkEventModel, bool> SendToOthers,
    Action<NetworkEventModel, bool> SendToAll
);