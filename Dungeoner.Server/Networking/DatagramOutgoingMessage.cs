
using System.Net;

public enum DatagramOutgoingSendType {
    SendTo,
    SendToAll,
    SendToOthers
}

public record DatagramOutgoingMessage (
    DatagramOutgoingSendType SendType,
    byte[] Data,
    bool IsRel,
    IPEndPoint[]? EndPoints = null
);