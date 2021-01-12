using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// Result of a Receive operation. Is attempting to receive causes a disconnection to be
    /// detected, IsDisconnected will = true and DisconnectArgs will have info as to why
    /// </summary>
    public struct SReceiveResult
    {
        /// <summary>
        /// Data received
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs DisconnectArgs;

        /// <summary>
        /// When attempting to receive, was it foudn that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs));
    }

    /// <summary>
    /// Result of a Receive operation. Is attempting to receive causes a disconnection to be
    /// detected, IsDisconnected will = true and DisconnectArgs will have info as to why
    /// </summary>
    public struct SReceiveResult<T>
    {
        /// <summary>
        /// Data received
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs<T> DisconnectArgs;

        /// <summary>
        /// When attempting to receive, was it foudn that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs<T>));
    }
}
