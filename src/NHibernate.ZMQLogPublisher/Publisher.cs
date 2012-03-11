namespace NHibernate.ZMQLogPublisher
{
    using System.Text;
    using System.Threading;
    
    using ZMQ;

    public class Publisher
    {
        private static Context context;

        private static bool stopping;

        private static Thread publisherThread;

        private static ZmqLoggerFactory zmqLoggerFactory;

        public static bool Running { get; set; }

        public static void Start()
        {
            Start(68748);
        }

        public static void Start(int port)
        {
            context = new Context(1);

            publisherThread = new Thread(() => ListenAndPublishLogMessages(port));
            publisherThread.Start();

            while (!Running)
            {
            }

            if (zmqLoggerFactory == null)
            {
                zmqLoggerFactory = new ZmqLoggerFactory();
            }

            zmqLoggerFactory.Initialize(context);

            LoggerProvider.SetLoggersFactory(zmqLoggerFactory);
        }

        public static void Shutdown()
        {
            stopping = true;
            LoggerProvider.SetLoggersFactory(new NoLoggingLoggerFactory());

            while (Running)
            {
            }

            stopping = false;
        }

        private static void ListenAndPublishLogMessages(int port)
        {
            using (Socket publisher = context.Socket(SocketType.PUB),
                loggers = context.Socket(SocketType.PULL))
            {
                publisher.Bind(string.Format("tcp://*:{0}", port));
                publisher.Linger = 0;

                loggers.Bind("inproc://loggers");
                loggers.Linger = 0;
                Running = true;
                
                while (Running && !stopping)
                {
                    var logMessage = loggers.Recv(Encoding.Unicode, SendRecvOpt.NOBLOCK);
                    if (logMessage != null)
                    {
                        publisher.Send(logMessage, Encoding.Unicode);
                    }
                }
            }

            Running = false;
            zmqLoggerFactory.DisposeSockets();
            context.Dispose();   
        }
    }
}
