using System;

namespace Dungeoner.Server.Events.NetworkEventHandlers;

[AttributeUsage(AttributeTargets.Class)]
public class NetworkEventModelAttribute : Attribute
{
    public string Name { get; private set; }
    public NetworkEventModelAttribute(string name) => Name = name;
}