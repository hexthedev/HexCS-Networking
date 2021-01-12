using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HexCS.Core;

namespace HexCS.Networking
{
    /// <summary>
    /// Represents a TcpClient object that will attempt to connect to a server. The client can
    /// * Connect to a given Remote Endpoint
    /// * Disconnect from it's current endpoint
    /// </summary>
    public class SimpleTcpClient : ADisposableManager
    {
        private TcpClientConnection _connection;

        #region Protected API
        /// <inheritdoc />
        protected override string AccessAfterDisposalMessage => $"Disposed Error: Cannot access {nameof(SimpleTcpClient)} after disposal";

        /// <inheritdoc />
        protected override void SetDisposablesToNull()
        {
            _connection = null;
        }
        #endregion

        #region Public API
        /// <summary>
        /// The endpoint this client represents, so likely the IPAddress on the local machine
        /// </summary>
        public IPEndPoint LocalEndpoint { get; private set; } = null;

        /// <summary>
        /// The endpoint of the remote connection, only valid if IsConnected = true
        /// </summary>
        public IPEndPoint RemoteConnection { get; private set; } = null;

        /// <summary>
        /// Is there an active connection associated with this client
        /// </summary>
        public bool IsConnected => _connection != null && !_connection.IsDisposed;

        /// <summary>
        /// Invoked when an attempt to connect fails
        /// </summary>
        public event Action<SDisconnectArgs<TcpClient>> OnConnectionFailedAsync;

        /// <summary>
        /// Invoked when a previously active connection is disconnected
        /// </summary>
        public event Action OnDisconnectAsync;

        /// <summary>
        /// Invoked when a message is received
        /// </summary>
        public event Action<SPacket<TcpClient>> OnMessageRecievedAsync;

        /// <summary>
        /// Construct to the SimpleTcpClient with a local endpoint
        /// </summary>
        /// <param name="localEndpoint"></param>
        public SimpleTcpClient(IPEndPoint localEndpoint) : base()
        {
            LocalEndpoint = localEndpoint;
        }

        /// <summary>
        /// Disconnects and disposes the current connection, then attempts to connect
        /// to the new remote
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        public async Task<bool> Connect(IPEndPoint remote)
        {
            if (IsConnected && RemoteConnection == remote) return true;
            Disconnect();

            // make a new client and attempt connection
            TcpClient cli = new TcpClient(LocalEndpoint);
            SConnectResult<TcpClient> res = await cli.SimpleConnect(remote);

            // check if disconnected
            if (res.IsDisconnected)
            {
                OnConnectionFailedAsync?.Invoke(
                    new SDisconnectArgs<TcpClient>()
                    {
                        Connection = cli,
                        ReadableReason = res.DisconnectArgs.ReadableReason,
                        Exception = res.DisconnectArgs.Exception
                    }
                );

                return false;
            }

            // It's possible a conneciton isn't established even if no exception occurs.
            // In this case, the reaosn is currently unknown
            if (!res.IsConnected)
            {
                OnConnectionFailedAsync?.Invoke(
                    new SDisconnectArgs<TcpClient>() { Connection = cli, ReadableReason = "Unknown Reason" }
                );

                return false;
            }

            // if connected successfully, save this client and start receiving/sending
            _connection = new TcpClientConnection(cli);

            _connection.OnDisconnectAsync += (c) => OnDisconnectAsync?.Invoke();
            _connection.OnReceiveAsync += (s) => OnMessageRecievedAsync?.Invoke(s);

            RemoteConnection = remote;
            return true;
        }

        /// <summary>
        /// Disconnects and disposes the current connection
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                _connection.Disconnect();
                _connection = null;
            }
        }

        /// <summary>
        /// Sends byte[] data over the connection. This may cause the connection to disconnect if
        /// sending fails
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            if (IsConnected) _connection.Send(data);
        }
        #endregion
    }
}
