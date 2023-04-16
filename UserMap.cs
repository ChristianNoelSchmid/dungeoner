using Dungeoner.Server.Events;
using Godot;
using System;
using System.Collections.Generic;

public partial class UserMap : Node {

	public Guid ClientId { get; set; }
	private Dictionary<Guid, User> _users = new();

	public User this[Guid id] {
		get => _users[id];
		set => _users[id] = value;
	}

	public bool ContainsId(Guid id) => _users.ContainsKey(id);
	public void RemoveId(Guid id) => _users.Remove(id);

	public IEnumerable<User> Users => _users.Values;
}
