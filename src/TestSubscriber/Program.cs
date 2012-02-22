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
        public static IDictionary<string, List<string>> MessagesPerLogger  = new Dictionary<string, List<string>>();



        static void Main(string[] args)
        {
            using(Context context = new Context(1))
            using (Socket subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Connect("tcp://localhost:5555");
                subscriber.Subscribe("", Encoding.Unicode);

                string message = "";

                while (!message.Contains("unbinding factory"))
                {
                    message = subscriber.Recv(Encoding.Unicode);
                    var logDetails = JsonSerializer.DeserializeFromString<LogDetails>(message);

                    if(!MessagesPerLogger.ContainsKey(logDetails.LoggerKey))
                    {
                        MessagesPerLogger.Add(logDetails.LoggerKey, new List<string>());
                    }

                    MessagesPerLogger[logDetails.LoggerKey].Add(logDetails.Message);
                    Console.WriteLine(logDetails.Message);
                }

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
                
                File.WriteAllText("loggers.txt", writer.InnerWriter.ToString());
            }
        }
    }
}
