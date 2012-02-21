namespace NHibernate.ZMQLogPublisher
{
    using System;

    using ZMQ;

    public class ZmqLoggerFactory : ILoggerFactory
    {
        private Context context;

        public ZmqLoggerFactory(Context context)
        {
            this.context = context;
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new ZmqLogger(keyName, context);
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return this.LoggerFor(type.FullName);
        }
    }
}