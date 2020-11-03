using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HexCS.Core;
using HexCS.Parallel;

namespace HexCS.Networking
{
    /// <summary>
    /// Simple TCP server that is responsible for
    ///  * Listening for TCP connections from clients and registering connected clients
    ///  * Receiving byte[] data and passing as events
    ///  * Sending byte[] data to all clients
    ///  * Sending byte[] data to a single client
    ///  * Handling disconnection when TCP connection fails
    ///  
    /// It's important to note that all events can be called from different threads.
    /// </summary>
    public class SimpleTcpServer : ADisposableManager
    {
        private const int cMillisPerCheck = 10;

        // data
        private List<TcpClientConnection> _connections = new List<TcpClientConnection>();

        // actions
        private RecurrentAsyncAction _acceptConnection;

        #region Protected API
        /// <inheritdoc />
        protected override string AccessAfterDisposalMessage => $"Disposed Error: Cannot access {nameof(SimpleTcpServer)} after disposal";

        /// <inheritdoc />
        protected override void SetDisposablesToNull()
        {
            TcpListener?.Stop();
            _acceptConnection = null;
        }
        #endregion

        #region Public API
        /// <summary>
        /// The endpoint that the server represents. So the local endpoint on this computer
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// The TcpListener instance that is used to listen for incoming
        /// connections
        /// </summary>
        public TcpListener TcpListener { get; private set; }

        /// <summary>
        /// Invoked when a client connects
        /// </summary>
        public event Action<TcpClient> OnClientConnectedAsync;

        /// <summary>
        /// Invoked when a client is disconnected. The client is disposed and 
        /// removed from server connections
        /// </summary>
        public event Action<SDisconnectArgs<TcpClient>> OnClientDisconnectAsync;

        /// <summary>
        /// Invoked when the server receives a message
        /// </summary>
        public event Action<SPacket<TcpClient>> OnMessageRecievedAsync;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="endpoint">The endpoint of the server</param>
        public SimpleTcpServer(IPEndPoint endpoint) : base()
        {
            EndPoint = endpoint;
            TcpListener = new TcpListener(EndPoint);

            _acceptConnection = new RecurrentAsyncAction(HandleAttemptAccept);
            RegisterInteralDisposable(_acceptConnection);

            TcpListener.Start();
        }

        /// <summary>
        /// Disconnect all connections
        /// </summary>
        public void DisconnectAll()
        {
            ThrowErrorIfDisposed();

            TcpClientConnection[] connections = _connections.ToArray();

            foreach (TcpClientConnection con in connections)
            {
                con?.Disconnect();
            }

            _connections.Clear();
        }

        /// <summary>
        /// Disconnect a connection
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        public void DisconnectConnection(TcpClient client)
        {
            if (client == null) return;
            int index = _connections.QueryIndexOf(c => c.Client == client);
            if (index == -1) return;
            _connections[index].Disconnect();
        }

        /// <summary>
        /// Send to all connections
        /// </summary>
        /// <param name="data"></param>
        public void SendToAll(byte[] data)
        {
            TcpClientConnection[] connections = _connections.ToArray();

            foreach (TcpClientConnection con in connections)
            {
                if (con == null) continue;
                con.Send(data);
            }
        }

        /// <summary>
        /// Send to a specific connection
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        public void SendToConnection(TcpClient client, byte[] data)
        {
            if (client == null) return;
            int index = _connections.QueryIndexOf(c => c.Client == client);
            if (index == -1) return;
            _connections[index].Send(data);
        }
        #endregion

        /// <summary>
        /// Attempts to accept a connection. If a conneciton is accepted, 
        /// adds to the conneciton queue. 
        /// </summary>
        /// <returns></returns>
        private async Task HandleAttemptAccept()
        {
            TcpClient client = await TcpListener.AcceptTcpClientAsync();

            if (client != null)
            {
                TcpClientConnection connection = new TcpClientConnection(client);

                connection.OnDisconnectAsync += dis =>
                {
                    _connections.Remove(connection);

                    OnClientDisconnectAsync?.Invoke(new SDisconnectArgs<TcpClient>()
                    {
                        Connection = dis.Connection,
                        Exception = dis.Exception,
                        ReadableReason = dis.ReadableReason
                    });
                };

                connection.OnReceiveAsync += pack =>
                {
                    OnMessageRecievedAsync?.Invoke(new SPacket<TcpClient>()
                    {
                        Sender = client,
                        Data = pack.Data
                    });
                };

                _connections.Add(connection);
                OnClientConnectedAsync?.Invoke(client);
            }

            await Task.Delay(cMillisPerCheck);
        }
    }
}
