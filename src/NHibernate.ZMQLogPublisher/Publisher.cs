namespace NHibernate.ZMQLogPublisher
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    
    using ZMQ;

    public class Publisher
    {
        private static Publisher instance;

        private Configuration configuration;

        private Context context;

        private ManualResetEvent threadRunningEvent;

        private ManualResetEvent threadStoppedEvent;

        private bool stopping;

        private bool running;

        private Thread publisherThread;

        private ZmqLoggerFactory zmqLoggerFactory;


        public static Publisher Instance
        {
            get
            {
                return instance;
            }
        }

        public bool Running
        {
            get
            {
                return this.running;
            }
        }

        public Publisher(Configuration configuration)
            :this(configuration, new Context(1))
        {
        }

        public Publisher(Configuration configuration, Context context)
        {
            this.context = context;
            this.configuration = configuration;
            this.zmqLoggerFactory = new ZmqLoggerFactory(configuration.LoggersToPublish.ToArray());
            
            this.threadRunningEvent = new ManualResetEvent(false);
            this.threadStoppedEvent = new ManualResetEvent(false);
        }

        public static void Start()
        {
            Start(new Publisher(Configuration.LoadDefault()));
        }

        public static void Start(Publisher configuredInstance)
        {   
            instance = configuredInstance;
            instance.StartPublisherThread();
            instance.AssociateWithNHibernate();
        }

        public static void Stop()
        {
            instance.Shutdown();
        }

        public void Shutdown()
        {
            this.stopping = true;
            this.running = false;
            
            this.threadStoppedEvent.WaitOne();
            this.stopping = false;
        }

        public void StartPublisherThread()
        {
            this.publisherThread = new Thread(() => this.ListenAndPublishLogMessages());
            this.publisherThread.Start();

            this.threadRunningEvent.WaitOne(5000);
            this.running = true;
        }

        private void AssociateWithNHibernate()
        {
            this.zmqLoggerFactory.Initialize(this.context);

            LoggerProvider.SetLoggersFactory(this.zmqLoggerFactory);
        }


        private void ListenAndPublishLogMessages()
        {
            using (Socket publisher = this.context.Socket(SocketType.PUB),
                loggersSink = this.context.Socket(SocketType.PULL),
                syncSocket = this.context.Socket(SocketType.REP))
            {
                this.ConfigureSocket(publisher, this.configuration.PublisherSocketConfig);
                this.ConfigureSocket(syncSocket, this.configuration.SyncSocketConfig);
                
                this.StartLoggersSink(loggersSink);
                loggersSink.PollInHandler += (socket, revents) => publisher.Send(socket.Recv());

                this.threadRunningEvent.Set();

                byte[] syncMessage = null;
                // keep waiting for syncMessage before starting to publish
                // unless we stop before we recieve the sync message
                while (!this.stopping && syncMessage == null)
                {
                    syncMessage = syncSocket.Recv(SendRecvOpt.NOBLOCK);
                }

                // send sync confirmation if we recieved a sync request
                if(syncMessage != null)
                {
                    syncSocket.Send(string.Empty, Encoding.Unicode);
                }

                while (!this.stopping)
                {
                    Context.Poller(new List<Socket> { loggersSink, publisher }, 2000);
                }
            }

            this.threadStoppedEvent.Set();
            this.zmqLoggerFactory.StopSockets();
        }

        private void StartLoggersSink(Socket loggers)
        {
            loggers.Linger = 0;
            loggers.Bind(Transport.INPROC, "loggers");
        }

        private void ConfigureSocket(Socket socket, SocketConfiguration socketConfig)
        {
            socket.Bind(socketConfig.Address);
            socket.HWM = socketConfig.HighWaterMark;
            socket.Linger = socketConfig.Linger;
        }
    }
}
