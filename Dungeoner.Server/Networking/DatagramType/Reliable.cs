using System;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Server.Networking.Datagrams
{
    /// <summary>
    /// Represents a reliable datagram transmission, with an
    /// acknowledgement index, and the associated data.
    /// </summary>
    public record Reliable : Datagram
    {
        public ulong AckIndex { get; init; }
        public byte[] Data { get; init; }

        public Reliable(byte[] data)
        {
            AckIndex = BitConverter.ToUInt64(data);
            Data = data[4..];
        }

        public static byte[] Create(ulong ackIndex, byte[] data) {
            var msgBytes = Encoding.UTF8.GetBytes("REL");
            var ackBytes = BitConverter.GetBytes(ackIndex);

            return msgBytes.Concat(ackBytes).Concat(data).ToArray();
        }
    }
}