namespace NHibernate.ZMQLogPublisher
{
    using System.Collections.Generic;

    public class Configuration
    {
        public SocketConfiguration SyncSocketConfig { get; set; }

        public SocketConfiguration PublisherSocketConfig { get; set; }

        public List<string> LoggersToPublish { get; set; }
        
        public static Configuration LoadDefault()
        {
            var config = new Configuration();
            config.SyncSocketConfig = new SocketConfiguration { Address = "tcp://*:68747" };
            config.PublisherSocketConfig = new SocketConfiguration { Address = "tcp://*:68748" };
            config.LoggersToPublish = new List<string>
            { 
                "NHibernate.SQL", "NHibernate.Impl.SessionImpl", "NHibernate.Transaction.AdoTransaction",
                "NHibernate.AdoNet.AbstractBatcher"
            };

            return config;
        }

        public Configuration AddLoggerKeyToPublish(string key)
        {
            if (!LoggersToPublish.Contains(key))
            {
                LoggersToPublish.Add(key);
            }

            return this;
        }

        public Configuration ConfigureSyncSocket(System.Action<SocketConfiguration> socketConfigAction)
        {
            socketConfigAction(this.SyncSocketConfig);

            return this;
        }

        public Configuration ConfigurePublisherSocket(System.Action<SocketConfiguration> socketConfigAction)
        {
            socketConfigAction(this.PublisherSocketConfig);

            return this;
        }
    }
}