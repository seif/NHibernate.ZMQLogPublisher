
namespace TestPublisher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using NHibernate.ZMQLogPublisher;

    using TestPublisher.TestData;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to start publishing");
            Console.ReadLine();
            
            var config = new Configuration();
            config.Configure("nh.sqlserver.config");
            config.SessionFactoryName("Test session factory");
            config.AddAssembly(typeof(Dog).Assembly);

            new SchemaExport(config).Execute(false, false, true);
            new SchemaExport(config).Execute(false, true, false);
            
            using(var sessionFactory = config.BuildSessionFactory())
            {
                Stopwatch sw = new Stopwatch();
                Publisher.Start();
                sw.Start();
                InsertData(sessionFactory);
                TimeSpan elapsedWithLogging = sw.Elapsed;

                sw.Restart();
                Publisher.Shutdown();
                TimeSpan shutdownTime = sw.Elapsed;

                Console.WriteLine("Completed in: {0}", elapsedWithLogging);
                Console.WriteLine("Shutdown complete in {0}, press any key to exit", shutdownTime);
            }
            Console.ReadLine();
        }

        private static void InsertData(ISessionFactory sessionFactory)
        {
            Task[] tasks = new Task[500];
            for (int i = 0; i < 500; i++)
            {
                tasks[i] = new Task(
                    () =>
                        {
                            using (var session = sessionFactory.OpenSession())
                            {
                                using (var tx = session.BeginTransaction())
                                {
                                    session.Save(
                                        new Lizard()
                                        {
                                            SerialNumber = "11111",
                                            Description = "Saving lizard to get a new logger requested"
                                        });

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

                                    tx.Commit();
                                }
                            }
                            using (var session = sessionFactory.OpenSession())
                            {
                                using (var tx = session.BeginTransaction())
                                {
                                    session.CreateCriteria<Dog>().List();
                                    session.QueryOver<Animal>().Where(a => a.BodyWeight > 0);
                                    tx.Commit();
                                }
                            }
                        });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
        }
    }
}
