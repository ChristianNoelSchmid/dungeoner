using System;

namespace Dungeoner.Server.Events.NetworkEventHandlers;

public record ClientCallback(Action<NetworkEventModel, bool> SendToHost);