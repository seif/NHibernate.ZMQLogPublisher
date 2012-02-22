using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestSubscriber
{
    using ZMQ;

    class Program
    {
        static void Main(string[] args)
        {
            using(Context context = new Context(1))
            using (Socket subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Connect("tcp://localhost:5555");
                subscriber.Subscribe("", Encoding.Unicode);

                string message = "";

                while (true)
                {
                    message = subscriber.Recv(Encoding.Unicode);
                    Console.WriteLine(message);
                }
            }
        }
    }
}
