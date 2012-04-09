namespace NHibernate.ZMQLogPublisher
{
    using ZMQ;

    public class SocketConfiguration
    {
        public string Address { get; set; }

        public ulong HighWaterMark { get; set; }

        public int Linger { get; set; }
    }
}