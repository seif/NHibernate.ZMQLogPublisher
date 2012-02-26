namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Collections.Concurrent;

    public class ZmqLoggerFactory : ILoggerFactory
    {
        private readonly SocketManager socketManager;

        private ConcurrentDictionary<string, IInternalLogger> loggers;

        public ZmqLoggerFactory(SocketManager socketManager)
        {
            this.socketManager = socketManager;
            this.loggers = new ConcurrentDictionary<string, IInternalLogger>();
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return this.loggers.GetOrAdd(keyName, (key) => new ZmqLogger(keyName, this.socketManager));
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }
    }
}