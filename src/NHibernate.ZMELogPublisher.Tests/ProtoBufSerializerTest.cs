namespace NHibernate.ZMELogPublisher.Tests
{
    using System;

    using NHibernate.ZMQLogPublisher;

    using NUnit.Framework;

    [TestFixture]
    public class ProtoBufSerializerTests
    {
        [Test]
        public void CanSerializeAndDeserializeLogDetails()
         {
             LogDetails logDetails = new LogDetails
                 {
                     Exception = null,
                     LoggerKey = "NHibernate.SQL",
                     Message = "Some log message",
                     SessionId = Guid.NewGuid()
                 };

             var serialized = ProtoBufSerializer<LogDetails>.Serialize(logDetails);

             var deserialized = ProtoBufSerializer<LogDetails>.Deserialize(serialized);

             Assert.IsNotNull(deserialized, "Deserialized object was null");
         }
    }
}