namespace NHibernate.ZMQLogPublisher
{
    using System;

    using ZMQ;

    public class ZmqLoggerFactory : ILoggerFactory
    {
        private Socket publisher;

        private Context context;

        public ZmqLoggerFactory(Socket publisher)
        {
            this.publisher = publisher;
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new ZmqLogger(keyName, this.publisher);
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }

        public void Shutdown()
        {
            this.publisher.Dispose();
            this.context.Dispose();
        }
    }
}