
using System;

namespace Dungeoner.Server.Events;

public class NetworkAuthResult
{
    public byte[]? Data { get; private set; }
    public bool IsSuccessful => Data != null;
    public bool IsCompleted => Data?.Length == 0;

    public NetworkAuthResult(byte[] data) => Data = data;

    private static NetworkAuthResult _failedResult = new(null!);
    private static NetworkAuthResult _completedResult = new(Array.Empty<byte>());

    public static NetworkAuthResult Failed => _failedResult;
    public static NetworkAuthResult Completed => _completedResult;
}