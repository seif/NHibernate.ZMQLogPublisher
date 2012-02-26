namespace NHibernate.ZMQLogPublisher
{
    using System;

    using ZMQ;

    public class ZmqLoggerFactory : ILoggerFactory
    {
        private readonly SocketManager socketManager;

        public ZmqLoggerFactory(SocketManager socketManager)
        {
            this.socketManager = socketManager;
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new ZmqLogger(keyName, this.socketManager);
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }
    }
}