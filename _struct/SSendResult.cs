using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// Result of a Send operation. Send operations have the potenital error, meaning the
    /// connection is broken. If this happens IsDisconnected = true and DisconnectArgs 
    /// will have the reaosn why
    /// </summary>
    public struct SSendResult
    {
        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs DisconnectArgs;

        /// <summary>
        /// When attempting to send, was it found that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs));
    }

    /// <summary>
    /// Result of a Send operation. Send operations have the potenital error, meaning the
    /// connection is broken. If this happens IsDisconnected = true and DisconnectArgs 
    /// will have the reaosn why
    /// </summary>
    public struct SSendResult<T>
    {
        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs<T> DisconnectArgs;

        /// <summary>
        /// When attempting to send, was it found that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs<T>));
    }
}
