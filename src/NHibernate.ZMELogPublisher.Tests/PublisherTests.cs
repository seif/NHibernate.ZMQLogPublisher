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

        private bool stopSubscriber;

        private Timer timer;

        private Task subscriberTask;

        private static int numberOfThreads = 10;

        [TestFixtureSetUp]
        public void RunOnceBeforeAllTests()
        {

            this.subscriberTask = new Task(this.StartSubscriber);
            this.subscriberTask.Start(); // start subscriber to listen to messages

            this.timer = new Timer(x => this.stopSubscriber = true, null, 60000, Timeout.Infinite);

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
            timer.Dispose();
            Publisher.Shutdown();
        }

        [Test]
        public void OpeningSessionPublishesEvent()
        {
            Task[] tasks = new Task[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks[i] = this.OpenSessionAndSaveAnObject(i);
            }

            Task.WaitAll(tasks);
            
            this.stopSubscriber = true;

            this.subscriberTask.Wait(); // wait until subscriber finished

            Assert.AreEqual(recievedMessages.Count(m => m.Contains("opened session")), numberOfThreads, "Did not recieve session opened message for all sessions.");
        }

        private Task OpenSessionAndSaveAnObject(int i)
        {
            return Task.Factory.StartNew(() =>
                {
                    using (var session = this.sessionFactory.OpenSession())
                    {
                        session.Save(
                            new Dog { BirthDate = DateTime.Now, BodyWeight = i, Description = "Some dog" + i, SerialNumber = "98765" });
                    }
                });
        }

        private void StartSubscriber()
        {
            this.recievedMessages.Clear();

            using (Socket subscriber = new Socket(SocketType.SUB))
            {
                subscriber.Subscribe("", Encoding.Unicode);
                subscriber.Connect("tcp://localhost:5555");

                string message = "";

                while (!(this.stopSubscriber || recievedMessages.Count(m => m.Contains("opened session")) == numberOfThreads))
                {
                    message = subscriber.Recv(Encoding.Unicode);
                    Console.WriteLine(message);
                    this.recievedMessages.Add(message);
                }
            }
        }
    }
}
