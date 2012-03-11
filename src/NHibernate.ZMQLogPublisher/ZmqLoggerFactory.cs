namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Collections.Concurrent;

    using ZMQ;

    public class ZmqLoggerFactory : ILoggerFactory
    {   
        private readonly ConcurrentDictionary<string, ZmqLogger> loggers;

        private Context context;

        public ZmqLoggerFactory()
        {
            this.loggers = new ConcurrentDictionary<string, ZmqLogger>();
        }

        public void Initialize(Context ctx)
        {
            this.context = ctx;

            foreach (var logger in this.loggers.Values)
            {
                lock (logger)
                {
                    logger.InitializeSocket(this.context.Socket(SocketType.PUSH));
                }
            }
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return this.loggers.GetOrAdd(
                keyName,
                key =>
                    {
                        var logger = new ZmqLogger(keyName);
                    logger.InitializeSocket(this.context.Socket(SocketType.PUSH));
                    return logger;
                });
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }

        public void DisposeSockets()
        {
            foreach (var kvp in this.loggers)
            {
                ZmqLogger logger = kvp.Value;
                logger.DisposeSocket();
            }
        }
    }
}