using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestPublisher
{
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using NHibernate.ZMELogPublisher.Tests.TestData;
    using NHibernate.ZMQLogPublisher;

    class Program
    {
        static void Main(string[] args)
        {
            var config = new Configuration();
            config.Configure("nh.sqlserver.config");
            config.SessionFactoryName("Test session factory");
            config.AddAssembly(typeof(Dog).Assembly);

            new SchemaExport(config).Create(true, true);


            Publisher.Start();
            using(var sessionFactory = config.BuildSessionFactory())
            {
                using (var session = sessionFactory.OpenSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        session.Save(new Lizard() { SerialNumber = "11111", Description = "Saving lizard to get a new logger requested" });
                        Publisher.Shutdown();

                        Publisher.Start();
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
                Console.ReadLine();
            }
            Publisher.Shutdown();
        }
    }
}
