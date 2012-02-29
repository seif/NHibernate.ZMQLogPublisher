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

    using Exception = System.Exception;

    [TestFixture]
    public class PublisherTests
    {
        private ISessionFactory sessionFactory;

        IList<string> recievedMessages = new List<string>();

        private bool stopSubscriber;

        private Timer timer;

        private Task subscriberTask;

        [TestFixtureSetUp]
        public void RunOnceBeforeAllTests()
        {
            var config = new Configuration();
            config.Configure("nh.sqlite.config");
            config.SessionFactoryName("Test session factory");
            config.AddAssembly(this.GetType().Assembly);

            new SchemaExport(config).Create(true, true);

            sessionFactory = config.BuildSessionFactory();

            timer = new Timer(x => stopSubscriber = true, null, 5000, Timeout.Infinite);
        }

        [TestFixtureTearDown]
        public void RunAfterAllTests()
        {
            sessionFactory.Dispose();
        }

        [SetUp]
        public void RunBeforeEachTest()
        {
            timer.Change(5000, Timeout.Infinite);
        }
        
        [Test]
        [Ignore]
        public void OpeningMultipleSessionsInDifferentThreads()
        {
            timer.Change(300000, Timeout.Infinite);
            Publisher.Start();

            int expectedSessions = 100;
            subscriberTask = new Task(() => this.StartSubscriber(expectedSessions));
            subscriberTask.Start(); // start subscriber to listen to messages

            Task[] tasks = new Task[expectedSessions];
            for (int i = 0; i < expectedSessions; i++)
            {
                tasks[i] = new Task(this.OpenSessionAndSaveDogWithChild);
                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            Publisher.Shutdown();
            this.subscriberTask.Wait(); // wait until subscriber finished

            Assert.AreEqual(expectedSessions, this.recievedMessages.Count(m => m.Contains("opened session")), "Did not recieve session opened message for all sessions.");
        }

        [Test]
        public void OpeningSessionPublishesEvent()
        {
            Publisher.Start();

            subscriberTask = new Task(() => this.StartSubscriber(1));
            subscriberTask.Start(); // start subscriber to listen to messages

            this.OpenSessionAndSaveDogWithChild();
            Publisher.Shutdown();
            this.subscriberTask.Wait(); // wait until subscriber finished

            Assert.AreEqual(1, this.recievedMessages.Count(m => m.Contains("opened session")), "Did not recieve session opened message for all sessions.");
        }

        [Test]
        public void UsingNHibernateAfterShutingPublisherDownShouldNotThrow()
        {
            AssertNoExceptionThrown(() =>
            {   
                Publisher.Start();
                OpenSessionAndSaveDogWithChild();
                Publisher.Shutdown();

                OpenSessionAndSaveDogWithChild();
                OpenSessionAndSave(
                    new Lizard() { SerialNumber = "11111", Description = "Saving lizard to get a new logger requested" });
            });
        }

        private void AssertNoExceptionThrown(Action action)
        {
            Exception exceptionThrown = null;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exceptionThrown = ex;
            }

            Assert.IsNull(exceptionThrown);
        }

        private void OpenSessionAndSaveDogWithChild()
        {
            var dog = new Dog
            {
                BirthDate = DateTime.Now.AddYears(-1),
                BodyWeight = 10,
                Description = "Some dog",
                SerialNumber = "98765"
            };
            var puppy = new Dog
            {
                BirthDate = DateTime.Now,
                BodyWeight = 2,
                Description = "Some pup",
                SerialNumber = "9875"
            };
            dog.Children = new List<Animal>();
            dog.Children.Add(puppy);
            puppy.Mother = dog;
            OpenSessionAndSave(dog);
        }

        private void OpenSessionAndSave(Animal animal)
        {
            using (var session = this.sessionFactory.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Save(animal);

                    tx.Commit();
                }
                var animals = session.QueryOver<Animal>().List();
                var dogs = session.QueryOver<Dog>().List();
                var lizards = session.QueryOver<Lizard>().List();
            }
        }

        private void StartSubscriber(int expectedSessions)
        {
            this.recievedMessages.Clear();

            using(var context = new Context(1))
            using (Socket subscriber = context.Socket(SocketType.SUB))
            {
                subscriber.Subscribe("", Encoding.Unicode);
                subscriber.Linger = 0;
                subscriber.Connect("tcp://localhost:68748");

                string message = "";

                while (!(this.stopSubscriber || recievedMessages.Count(m => m.Contains("opened session")) == expectedSessions))
                {
                    message = subscriber.Recv(Encoding.Unicode, 10);
                    if (message != null)
                    {
                        this.recievedMessages.Add(message);
                    }
                }
            }
        }
    }
}
