namespace NHibernate.ZMQLogPublisher
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    
    using ZMQ;

    public class Publisher
    {
        private static Context context = new Context(1);

        private static volatile bool stopping;

        private static volatile bool running;

        private static Thread publisherThread;

        private static ZmqLoggerFactory zmqLoggerFactory;

        public static bool Running
        {
            get
            {
                return running;
            }
        }

        public static void Start()
        {
            Start(68748);
        }

        public static void Start(int port)
        {
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
            
            while (Running)
            {
            }

            stopping = false;
        }

        private static void ListenAndPublishLogMessages(int port)
        {
            using (Socket publisher = context.Socket(SocketType.PUB),
                loggers = context.Socket(SocketType.PULL),
                syncService = context.Socket(SocketType.REP))
            {
                publisher.Bind(string.Format("tcp://*:{0}", port));
                publisher.HWM = 100000;
                publisher.Linger = 0;

                loggers.Bind("inproc://loggers");
                loggers.Linger = 0;

                syncService.Bind("tcp://*:68747");

                loggers.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());

                running = true;

                byte[] syncMessage = null;
                // keep waiting for syncMessage before starting to publish
                // unless we stop before we recieve the sync message
                while (!stopping && syncMessage == null)
                {
                    syncMessage = syncService.Recv(SendRecvOpt.NOBLOCK);
                }

                // send sync confirmation if we recieved a sync request
                if(syncMessage != null)
                {
                    syncService.Send("", Encoding.Unicode);
                }

                while (!stopping)
                {
                    Context.Poller(new List<Socket> { loggers, publisher }, 1000);
                }
            }

            running = false;
            zmqLoggerFactory.DisposeSockets();
        }
    }
}
