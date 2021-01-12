using NUnit.Framework;
using System.Net;
using HexCS.Networking;
using static HexCSTests.Networking.UTNetworkingTests;

namespace HexCSTests.Networking
{
    [TestFixture]
    public class UdpSocketTests
    {
        [Test]
        public void Works()
        {
            // Arrange
            IPEndPoint udp0 = GetTestLocalEndpoint(0);
            IPEndPoint udp1 = GetTestLocalEndpoint(1);

            // Act
            UdpSocket s0 = new UdpSocket(udp0);
            UdpSocket s1 = new UdpSocket(udp1);

            bool s0Recieve = false;
            s0.OnReceiveAsync += p => s0Recieve = true;
            s1.Send(new byte[] { 0 }, udp0);

            bool s0ConnectedTos1 = false;
            s0.RegisterConnection(udp1);
            s1.OnReceiveAsync += p => s0ConnectedTos1 = true;
            s0.SendToAll(new byte[] { 0 });

            ResolveTime();
            
            s0.Dispose();
            s1.Dispose();

            // Assert
            Assert.That(s0Recieve && s0ConnectedTos1);
        }
    }
}