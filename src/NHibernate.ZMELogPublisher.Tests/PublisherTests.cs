namespace NHibernate.ZMELogPublisher.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using NHibernate.ZMELogPublisher.Tests.TestData;
    using NHibernate.ZMQLogPublisher;

    using NUnit.Framework;

    using ZMQ;

    [TestFixture]
    public class PublisherTests
    {
        private ISessionFactory sessionFactory;

        IList<string> recievedMessages = new List<string>();

        private bool timeoutReached;

        private Timer timer;

        [TestFixtureSetUp]
        public void RunOnceBeforeAllTests()
        {
            this.timer = new Timer(x => this.timeoutReached = true, null, 5000, 5000);
            Publisher.Start();
            var config = new Configuration();
            config.Configure("nh.sqlite.config");
            config.SessionFactoryName("Test session factory");
            config.AddAssembly(this.GetType().Assembly);

            new SchemaExport(config).Create(true, true);

            sessionFactory = config.BuildSessionFactory();
        }

        [TestFixtureTearDown]
        public void RunAfterAllTests()
        {
            sessionFactory.Dispose();
            this.timer.Dispose();
            Publisher.Shutdown();
        }

        [Test]
        public void OpeningSessionPublishesEvent()
        {
            var task = new Task(StartSubscriber);

            task.Start(); // start subscriber to listen to messages
            
            using (var session = sessionFactory.OpenSession())
            {
                session.Save(
                    new Dog
                        {
                            BirthDate = DateTime.Now, 
                            BodyWeight = 13, 
                            Description = "Some dog", 
                            SerialNumber = "98765"
                        });
            }

            task.Wait(); // wait until subscriber finished

            Assert.That(recievedMessages.Count > 0, "No messages recieved.");
            Assert.That(recievedMessages.Any(m => m.Contains("opened session")), "Did not recieve session opened message.");
        }

        private void StartSubscriber()
        {
            this.recievedMessages.Clear();
            using (var context = new Context(1))
            {
                using (Socket subscriber = context.Socket(SocketType.SUB))
                {
                    subscriber.StringToIdentity("Test subscriber", Encoding.Unicode);
                    subscriber.Subscribe("", Encoding.Unicode);
                    subscriber.Connect("tcp://localhost:5555");
                    
                    string message = "";
                    
                    while (!(this.timeoutReached || message.Contains("closing")))
                    {
                        message = subscriber.Recv(Encoding.Unicode);
                        Console.WriteLine(message);
                        this.recievedMessages.Add(message);
                    }
                }
            }
        }
    }
}
