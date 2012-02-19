namespace NHibernate.ZMQLogPublisher
{
    using System;
    using System.Text;

    using ZMQ;

    using Exception = System.Exception;

    public class ZmqLogger : IInternalLogger
    {
        private string keyName;
        
        private Socket publisher;

        public ZmqLogger(string keyName, Socket publisher)
        {
            this.keyName = keyName;
            this.publisher = publisher;
        }

        private void Publish(string message)
        {
            this.publisher.Send(string.Format("{0} - {1}", this.keyName, message), Encoding.Unicode);
        }

        public void Error(object message)
        {
            this.Publish(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            this.Publish(message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            this.Publish(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            this.Publish(message.ToString());
        }

        public void Debug(object message)
        {
            this.Publish(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            this.Publish(message.ToString());
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
            this.Publish(message.ToString());
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
            this.Publish(message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            this.Publish(string.Format(format,args));
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
    }
}