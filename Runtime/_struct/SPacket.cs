using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// A packet of bytes that comes from some sender
    /// </summary>
    public struct SPacket
    {
        /// <summary>
        /// Sender of the packet
        /// </summary>
        public object Sender;

        /// <summary>
        /// packet data
        /// </summary>
        public byte[] Data;
    }

    /// <summary>
    /// A packet of bytes that comes from some sender
    /// </summary>
    /// <typeparam name="T">The type of sender</typeparam>
    public struct SPacket<T>
    {
        /// <summary>
        /// Sender of the packet
        /// </summary>
        public T Sender;

        /// <summary>
        /// packet data
        /// </summary>
        public byte[] Data;
    }
}
