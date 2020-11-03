using System;
using System.Collections.Concurrent;
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
    /// Represents a connection to a UdpServer. Since UdpServers do not actually 
    /// create a connection, this class can be thought of as a UdpClient auto receiver
    /// and sender when the UdpClient is supposed to send and receive to a single server. 
    /// 
    /// In the case where the UdpClient needs to send and receive to and from multiple users
    /// this clas isn't useful. This connection discards all packets received from an conneciton
    /// other than the specified remote host, and can only send packets to the remote host in question
    /// </summary>
    public class UdpSocket: ADisposableManager
    {
        private const int cMillisPerCheck = 10; // millis per check

        private RecurrentAsyncAction _receiver;

        private ConcurrentQueue<UdpReceiveResult> _sendQueue = new ConcurrentQueue<UdpReceiveResult>();
        private RecurrentAsyncAction _sender;

        private List<IPEndPoint> _connections = new List<IPEndPoint>();

        #region Protected API
        /// <inheritdoc />
        protected override string AccessAfterDisposalMessage => $"Disposed Error: Cannot access {nameof(UdpSocket)} after disposal";

        /// <inheritdoc />
        protected override void SetDisposablesToNull()
        {
            Client = null;
            _sender = null;
            _receiver = null;
        }
        #endregion


        #region Public API
        /// <summary>
        /// The client used by the connection to send and receive data
        /// </summary>
        public UdpClient Client { get; private set; }

        /// <summary>
        /// Invoked whenever a packet is received. This may be called from another thread
        /// and is guarenteed to be called async
        /// </summary>
        public event Action<UdpReceiveResult> OnReceiveAsync;

        /// <summary>
        /// Constructor
        /// </summary>
        public UdpSocket(IPEndPoint localEndpoint)
        {
            Client = new UdpClient(localEndpoint);
            RegisterInteralDisposable(Client);

            _sender = new RecurrentAsyncAction(HandleSend);
            RegisterInteralDisposable(_sender);

            _receiver = new RecurrentAsyncAction(HandleReceive);
            RegisterInteralDisposable(_receiver);
        }

        /// <summary>
        /// Attempts a send to an IPEndpoint
        /// </summary>
        /// <param name="data"></param>
        /// <param name="remote"></param>
        public void Send(byte[] data, IPEndPoint remote) => _sendQueue.Enqueue(new UdpReceiveResult(data, remote));

        /// <summary>
        /// Send to all connections
        /// </summary>
        /// <param name="data"></param>
        public void SendToAll(byte[] data)
        {
            foreach(IPEndPoint con in _connections)
            {
                Send(data, con);
            }
        }

        /// <summary>
        /// Determines whether the IpEndpoint is already a connection
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public bool IsConnection(IPEndPoint endpoint) 
            => _connections.QueryContains(i => i.Address.Equals(endpoint.Address) && i.Port == endpoint.Port);

        /// <summary>
        /// Register an endpoint as a connection
        /// </summary>
        /// <param name="endpoint"></param>
        public void RegisterConnection(IPEndPoint endpoint)
        {
            if (!IsConnection(endpoint))
            {
                _connections.Add(endpoint);
            }
        }
        #endregion

        private async Task HandleSend()
        {
            while(_sendQueue.Count > 0 && _sendQueue.TryDequeue(out UdpReceiveResult res))
            {
                await Client.SendAsync(res.Buffer, res.Buffer.Length, res.RemoteEndPoint);
            }
            
            await Task.Delay(cMillisPerCheck);
        }

        private async Task HandleReceive()
        {
            while (Client.Available > 0)
            {
                UdpReceiveResult res = await Client.ReceiveAsync();

                if (res != default(UdpReceiveResult))
                {
                    OnReceiveAsync?.Invoke(res);
                }
            }

            await Task.Delay(cMillisPerCheck);
        }
    }
}