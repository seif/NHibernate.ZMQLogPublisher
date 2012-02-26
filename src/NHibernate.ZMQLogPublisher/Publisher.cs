namespace NHibernate.ZMQLogPublisher
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ZMQ;

    public class Publisher
    {
        private static SocketManager socketManager;

        private static bool running;

        private static bool stopping;

        private static Thread publisherThread;

        public static void Start()
        {
            Start(68748);
        }

        public static void Start(int port)
        {
            socketManager = new SocketManager();

            publisherThread = new Thread(() => ListenAndPublishLogMessages(port));
            publisherThread.Start();

            while(!running)
            {
            }

            LoggerProvider.SetLoggersFactory(new ZmqLoggerFactory(socketManager));
        }

        public static void Shutdown()
        {
            stopping = true;
            LoggerProvider.SetLoggersFactory(new NoLoggingLoggerFactory());

            while(running)
            {
            }
        }

        private static void ListenAndPublishLogMessages(int port)
        {
            using (Socket publisher = socketManager.Context.Socket(SocketType.PUB),
                loggers = socketManager.Context.Socket(SocketType.PULL))
            {
                publisher.Bind(string.Format("tcp://*:{0}", port));
                publisher.Linger = 0;

                loggers.Bind("inproc://loggers");
                loggers.Linger = 0;
                running = true;
                
                while (running && !stopping)
                {
                    var logMessage = loggers.Recv(Encoding.Unicode, SendRecvOpt.NOBLOCK);
                    if (logMessage != null)
                    {
                        publisher.Send(logMessage, Encoding.Unicode);
                    }
                }
            }

            running = false;
            socketManager.Terminate();
        }
    }
}
