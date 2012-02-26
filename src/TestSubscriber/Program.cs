using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSubscriber
{
    using System.CodeDom.Compiler;
    using System.IO;

    using NHibernate.ZMQLogPublisher;

    using ServiceStack.Text;

    using ZMQ;

    class Program
    {

        static void Main(string[] args)
        {
            IDictionary<string, List<string>> MessagesPerLogger  = new Dictionary<string, List<string>>();

            Console.WriteLine("Subscriber started, press ENTER to start logging, while logging ESCAPE to stop");
            Console.ReadLine();

            using(Context context = new Context(1))
            using (Socket subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Connect("tcp://localhost:68748");
                subscriber.Subscribe("", Encoding.Unicode);

                string message = "";

                MessagesPerLogger.Add("ALL MESSAGES AS RECIEVED", new List<string>());

                int sessionCount = 0;
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        message = subscriber.Recv(Encoding.Unicode, SendRecvOpt.NOBLOCK);
                        if (message != null)
                        {
                            var logDetails = JsonSerializer.DeserializeFromString<LogDetails>(message);

                            if (!MessagesPerLogger.ContainsKey(logDetails.LoggerKey))
                            {
                                MessagesPerLogger.Add(logDetails.LoggerKey, new List<string>());
                            }

                            if (logDetails.Message.Contains("opened session"))
                            {
                                sessionCount++;
                            }

                            MessagesPerLogger[logDetails.LoggerKey].Add(logDetails.Message);
                            MessagesPerLogger["ALL MESSAGES AS RECIEVED"].Add(
                                string.Format("{{{0}}} - {1}", logDetails.LoggerKey, logDetails.Message));
                            Console.WriteLine(logDetails.Message);
                        }
                    }
                }
                while (Console.ReadKey(true).Key != ConsoleKey.Escape);

                IndentedTextWriter writer = new IndentedTextWriter(new StringWriter());
                foreach (var logger in MessagesPerLogger.Keys)
                {
                    writer.WriteLine(logger);
                    writer.Indent++;
                    foreach (var logMessage in MessagesPerLogger[logger])
                    {
                        writer.WriteLine(logMessage);
                    }

                    writer.Indent--;
                    writer.WriteLine();
                }
                Console.WriteLine("{0} session opened messages were recieved.", sessionCount);
                File.WriteAllText("loggers.txt", writer.InnerWriter.ToString());
                Console.ReadLine();
            }
        }
    }
}
