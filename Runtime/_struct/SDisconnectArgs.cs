using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// Arguments explaining why a disconnection occured
    /// </summary>
    public struct SDisconnectArgs
    {
        /// <summary>
        /// The disconnected connection
        /// </summary>
        public object Connection;

        /// <summary>
        /// Human readable reason for disconnection
        /// </summary>
        public string ReadableReason;

        /// <summary>
        /// The associated exception
        /// </summary>
        public Exception Exception;
    }

    /// <summary>
    /// Arguments explaining why a disconnection occured
    /// </summary>
    /// <typeparam name="T">The type of sender</typeparam>
    public struct SDisconnectArgs<T>
    {
        /// <summary>
        /// The disconnected connection
        /// </summary>
        public T Connection;

        /// <summary>
        /// Human readable reason for disconnection
        /// </summary>
        public string ReadableReason;

        /// <summary>
        /// The associated exception
        /// </summary>
        public Exception Exception;
    }
}
