using System;
using System.Collections.Generic;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// Connections can send and receive data and be disconnected
    /// </summary>
    /// <typeparam name="TClient">The object that represents the conneciton. Normally the object that is used to interface with the hardware. Exmaple: for Tcp it's System.Net.Sockets.TcpClient</typeparam>
    public interface IConnection<TClient>
    {
        /// <summary>
        /// Invoked when this conneciton is disconnected. If a connection is disconnected, it
        /// a new connect needs to be created. This connection is dead and isposed
        /// </summary>
        event Action<SDisconnectArgs<TClient>> OnDisconnectAsync;

        /// <summary>
        /// Invoked when a packet is received
        /// </summary>
        event Action<SPacket<TClient>> OnReceiveAsync;

        /// <summary>
        /// Enqueue a message for sending over the network. This is non-blocking, and
        /// normally involves a send queue in which another thread performs the actual send
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data);

        /// <summary>
        /// Disconnects this connection, which causes the Connection to be disposed leaving it
        /// in an unusable state. 
        /// </summary>
        void Disconnect();
    }
}
