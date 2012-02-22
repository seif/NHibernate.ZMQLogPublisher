namespace NHibernate.ZMQLogPublisher
{
    using System;

    public class LogDetails
    {   
        public string LoggerKey { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}