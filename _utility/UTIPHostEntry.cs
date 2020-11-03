using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HexCS.Networking
{
    /// <summary>
    /// General utilities for IPHostEntries
    /// </summary>
    public static class UTIPHostEntry
    {
        private static Lazy<IPHostEntry> _meCache 
            = new Lazy<IPHostEntry>(() => Dns.GetHostEntry(Dns.GetHostName()));

        /// <summary>
        /// Returns the host entry of the current computer. This is useful for finding the
        /// local ip address
        /// </summary>
        /// <returns></returns>
        public static IPHostEntry Me => _meCache.Value;
    }
}
