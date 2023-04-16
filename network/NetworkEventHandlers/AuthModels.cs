
using System;
using Dungeoner.Server.Events;
using Dungeoner.Server.Events.NetworkEventHandlers;

[NetworkEventModel("Hello")]
public record HelloModel(string Username) : NetworkEventModel;
[NetworkEventModel("Welcome")]
public record WelcomeModel(Guid NewId, User[] Users) : NetworkEventModel;
[NetworkEventModel("NewClient")]
public record NewClientModel(Guid NewClientId, string Username) : NetworkEventModel;
[NetworkEventModel("ClientDisconnect")]
public record ClientDisconnectModel(Guid ClientId) : NetworkEventModel;