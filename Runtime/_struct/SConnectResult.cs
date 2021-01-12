using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// Result of a Connect operation. If the connection fails, the info related to the 
    /// failure will be present in Disconnect args and IsDisconnected = true
    /// </summary>
    public struct SConnectResult
    {
        /// <summary>
        /// Did the operation result in a connection to the remote host
        /// </summary>
        public bool IsConnected;

        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs DisconnectArgs;

        /// <summary>
        /// When attempting to connect, was it foudn that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs));
    }

    /// <summary>
    /// Result of a Connect operation. If the connection fails, the info related to the 
    /// failure will be present in Disconnect args and IsDisconnected = true
    /// </summary>
    public struct SConnectResult<T>
    {
        /// <summary>
        /// Did the operation result in a connection to the remote host
        /// </summary>
        public bool IsConnected;

        /// <summary>
        /// Disconnect args
        /// </summary>
        public SDisconnectArgs<T> DisconnectArgs;

        /// <summary>
        /// When attempting to connect, was it foudn that the client is disconnected
        /// </summary>
        public bool IsDisconnected => !DisconnectArgs.Equals(default(SDisconnectArgs<T>));
    }
}
