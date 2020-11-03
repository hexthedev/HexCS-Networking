using System;
using System.Runtime.Serialization;
using HexCS.Core;
using HexCS.Data.Serialization;

namespace HexCS.Networking
{
    /// <summary>
    /// Requests are messages that also have an id
    /// </summary>
    public class IdentifiableMessage : ISimpleSerializable
    {
        private static uint NextId = 0;

        /// <summary>
        /// Id of message
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// The message
        /// </summary>
        public Message Message { get; private set; }

        /// <summary>
        /// Make with message, and auto increment id
        /// </summary>
        /// <param name="message"></param>
        public IdentifiableMessage(Message message)
        {
            Id = NextId++;
            Message = message;
        }

        /// <summary>
        /// Empty constructor for deserialization only
        /// </summary>
        public IdentifiableMessage() { }

        /// <inheritdoc/>
        public void ConstructFromBytes(byte[] bytes)
        {
            using (ByteArrayExtractor ext = new ByteArrayExtractor(bytes))
            {
                Id = ext.ExtractUInt();
                Message = UTSerialization.DeserializeSimple<Message>(ext.ExtractRemaining());
            }
        }

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            using (ArrayConstructor<byte> array = new ArrayConstructor<byte>())
            {
                array.AppendToArray(BitConverter.GetBytes(Id));
                array.AppendToArray(Message.GetBytes());
                return array.ToArray();
            }
        }

        /// <summary>
        /// For C# Serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected IdentifiableMessage(SerializationInfo info, StreamingContext context)
        {
            Id = (uint)info.GetValue(nameof(Id), typeof(uint));
            Message = (Message)info.GetValue(nameof(Message), typeof(Message));
        }
    }
}
