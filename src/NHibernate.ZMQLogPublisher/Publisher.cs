﻿namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    using ZMQ;

    public class Publisher
    {
        private static Context context;

        private static bool running;

        public static void Start()
        {
            context = new Context(1);
            
            new Task(ListenAndPublishLogMessages).Start();

            while(!running)
            {
            }

            LoggerProvider.SetLoggersFactory(new ZmqLoggerFactory(context));
        }

        public static void Shutdown()
        {
            running = false;
            LoggerProvider.SetLoggersFactory(new NoLoggingLoggerFactory());
        }

        private static void ListenAndPublishLogMessages()
        {
            using (Socket publisher = context.Socket(SocketType.PUB), loggers = context.Socket(SocketType.PULL))
            {
                publisher.Bind("tcp://*:5555");
                publisher.Linger = 0;

                loggers.Bind("inproc://loggers");
                loggers.Linger = 0;
                running = true;
                
                while (running)
                {
                    var logMessage = loggers.Recv(Encoding.Unicode, timeout: 1000);
                    if(logMessage != null)
                    publisher.Send(logMessage, Encoding.Unicode);
                }
            }

            context.Dispose();
        }
    }
}
