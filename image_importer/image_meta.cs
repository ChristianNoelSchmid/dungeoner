using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Dungeoner;

public record ImageMeta (
    [property:JsonPropertyName("file_path")] string FilePath,
    [property:JsonPropertyName("key")] string Key,
    [property:JsonPropertyName("pivot")] IReadOnlyList<int> Pivot
);