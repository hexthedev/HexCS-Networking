using HexCS.Core;
using HexCS.Data.Persistence;
using HexCS.Data.Serialization;

namespace HexCS.Networking
{
    /// <summary>
    /// A message is generic data sent to another entitity
    /// </summary>
    public class Message : ISimpleSerializable
    {
        /// <summary>
        /// The bytes that make up the message
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Create a message with bytes
        /// </summary>
        /// <param name="bytes"></param>
        public Message(byte[] bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Empty constructor for deserialization only
        /// </summary>
        public Message() { }

        /// <inheritdoc/>
        public void ConstructFromBytes(byte[] bytes)
        {
            Bytes = bytes.ShallowCopy();
        }

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            return Bytes.ShallowCopy();
        }
    }
}
