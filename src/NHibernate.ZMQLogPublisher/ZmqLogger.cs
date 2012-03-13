namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Diagnostics;
    using System.Text;

    using ServiceStack.Text;

    using ZMQ;

    using Exception = System.Exception;

    public class ZmqLogger : IInternalLogger
    {
        private string keyName;
        
        private Socket sender;

        private object socketLock = new object();

        public ZmqLogger(string keyName)
        {
            this.keyName = keyName;
        }

        private void Publish(string message)
        {
            this.Publish(message, null);
        }

        private void Publish(string message, Exception exception)
        {
            if (Publisher.Running)
            {
                var logDetails = new LogDetails
                {
                    Exception = exception,
                    Message = message,
                    LoggerKey = this.keyName
                };

                string serializedLogDetails = logDetails.ToJson();

                lock (socketLock)
                {
                    if (Publisher.Running)
                    {
                        this.sender.Send(serializedLogDetails, Encoding.Unicode);
                    }
                }
            }
        }

        public void Error(object message)
        {
            this.Publish(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            this.Publish(message.ToString(), exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this.Publish(string.Format(format, args));
        }

        public void Fatal(object message)
        {
            this.Publish(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            this.Publish(message.ToString(), exception);
        }

        public void Debug(object message)
        {
            this.Publish(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            this.Publish(message.ToString(), exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this.Publish(string.Format(format, args));
        }

        public void Info(object message)
        {
            this.Publish(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            this.Publish(message.ToString(), exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this.Publish(string.Format(format, args));
        }

        public void Warn(object message)
        {
            this.Publish(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            this.Publish(message.ToString(), exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.Publish(string.Format(format, args));
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