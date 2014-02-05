using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SPing
{
    enum TimeFormat
    {
        YYYYMMDDHHMMSS,
        NONE
    }

    class Parameter : BaseParameter
    {

        int _timeOut = 0;

        //カウンタ
        int _echoCount = 0;     //デフォルト１回

        int _maxThreads = 0;
        int _completionPortThreads = 0;
        bool _isErrorOnly = false;
        bool _isDetailResult = false;
        bool _isHelp = false;

        TimeFormat _timeFormat = TimeFormat.NONE;

        public Parameter(string[] args)
            : base(args)
        {
            _SetParameter(args);
        }

        void _SetParameter(string[] args)
        {
            string[] values;
            values = base.GetParameter("W", 1);
            if (values != null)
            {
                try
                {
                    _timeOut = int.Parse(values[0]);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("パラメータ タイムアウト(/w)の指定が正しくありません。指定は数字である必要があります。", ex);
                }
            }

            values = base.GetParameter("S", 2);
            if (values != null)
            {
                try
                {
                    _maxThreads = int.Parse(values[0]);
                    _completionPortThreads = int.Parse(values[1]);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("パラメータ/sのMaxThreadsCount, CompletionPortThreadsが正しくありません。指定する内容は２つの数字である必要があります。", ex);
                }
            }

            values = base.GetParameter("E", 0);
            if (values != null)
            {
                _isErrorOnly = true;
            }

            values = base.GetParameter("D", 0);
            if (values != null)
            {
                _isDetailResult = true;
            }

            values = base.GetParameter("?", 0);
            if (values != null)
            {
                _isHelp = true;
            }

            values = base.GetParameter("help", 0);
            if (values != null)
            {
                _isHelp = true;
            }

            values = base.GetParameter("T", 0);
            if (values != null)
            {
                _timeFormat = TimeFormat.YYYYMMDDHHMMSS;
            }


            values = base.GetParameter("N", 1);
            if (values != null)
            {
                try
                {
                    _echoCount = int.Parse(values[0]);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("パラメータ タイムアウト(/w)の指定が正しくありません。指定は数字である必要があります。", ex);
                }
            }

            values = base.GetParameter("I", 1);
            if (values != null)
            {
                string parameterFile = values[0];
                string parameterData = File.ReadAllText(parameterFile);
                string[] parameters = parameterData.Split(new char[] { ' ', '\r', '\n', });
                //パラメータ解析
                _SetParameter(parameters);
                //残りは通常引数
                parameters.All(param =>
                {
                    if (param.Trim() != "")
                    {
                        ArgumentsExceptOptions.Add(param);
                    }
                    return true;
                });
            }

        }

        public int TimeOut
        {
            get
            {
                return _timeOut;
            }
        }

        public int EchoCount
        {
            get
            {
                return _echoCount;
            }
            set
            {
                _echoCount = value;
            }
        }

        public int MaxThreads
        {
            get
            {
                return _maxThreads;
            }
            set
            {
                _maxThreads = value;
            }
        }
        public int CompletionPortThreads
        {
            get
            {
                return _completionPortThreads;
            }
            set
            {
                _completionPortThreads = value;
            }
        }

        public bool IsErrorOnly
        {
            get
            {
                return _isErrorOnly;
            }
            set
            {
                _isErrorOnly = value;
            }
        }
        public bool IsDetailResult
        {
            get
            {
                return _isDetailResult;
            }
            set
            {
                _isDetailResult = value;
            }
        }
        public bool IsHelp
        {
            get
            {
                return _isHelp;
            }
            set
            {
                _isHelp = value;
            }
        }

        public TimeFormat TimeFormat
        {
            get
            {
                return _timeFormat;
            }
            set
            {
                _timeFormat = value;
            }
        }
    }
}
