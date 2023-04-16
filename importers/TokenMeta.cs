using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dungeoner;

public class TokenMeta : IMetaFile {
    [JsonPropertyName("file_path")] 
    public string FilePath { get; set; }
    [JsonPropertyName("key")] 
    public string Key { get; set; }
    [JsonPropertyName("pivot")] 
    public IReadOnlyList<int> Pivot { get; set; }
}