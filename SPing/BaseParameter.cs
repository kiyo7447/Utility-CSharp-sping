using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPing
{
    class BaseParameter
    {
        List<string> _argumentsExceptOptions = new List<string>();

        public BaseParameter(string[] args)
        {
            args.All(arg => { _argumentsExceptOptions.Add(arg); return true; });
        }

        protected string[] GetParameter(string key, int dataCount)
        {
            int idx;
            for (idx = 0; idx < _argumentsExceptOptions.Count; idx++)
            {
                bool existsFlg = false;
                if (_argumentsExceptOptions[idx] == "/" + key.ToLower())
                {
                    existsFlg = true;
                }
                else if (_argumentsExceptOptions[idx] == "/" + key.ToUpper())
                {
                    existsFlg = true;
                }
                else if (_argumentsExceptOptions[idx] == "-" + key.ToLower())
                {
                    existsFlg = true;
                }
                else if (_argumentsExceptOptions[idx] == "-" + key.ToUpper())
                {
                    existsFlg = true;
                }

                if (existsFlg == true)
                {
                    break;
                }
            }
            if (idx == _argumentsExceptOptions.Count)
            {
                //パラメータなし
                return null;
            }
            else
            {
                //パラメータあり
                _argumentsExceptOptions.RemoveAt(idx);

                string[] ret = new string[dataCount];
                for (int idxRet = 0; idxRet < dataCount; idxRet++)
                {
                    if (idx >= _argumentsExceptOptions.Count)
                    {
                        throw new ApplicationException("パラメータ/" + key + "に続く引数がありません。引数を正しく指定してください。");
                    }
                    ret[idxRet] = _argumentsExceptOptions[idx];
                    _argumentsExceptOptions.RemoveAt(idx);
                }

                return ret;
            }
        }

        public List<string> ArgumentsExceptOptions
        {
            get
            {
                return _argumentsExceptOptions;
            }
        }
    }
}
