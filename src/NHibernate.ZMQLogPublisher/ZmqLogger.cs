namespace NHibernate.ZMQLogPublisher
{
    using System.Text;

    using ServiceStack.Text;

    using ZMQ;

    using Exception = System.Exception;

    public class ZmqLogger : IInternalLogger
    {
        private string keyName;

        private readonly bool onlyPublishExceptions;

        private Socket sender;

        private object socketLock = new object();

        public ZmqLogger(string keyName, bool onlyPublishExceptions)
        {
            this.keyName = keyName;
            this.onlyPublishExceptions = onlyPublishExceptions;
        }

        private void SendMessageToPublisher(string message)
        {
            if (!this.onlyPublishExceptions)
            {
                this.SendMessageToPublisher(message, null);
            }
        }

        private void SendMessageToPublisher(string message, Exception exception)
        {
            if (Publisher.Running)
            {
                var serializedLogDetails = this.GetSerializedLogDetails(message, exception);

                lock (socketLock)
                {
                    if (Publisher.Running)
                    {
                        this.sender.Send(serializedLogDetails, Encoding.Unicode);
                    }
                }
            }
        }

        private string GetSerializedLogDetails(string message, Exception exception)
        {
            var logDetails = new LogDetails { Exception = exception, Message = message, LoggerKey = this.keyName };

            string serializedLogDetails = logDetails.ToJson();
            return serializedLogDetails;
        }

        public void Error(object message)
        {
            this.SendMessageToPublisher(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            this.SendMessageToPublisher(message.ToString(), exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this.SendMessageToPublisher(string.Format(format, args));
        }

        public void Fatal(object message)
        {
            this.SendMessageToPublisher(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            this.SendMessageToPublisher(message.ToString(), exception);
        }

        public void Debug(object message)
        {
            this.SendMessageToPublisher(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            this.SendMessageToPublisher(message.ToString(), exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this.SendMessageToPublisher(string.Format(format, args));
        }

        public void Info(object message)
        {
            this.SendMessageToPublisher(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            this.SendMessageToPublisher(message.ToString(), exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this.SendMessageToPublisher(string.Format(format, args));
        }

        public void Warn(object message)
        {
            this.SendMessageToPublisher(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            this.SendMessageToPublisher(message.ToString(), exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.SendMessageToPublisher(string.Format(format, args));
        }

        public bool IsErrorEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsDebugEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return true;
            }
        }

        public void InitializeSocket(Socket socket)
        {
            this.sender = socket;

            this.sender.Linger = 0;

            this.sender.Connect("inproc://loggers");
        }

        public void DisposeSocket()
        {
            lock (socketLock)
            {
                sender.Dispose();
            }
        }
    }
}