using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestPublisher
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using NHibernate.ZMELogPublisher.Tests.TestData;
    using NHibernate.ZMQLogPublisher;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started publisher and inserting data.");
            Publisher.Start();
    
            var config = new Configuration();
            config.Configure("nh.sqlserver.config");
            config.SessionFactoryName("Test session factory");
            config.AddAssembly(typeof(Dog).Assembly);

            new SchemaUpdate(config).Execute(false, true);
            
            using(var sessionFactory = config.BuildSessionFactory())
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                InsertData(sessionFactory);
                Console.WriteLine("Inserting data  with logging took: {0}", sw.Elapsed);

                sw.Restart();
                Publisher.Shutdown();
                Console.WriteLine("Publisher shutdown complete in {0}", sw.Elapsed);

                Console.WriteLine("inserting data with publisher shutdown");
                sw.Restart();
                InsertData(sessionFactory);
                Console.WriteLine("Inserting data  without logging took: {0}", sw.Elapsed);
            }
            Console.ReadLine();
        }

        private static void InsertData(ISessionFactory sessionFactory)
        {
            Task[] tasks = new Task[100];
            for (int i = 0; i < 100; i++)
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
                        });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
        }
    }
}
