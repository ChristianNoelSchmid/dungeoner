using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.Networking.Datagrams
{
    /// <summary>
    /// Contains information related to which acknowledgement
    /// index either the Client or Server is sending / receiving.
    /// </summary>
    public record Ack : Datagram
    {
        // The index of the acknowledgement
        public ulong AckIndex { get; init; }
        public Ack(byte[] data) {
            AckIndex = BitConverter.ToUInt64(data);
        }

        public static byte[] Create(ulong ackIndex) => Encoding.UTF8.GetBytes($"::ACK{ackIndex}");
    }
}