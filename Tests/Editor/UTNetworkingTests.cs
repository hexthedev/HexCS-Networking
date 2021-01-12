using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HexCS.Networking;

namespace HexCSTests.Networking
{
    public static class UTNetworkingTests
    {
        private static Lazy<IPAddress> _me = new Lazy<IPAddress>(() => UTIPHostEntry.Me.AddressList[1]);
        
        /// <summary>
        /// Time given to async network tasks to allow them to resolve before
        /// testing. May be a good idea to always provide a blocking and non-blocking
        /// version of the functions. 
        /// </summary>
        public const int cResolveTime = 50;

        /// <summary>
        /// Waits the task so that async network calls have time to resolve
        /// </summary>
        public static void ResolveTime() => Task.Delay(cResolveTime).Wait();

        /// <summary>
        /// Returns an IPEndpoint of local machine with port = 10000 + offset
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static IPEndPoint GetTestLocalEndpoint(int offset) => new IPEndPoint(_me.Value, 10000 + offset);
    }
}
