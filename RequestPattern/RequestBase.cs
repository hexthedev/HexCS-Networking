using HexCS.Data.Persistence;
using HexCS.Data.Serialization;

namespace HexCS.Networking
{
    /// <summary>
    /// Implements common functionality shared by ReequestResponse objects
    /// </summary>
    public abstract class RequestBase<TType> : ISimpleSerializable, IMessagable
    {
        #region API
        /// <summary>
        /// The Role of this request pattern Message
        /// </summary>
        public ERequestRole Role { get; protected set; }

        /// <summary>
        /// A type used to differentiate purpose of message
        /// </summary>
        public TType Type { get; protected set; }

        /// <summary>
        /// The id associated with the message
        /// </summary>
        public uint RequestId { get; protected set; }

        /// <summary>
        /// The extra arguments associated with the message
        /// </summary>
        public byte[] Args { get; protected set; }

        /// <summary>
        /// Get as a message object
        /// </summary>
        /// <returns></returns>
        public Message AsMessage()
        {
            return new Message(GetBytes());
        }

        /// <summary>
        /// Construct this class from a byte[]. Can fail if byte is not correctly formatted
        /// </summary>
        /// <param name="bytes"></param>
        public abstract void ConstructFromBytes(byte[] bytes);

        /// <summary>
        /// Get this class a a byte[]
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetBytes();

        #endregion

        protected RequestBase() { }

        protected RequestBase(ERequestRole role, TType type, uint requestId, byte[] args)
        {
            Role = role;
            Type = type;
            RequestId = requestId;
            Args = args;
        }
    }
}
