namespace NHibernate.ZMQLogPublisher
{
    using ZMQ;

    public class Publisher
    {
        private static Socket publisher;

        private static Context context;

        public static void Start()
        {
            context = new Context(1);

            publisher = context.Socket(SocketType.PUB);

            publisher.Bind("tcp://*:5555");
            LoggerProvider.SetLoggersFactory(new ZmqLoggerFactory(publisher));
        }

        public static void Shutdown()
        {
            publisher.Dispose();
            context.Dispose();
        }
    }
}
