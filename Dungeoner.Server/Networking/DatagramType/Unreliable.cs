using System.IO;

namespace Server.Networking.Datagrams
{
    /// <summary>
    /// Sends a simple datagram, which will not
    /// be resent if droppped.
    /// </summary>
    public record Unreliable : Datagram
    {
        public byte[] Data { get; init; }
        public Unreliable(byte[] data) => Data = data;

        public static byte[] Create(byte[] data) => data;
    }
}