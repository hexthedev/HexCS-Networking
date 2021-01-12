using NUnit.Framework;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using HexCS.Networking;

using static HexCSTests.Networking.UTNetworkingTests;

namespace HexCSTests.Networking
{
    [TestFixture]
    public class TcpClientConnectionTests
    {
        [Test]
        public void Works()
        {
            // Arrange
            IPEndPoint server = GetTestLocalEndpoint(0);
            IPEndPoint client = GetTestLocalEndpoint(1);

            TcpListener listener = new TcpListener(server);
            listener.Start();

            TcpClient cli = new TcpClient(client);
            cli.Connect(server);

            TcpClient serverCli = listener.AcceptTcpClient();

            // Act
            TcpClientConnection clientCon = new TcpClientConnection(cli);
            TcpClientConnection serverCon = new TcpClientConnection(serverCli);

            bool clientSendRec = false;
            bool serverSendRec = false;

            serverCon.OnReceiveAsync += d => clientSendRec = true;
            clientCon.OnReceiveAsync += d => serverSendRec = true;

            bool clientDisconnect = false;
            bool serverDisconnect = false;

            clientCon.OnDisconnectAsync += d => clientDisconnect = true;
            serverCon.OnDisconnectAsync += d => serverDisconnect = true;

            clientCon.Send(new byte[] { 1, 2, 3 });
            serverCon.Send(new byte[] { 3, 4, 5 });

            ResolveTime();

            clientCon.Disconnect(); // Disconnect the client
            serverCon.Send(new byte[] { 0 }); // it takes a few sends for disconnection to detect
            serverCon.Send(new byte[] { 0 }); // should auto disconnect serverCon

            ResolveTime();

            listener.Stop();
            cli.Close();
            serverCli.Close();

            // Assert
            Assert.That(clientSendRec == true && serverSendRec == true);
            Assert.That(clientDisconnect == true && serverDisconnect == true);
        }
    }
}