using System.IO;
using System.Text;

namespace Server.Networking.Datagrams
{
    /// <summary>
    /// A datagram which informs the recipient to
    /// resend it's list of reliable datagram packets
    /// which have not been recieved by the sender yet.
    /// </summary>
    public record Resend : Datagram
    {
        public static byte[] Create() => Encoding.UTF8.GetBytes("RES");
    }
}