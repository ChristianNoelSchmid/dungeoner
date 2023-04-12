using System;
using Dungeoner.Server.Events;

[NetworkEventModel("TokenMoved")]
public record TokenMovedModel(int Id, float X, float Y) : NetworkEventModel;

[NetworkEvent("TokenMoved")]
public partial class TokenMovedEventHandler : NetworkEventHandler<TokenMovedModel> {
	public override void _Process(double delta) {
		while(TryGetEvent(out var model, out var endPoint)) {
            Console.WriteLine($"from {endPoint}: {model}");
            SendTo(endPoint, model with { X = model.X + 1, Y = model.Y + 1}, false);
        }
	}
}
