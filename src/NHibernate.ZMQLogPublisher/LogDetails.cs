namespace NHibernate.ZMQLogPublisher
{
    using System;

    using ProtoBuf;

    [ProtoContract]
    public class LogDetails
    {
        [ProtoMember(1)]
        public string LoggerKey { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }

        [ProtoMember(3)]
        public string Exception { get; set; }

        [ProtoMember(4)]
        public Guid? SessionId { get; set; }
    }
}