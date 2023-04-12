using System.IO;
using System.Net;

namespace Server.Networking.Resolvers 
{
    /// <summary>
    /// Contains all information related to a reliable
    /// datagram's acknowledgement information.
    /// </summary>
    /// <param name="AckIndex">The acknowledgement index</param>
    /// <param name="IPEndPoint">The Client's endpoint</param>
    /// <param name="TicksStart">The tick-count at which the datagram was sent</param>
    /// <param name="Data">The datagram message itself</param>
    /// <returns></returns>
    public record AckResolverData (
        ulong AckIndex, IPEndPoint IPEndPoint,
        long TicksStart, byte [] Data
    );
}