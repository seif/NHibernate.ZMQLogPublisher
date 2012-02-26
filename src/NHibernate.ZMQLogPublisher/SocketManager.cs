namespace NHibernate.ZMQLogPublisher
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using ZMQ;

    public class SocketManager
    {
        private readonly Context context;

        private ConcurrentDictionary<string, Socket> sockets;

        private object synclock = new object();

        private bool terminated;

        public SocketManager()
        {
            this.context = new Context(1);
            this.sockets = new ConcurrentDictionary<string, Socket>();
        }

        public bool Terminated
        {
            get
            {
                return this.terminated;
            }
        }

        public Context Context
        {
            get
            {
                return this.context;
            }
        }

        public Socket CreateSocketForKey(string loggerKey, SocketType socketType)
        {
            return this.sockets.GetOrAdd(
                loggerKey,
                key =>
                {
                    var socket = this.Context.Socket(socketType);
                    this.sockets.TryAdd(loggerKey, socket);
                    return socket;
                });
        }

        public void Terminate()
        {
            this.terminated = true;

            foreach (var socket in sockets.Values)
            {
                lock (this.synclock)
                {
                    socket.Dispose();
                }
            }

            sockets.Clear();
            this.Context.Dispose();
        }
    }
}