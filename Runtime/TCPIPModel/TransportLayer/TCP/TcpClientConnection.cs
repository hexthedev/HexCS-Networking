using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using HexCS.Core;
using HexCS.Parallel;

namespace HexCS.Networking
{
    /// <summary>
    /// Encapsulates a connection using a TcpClient and manges the state of that
    /// connection. If disconnected for any reason, the class disposes itself and should
    /// not be used again
    /// * Attempts sending at 100Hz
    /// * Attempts receiveing at 100Hz
    /// * If disconnected, invokes event and disposes self
    /// * Check IsDisposed to check if connected or disconnected
    /// </summary>
    public class TcpClientConnection : ADisposableManager, IConnection<TcpClient>
    {
        private const int cMillisPerCheck = 10; // millis per check

        private RecurrentAsyncAction _receiver;

        private ConcurrentQueue<byte[]> _sendQueue = new ConcurrentQueue<byte[]>();
        private RecurrentAsyncAction _sender;


        #region Protected API
        /// <inheritdoc />
        protected override string AccessAfterDisposalMessage => $"Disposed Error: Cannot access {nameof(TcpClientConnection)} after disposal";

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
        public TcpClient Client { get; private set; }

        /// <summary>
        /// Invoked when this conneciton is disconnected. If a connection is disconnected, it
        /// a new connect needs to be created. This connection is dead and isposed
        /// </summary>
        public event Action<SDisconnectArgs<TcpClient>> OnDisconnectAsync;

        /// <summary>
        /// Invoked when a packet is received
        /// </summary>
        public event Action<SPacket<TcpClient>> OnReceiveAsync;

        /// <summary>
        /// Construct a connection with a TcpClient. The client must be connected 
        /// to the target endpoint already, otherwise the connection will disconnect on 
        /// first send attempt;
        /// </summary>
        /// <param name="client"></param>
        public TcpClientConnection(TcpClient client) : base()
        {
            Client = client;
            RegisterInteralDisposable(Client);

            _receiver = new RecurrentAsyncAction(AttemptReceive);
            RegisterInteralDisposable(_receiver);

            _sender = new RecurrentAsyncAction(AttemptSend);
            RegisterInteralDisposable(_sender);
        }

        /// <summary>
        /// Enqueue a message for sending over the network. Sending dose not occur
        /// instantly, rather the data is enqueued and another thread periodically sends
        /// the queued data. The rate of this class is 100 Hz
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            ThrowErrorIfDisposed();
            _sendQueue.Enqueue(data);
        }

        /// <summary>
        /// Disconnects this connection, by disposing the TcpClientConnection and the TcpClient
        /// it manages. Once disconnected, the TcpClientCOnnection is no longer useful
        /// and another conneciton needs to be created
        /// </summary>
        public void Disconnect()
        {
            ThrowErrorIfDisposed();
            HandleDisconnect(new SDisconnectArgs<TcpClient>() { Connection = Client, ReadableReason = "Disconnect called by application" });
        }
        #endregion

        #region Sending
        /// <summary>
        /// Attempts to send all messages in the send queue
        /// </summary>
        /// <returns></returns>
        private async Task AttemptSend()
        {
            while (_sendQueue.Count > 0)
            {
                if (_sendQueue.TryDequeue(out byte[] res)) await HandleSendAndDisconnect(res);
            }

            await Task.Delay(cMillisPerCheck);
        }

        /// <summary>
        /// Attempts to send a message to a client. Returns true if the client is still connected
        /// after the attempted send operation
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="message"></param>
        /// <returns>true if the client is still connected after the attempted send operation</returns>
        private async Task HandleSendAndDisconnect(byte[] message)
        {
            if (Client == null) return;
            SSendResult<TcpClient> res = await Client.SimpleSend(message);
            if (res.IsDisconnected) HandleDisconnect(res.DisconnectArgs);
        }
        #endregion

        #region Receiving
        /// <summary>
        /// Attempts to send all messages in the send queue
        /// </summary>
        /// <returns></returns>
        private async Task AttemptReceive()
        {
            await HandleReceiveAndDisconnect();
            await Task.Delay(cMillisPerCheck);
        }

        /// <summary>
        /// Attempts to receive a message from a client. Returns true if the client is still connected
        /// after the attempted receive
        /// </summary>
        private async Task HandleReceiveAndDisconnect()
        {
            if (Client == null) return;
            SReceiveResult<TcpClient> res = await Client.SimpleReceive();

            if (res.IsDisconnected) HandleDisconnect(res.DisconnectArgs);
            else if (res.Data != null) OnReceiveAsync?.Invoke(new SPacket<TcpClient>() { Sender = Client, Data = res.Data });
        }
        #endregion

        private void HandleDisconnect(SDisconnectArgs<TcpClient> args)
        {
            Dispose();
            OnDisconnectAsync?.Invoke(args);
        }
    }
}
