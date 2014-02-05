using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace SPing
{
	/// <summary>
	/// pingに数秒待てない人用の強化版ping
	/// </summary>
	class Program
	{
		static Ping _ping = new Ping();
		static int _timeOut = 0;
		static Dictionary<string, PingResult> _pingResult = new Dictionary<string, PingResult>();
        static int _echoCount = 1;
        
        static int Main(string[] args)
		{
			try
			{
				Parameter parameter = new Parameter(args);
				if (parameter.IsHelp == true)
				{
					Console.WriteLine("manコマンドでspingのヘルプを参照して下さい。");
					return 1;
				}
				#region Timeoutの設定
				SPing.Properties.Settings setting = new SPing.Properties.Settings();
				if (setting.TimeOut != 0)
					_timeOut = setting.TimeOut;

				if (parameter.TimeOut != 0)
					_timeOut = parameter.TimeOut;

                if (parameter.EchoCount != 0)
                    _echoCount = parameter.EchoCount;


				#endregion

				#region パラメータの最大スレッド数だけThreadをプールします。ただし、最大スレッド数は、256個
				//デフォルトは、250、1000⇒32bitだから？少ない？
				if (parameter.MaxThreads != 0)
					ThreadPool.SetMaxThreads(parameter.MaxThreads, parameter.CompletionPortThreads);

				#endregion

                InitUpdateAddress();

				parameter.ArgumentsExceptOptions.All(arg =>
				{
					string key = arg.IndexOf(",") >= 0 ? arg.Substring(0, arg.IndexOf(",")) : arg;
					string hostName = arg.IndexOf(",") >= 0 ? arg.Substring(arg.IndexOf(",") + 1) : null;
					if (_pingResult.ContainsKey(key) == true)
					{
						return true;
					}
					else
					{
                        key = UpdateAddress(key);
						PingResult pr = new PingResult();
						pr.HostName = hostName;
						_pingResult.Add(key, pr);

						return true;
					}
				});

				//pingの実行
				_pingResult.All(dic => { ThreadPool.QueueUserWorkItem(new WaitCallback(_Ping), dic); return true; });

				//結果の取得
				if (parameter.IsErrorOnly == true)
				{
					//全てのチェックを終えてからエラーのみをフィルタする。
					_pingResult.All(dic => { dic.Value.AutoResetEvent.WaitOne(); return true; });

					//エラーの実を絞ってクエリ抽出
					var pingResult = from ret in _pingResult where ret.Value.Exception != null || ret.Value.PingReply.Status != IPStatus.Success select ret;

					//表示
					return _ShowResult(pingResult, parameter, false);
				}
				else
				{
					//結果はパラメータ順に表示する。
					return _ShowResult(_pingResult, parameter, true);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetAllMessage());
				return 1;
			}
		}

        static Regex _pattern2 = null;
        static string _header2 = null;
        static Regex _pattern1 = null;
        static string _header1 = null;

        private static void InitUpdateAddress() 
        {
            Properties.Settings setting = new SPing.Properties.Settings();

            Regex header;
            _pattern2 = new Regex("^[0-9]{1,3}\\.[0-9]{1,3}$");
            header = new Regex("([0-9]{1,3}\\.[0-9]{1,3}\\.)([0-9]{1,3}\\.[0-9]{1,3})");
            _header2 = header.Match(setting.DefaultIpAddress).Groups[1].ToString();

            _pattern1 = new Regex("^[0-9]{1,3}$");;
            header = new Regex("([0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)([0-9]{1,3})");
            _header1 = header.Match(setting.DefaultIpAddress).Groups[1].ToString();

        }
        /// <summary>
        /// 20110513_Add発行を省略
        ///     本来ならばデフォルトのIPを持たせる
        ///         
        ///     オクテット省略機能を追加
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static string UpdateAddress(string address)
        {
            //２オクテット指定
            if (address != null && _pattern2.IsMatch(address) == true)
                return _header2 + address;

            //１オクテット指定
            if (address != null && _pattern1.IsMatch(address) == true)
                return _header1 + address;
            return address;
        }


		static int _ShowResult(IEnumerable<KeyValuePair<string, PingResult>> ret, Parameter parameter,  bool wait)
		{
			if (ret.Count() == 0) return 1;
			Encoding enc = Encoding.GetEncoding("Shift_JIS");
			var keyLenght = ret.Max(c => c.Key.Length);
			var hostNameLength = ret.Max(c => c.Value.HostName == null ? 0 : enc.GetBytes(c.Value.HostName).Length);

			int flg = 0;//0成功、1失敗
			ret.All(r =>
			{
				string hostName = null;
				hostName = r.Value.HostName == null ? ":" + ("".PadRight(hostNameLength)) : ":" + r.Value.HostName.PadRight(hostNameLength - (enc.GetBytes(r.Value.HostName).Length - r.Value.HostName.Length)) + "";
				if (wait == true)
				{
					r.Value.AutoResetEvent.WaitOne();
				}
				string result = null;
				if (r.Value.Exception == null)
				{
					if (r.Value.PingReply.Status == IPStatus.Success) {
						string detailResult = "";
						if (parameter.IsDetailResult == true)
						{
							detailResult = 
							  "(bytes=" + r.Value.PingReply.Buffer.Length.ToString() + "," 
							+ "time<" + (r.Value.PingReply.RoundtripTime + 1).ToString() + "ms,"
							+ "TTL=" + r.Value.PingReply.Options.Ttl.ToString() + ")";
						}
                        if (r.Key.ToUpper() == r.Value.PingReply.Address.ToString())
                        {
                            //IPの表示は行わない。
                        }
                        else
                        {
                            result = " [" + r.Value.PingReply.Address.ToString() + "] ";
                        }
                        result = r.Value.PingReply.Status.ToString() + result + detailResult;

					}
					else
					{
						result = r.Value.PingReply.Status.ToString();
						flg = 1;
					}
				}
				else
				{
					result = r.Value.Exception.GetAllMessage();
					flg = 1;
				}

                string timeString = "";
                if (parameter.TimeFormat == TimeFormat.YYYYMMDDHHMMSS)
                    timeString = DateTime.Now.ToString("yyyy/MM/dd(ddd) HH:mm:ss") + ":";

				Console.WriteLine(timeString + r.Key.PadRight(keyLenght) + hostName + ":" + result);
				return true;
			});

			return flg;
		}

        /// <summary>
        /// pingの実行
        /// </summary>
        /// <param name="state"></param>
		static void _Ping(object state)
		{
			KeyValuePair<string, PingResult> dic = (KeyValuePair<string, PingResult>)state;

            
            for (int counter = _echoCount; counter>0; counter--)
				try
				{
					lock (_ping)
						// タイムアウト値=0はデフォルトを使用します。
						dic.Value.PingReply = _timeOut != 0 ? _ping.Send(dic.Key, _timeOut) : _ping.Send(dic.Key);

				}
				catch (Exception ex)
				{
					if (counter == 1)
						dic.Value.Exception = ex;
				}
			
			//終了通知(正常／エラーも)
			dic.Value.AutoResetEvent.Set();
		}

	}
}
