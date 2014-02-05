using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;

namespace SPing
{
    class PingResult
    {
        PingReply _pingReply = null;
        AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        Exception _exception = null;
        string _hostName = null;

        public PingReply PingReply
        {
            get
            {
                return _pingReply;
            }
            set
            {
                _pingReply = value;
            }
        }

        public AutoResetEvent AutoResetEvent
        {

            get
            {
                return _autoResetEvent;
            }
            set
            {
                _autoResetEvent = value;
            }
        }

        public Exception Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
            }
        }

        public string HostName
        {
            get
            {
                return _hostName;
            }
            set
            {
                _hostName = value;
            }
        }


    }
}
