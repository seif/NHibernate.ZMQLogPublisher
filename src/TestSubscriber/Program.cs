using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSubscriber
{
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;

    using NHibernate.ZMQLogPublisher;

    using ServiceStack.Text;

    using ZMQ;

    class Program
    {

        static void Main(string[] args)
        {
            IDictionary<string, List<string>> MessagesPerLogger  = new Dictionary<string, List<string>>();
            IDictionary<string, List<string>> sessions = new Dictionary<string, List<string>>();

            Console.WriteLine("Subscriber started, press ENTER then start the publisher to start logging.");
            Console.WriteLine("While logging press ESCAPE to stop.");
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

                            if(logDetails.SessionId.HasValue && logDetails.LoggerKey == "NHibernate.SQL")
                            {
                                string sessionId = logDetails.SessionId.Value.ToString();

                                if (!sessions.ContainsKey(sessionId))
                                {
                                    sessions.Add(sessionId, new List<string>());
                                }

                                sessions[sessionId].Add(logDetails.Message);
                            }

                            MessagesPerLogger[logDetails.LoggerKey].Add(logDetails.Message);
                            MessagesPerLogger["ALL MESSAGES AS RECIEVED"].Add(
                                string.Format("{{{0}}} - {1}", logDetails.LoggerKey, logDetails.Message));
                            Console.WriteLine(logDetails.Message);
                        }
                    }
                }
                while (Console.ReadKey(true).Key != ConsoleKey.Escape);

                WriteTabbedTextToFile(MessagesPerLogger, "loggers.txt");
                WriteTabbedTextToFile(sessions, "sessions.txt");

                Console.WriteLine("{0} session opened messages were recieved.", sessionCount);
                Console.WriteLine("Press enter to exit");
                
                Console.ReadLine();
            }
        }

        private static void WriteTabbedTextToFile(IDictionary<string, List<string>> values, string fileToWriteTo)
        {
            IndentedTextWriter writer = new IndentedTextWriter(new StringWriter());
            foreach (var key in values.Keys)
            {
                writer.WriteLine(key);
                writer.Indent++;
                foreach (var message in values[key])
                {
                    writer.WriteLine(message);
                }

                writer.Indent--;
                writer.WriteLine();
            }

            File.WriteAllText(fileToWriteTo, writer.InnerWriter.ToString());
        }
    }
}
