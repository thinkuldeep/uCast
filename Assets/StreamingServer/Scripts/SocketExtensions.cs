using System.Collections.Generic;
using System.Net.Sockets;

namespace StreamingServer
{
    internal static class SocketExtensions
    {
        public static IEnumerable<Socket> AcceptIncomingConnections(this Socket server)
        {
            while (true)
                yield return server.Accept();
        }
    }
}