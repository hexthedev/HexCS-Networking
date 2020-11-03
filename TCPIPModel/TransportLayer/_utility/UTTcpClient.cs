using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HexCS.Networking
{
    /// <summary>
    /// Utilities for TcpClients
    /// </summary>
    public static class UTTcpClient
    {
        #region Public API
        /// <summary>
        /// Handles receiving a simple byte[] by creating new array as return value. Returns 
        /// null if no message is available
        /// Can throw all exceptions that a GetStream() throws on TcpClient.
        /// Also calls TcpClient.Available() which may cause exceptions
        /// </summary>
        /// <param name="cli">client</param>
        /// <returns>recieved message, null if no message received</returns>
        public static async Task<SReceiveResult<TcpClient>> SimpleReceive(this TcpClient cli)
        {
            byte[] message = null;
            async Task InternalReceive(TcpClient c) => message = await Receive(c);
            SDisconnectArgs<TcpClient> res = await DisconnectableOperation(cli, InternalReceive);

            return new SReceiveResult<TcpClient>()
            {
                Data = message,
                DisconnectArgs = res
            };
        }

        /// <summary>
        /// Handles sending a simple byte[] message through a TcpClient.
        /// Can throw all exceptions that a GetStream() throws on TcpCLient
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<SSendResult<TcpClient>> SimpleSend(this TcpClient cli, byte[] message)
        {
            async Task InternalSend(TcpClient c) => await Send(cli, message);
            SDisconnectArgs<TcpClient> res = await DisconnectableOperation(cli, InternalSend);
            return new SSendResult<TcpClient>() { DisconnectArgs = res };
        }



        /// <summary>
        /// A simlpe asyncronous connection method
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        public static async Task<SConnectResult<TcpClient>> SimpleConnect(this TcpClient cli, IPEndPoint remote)
        {
            async Task InternalConnect(TcpClient c) => await Connect(c, remote);
            SDisconnectArgs<TcpClient> res = await DisconnectableOperation(cli, InternalConnect);
            return new SConnectResult<TcpClient>() { DisconnectArgs = res, IsConnected = res.Equals(default(SDisconnectArgs<TcpClient>)) };
        }


        // T-MODIFY: Should use the AsyncAction<T> but need implementing in HexCS
        /// <summary>
        /// Handles an action that may throw an exception. If an exception is thrown, invokes a client disconnect event
        /// with the client, otherwise returns true. The calling function needs to handle the resolution of the disconnection
        /// </summary>
        /// <param name="action"></param>
        /// <param name="client"></param>
        /// <returns>returns default if action did not cause diconnection, otherwise returns SDisconnectArgs<TcpClient> which disconneciton info</returns>
        public static async Task<SDisconnectArgs<TcpClient>> DisconnectableOperation(this TcpClient client, Func<TcpClient, Task> action)
        {
            try
            {
                await action(client);
            }
            catch (SocketException e)
            {
                client.Dispose();
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "An error occurred when attempting to access the socket" };
            }
            catch (ObjectDisposedException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "The System.Net.Sockets.Socket has been closed" };
            }
            catch (InvalidOperationException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "The System.Net.Sockets.TcpClient is not connected to a remote host." };
            }
            catch (NotSupportedException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "Attempted operation not supported" };
            }
            catch (ArgumentOutOfRangeException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "The argument was out of range" };
            }
            catch (ArgumentNullException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "The argument was null" };
            }
            catch (ArgumentException e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = "The supplied argument was invalid" };
            }
            catch (Exception e)
            {
                return new SDisconnectArgs<TcpClient>() { Connection = client, Exception = e, ReadableReason = e.Message };
            }

            return default;
        }
        #endregion

        /// <summary>
        /// Handles receiving a simple byte[] by creating new array as return value. Returns 
        /// null if no message is available
        /// Can throw all exceptions that a GetStream() throws on TcpClient.
        /// Also calls TcpClient.Available() which may cause exceptions
        /// </summary>
        /// <param name="cli">client</param>
        /// <returns>recieved message, null if no message received</returns>
        private static async Task<byte[]> Receive(TcpClient cli)
        {
            int bytesAvailable = cli.Available;
            if (bytesAvailable == 0) return null;

            byte[] message = new byte[bytesAvailable];
            NetworkStream s = cli.GetStream();
            await s.ReadAsync(message, 0, bytesAvailable);
            return message;
        }

        /// <summary>
        /// Handles sending a simple byte[] message through a TcpClient.
        /// Can throw all exceptions that a GetStream() throws on TcpCLient
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task Send(TcpClient cli, byte[] message)
        {
            NetworkStream s = cli.GetStream();
            await s.WriteAsync(message, 0, message.Length);
        }



        /// <summary>
        /// A simlpe asyncronous connection method
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        private static async Task Connect(TcpClient cli, IPEndPoint remote)
        {
            await cli.ConnectAsync(remote.Address, remote.Port);
        }
    }
}
