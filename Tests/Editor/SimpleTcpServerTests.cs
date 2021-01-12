using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HexCS.Networking;

using static HexCSTests.Networking.UTNetworkingTests;

namespace HexCSTests.Networking
{
    [TestFixture]
    public class SimpleTcpServerTests
    {
        [Test]
        public void Works()
        {
            // Arrange
            IPEndPoint serverIp = GetTestLocalEndpoint(0);
            IPEndPoint clientIp = GetTestLocalEndpoint(1);

            using (SimpleTcpServer server = new SimpleTcpServer(serverIp))
            {
                TcpClient cli = new TcpClient(clientIp);

                bool clientConnect = false;
                bool clientDisconnects = false;
                bool clientMessageReceived = false;

                server.OnClientConnectedAsync += c => clientConnect = true;
                server.OnClientDisconnectAsync += c => clientDisconnects = true;
                server.OnMessageRecievedAsync += c => clientMessageReceived = true;

                //Act
                cli.Connect(serverIp);
                cli.Client.Send(new byte[] { 1, 2, 3 });

                ResolveTime();

                cli.Close();

                server.SendToAll(new byte[] { 1, 2, 3 });
                server.SendToAll(new byte[] { 1, 2, 3 });

                ResolveTime();

                // Assert
                Assert.That(clientConnect == true && clientDisconnects == true && clientMessageReceived == true);
            }
        }
    }
}