
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeoner;

public static class Rect2Extensions {
    public static Rect2 OuterBounds(this IEnumerable<Rect2> rects) {
        return new Rect2 {
            Position = new(rects.Min(r => r.Position.X), rects.Min(r => r.Position.Y)),
            End = new(rects.Max(r => r.End.X), rects.Max(r => r.End.Y))
        };
    }
}