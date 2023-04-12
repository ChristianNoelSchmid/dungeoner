using Dungeoner.Maps;
using Dungeoner.Server.Events;
using Godot;

[NetworkEventModel("TokenCreated")]
public record TokenCreatedModel(string Key, float Id, float X, float Y) : NetworkEventModel;

[NetworkEvent("TokenCreated")]
public partial class TokenCreatedEventHandler : NetworkEventHandler<TokenCreatedModel> {
    [Export]
    private TokenMap _tokenMap;

	public override void _Process(double delta) {
		while(TryGetEvent(out var model, out var endPoint)) {

        }
	}
}