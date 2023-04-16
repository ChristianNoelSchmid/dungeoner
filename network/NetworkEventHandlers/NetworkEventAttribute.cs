using System;

namespace Dungeoner.Server.Events.NetworkEventHandlers;

[AttributeUsage(AttributeTargets.Class)]
public class NetworkEventAttribute : Attribute {
    public string Name { get; private set; }
    public NetworkEventAttribute(string name) => Name = name;
}