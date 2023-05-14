using System;

namespace Dungeoner.Server.Events;

public record User(
    string Username,
    Guid Id
);