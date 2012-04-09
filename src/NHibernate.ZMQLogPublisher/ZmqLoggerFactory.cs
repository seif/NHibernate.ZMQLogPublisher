namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    using ZMQ;

    public class ZmqLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ZmqLogger> loggers;

        private readonly string[] loggersToPublish;

        private Context context;

        public ZmqLoggerFactory(string[] loggersToPublish)
        {
            this.loggers = new ConcurrentDictionary<string, ZmqLogger>();
            this.loggersToPublish = loggersToPublish;
        }

        public void Initialize(Context ctx)
        {
            this.context = ctx;

            foreach (var logger in this.loggers.Values)
            {
                lock (logger)
                {
                    logger.InitializeWithSocket(this.context.Socket(SocketType.PUSH));
                }
            }
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return this.loggers.GetOrAdd(
                keyName,
                key =>
                {
                    var logger = new ZmqLogger(keyName, Array.IndexOf(loggersToPublish, keyName) == 0);

                    if (Publisher.Instance.Running)
                    {
                        logger.InitializeWithSocket(this.context.Socket(SocketType.PUSH));
                    }
                    return logger;
                });
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }

        public void StopSockets()
        {
            foreach (var kvp in this.loggers)
            {
                ZmqLogger logger = kvp.Value;
                logger.StopSocket();
            }
        }
    }
}