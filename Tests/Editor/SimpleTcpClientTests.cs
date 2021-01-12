using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HexCS.Networking;

using static HexCSTests.Networking.UTNetworkingTests;

namespace HexCSTests.Networking
{
    [TestFixture]
    public class SimpleTcpClientTests
    {
        [Test]
        public void Works()
        {
            // Arrange
            IPEndPoint serverIp = GetTestLocalEndpoint(0);
            IPEndPoint clientIp = GetTestLocalEndpoint(1);

            TcpListener listener = new TcpListener(serverIp);
            listener.Start();

            using (SimpleTcpClient cli = new SimpleTcpClient(clientIp))
            {
                // Act
                bool clientConnect = true;
                clientConnect = cli.Connect(serverIp).Result;
                TcpClient server = listener.AcceptTcpClient();
                if (server == null) clientConnect = false;

                bool clientMessageReceived = false;
                cli.Send(new byte[] { 1 });
                ResolveTime();
                byte[] buffer = new byte[server.Client.Available];
                server.Client.Receive(buffer);
                clientMessageReceived = buffer[0] == 1;

                bool serverMessageReceived = false;
                cli.OnMessageRecievedAsync += (m) => serverMessageReceived = m.Data[0] == 2;
                server.Client.Send(new byte[] { 2 });
                ResolveTime();

                bool clientDisconnects = false;
                cli.OnDisconnectAsync += () => clientDisconnects = true;
                server.Client.Close();
                cli.Send(new byte[] { 0 }); // send data, this is how diconnection is detected
                cli.Send(new byte[] { 0 });
                ResolveTime();

                listener.Stop();

                // Assert
                Assert.That(clientConnect && clientMessageReceived && serverMessageReceived && clientDisconnects);
            }
        }
    }
}