using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Engine
{
	public static class Log
	{
		private static object m_lock;

		private static List<ILogSink> m_logSinks;

		public static LogType MinimumLogType
		{
			get;
			set;
		}

		static Log()
		{
			m_lock = new object();
			m_logSinks = [];
			AddLogSink(new ConsoleLogSink());
			MinimumLogType = LogType.Information;
		}

		public static void Write(LogType type, string message)
		{
			if (m_logSinks.Count > 0 && type >= MinimumLogType)
			{
				lock (m_lock)
				{
					foreach (ILogSink logSink in m_logSinks)
					{
						try
						{
							logSink.Log(type, message);
						}
						catch
						{
						}
					}
				}
			}
#if ANDROID
            System.Console.WriteLine("SCAPI["+ type.ToString() +"]" + message);
#endif
		}

		[Conditional("DEBUG")]
		public static void Debug(object message)
		{
			Write(LogType.Debug, (message != null) ? message.ToString() : "null");
		}

		[Conditional("DEBUG")]
		public static void Debug(string message)
		{
			Write(LogType.Debug, message);
		}

		[Conditional("DEBUG")]
		public static void Debug(string format, params object[] parameters)
		{
			Write(LogType.Debug, string.Format(format, parameters));
		}

		public static void Verbose(object message)
		{
			Write(LogType.Verbose, (message != null) ? message.ToString() : "null");
		}

		public static void Verbose(string message)
		{
			Write(LogType.Verbose, message);
		}

		public static void Verbose(string format, params object[] parameters)
		{
			Write(LogType.Verbose, string.Format(format, parameters));
		}

		public static void Information(object message)
		{
			Write(LogType.Information, (message != null) ? message.ToString() : "null");
		}

		public static void Information(string message)
		{
			Write(LogType.Information, message);
		}

		public static void Information(string format, params object[] parameters)
		{
			Write(LogType.Information, string.Format(format, parameters));
		}

		public static void Warning(object message)
		{
			Write(LogType.Warning, (message != null) ? message.ToString() : "null");
		}

		public static void Warning(string message)
		{
			Write(LogType.Warning, message);
		}

		public static void Warning(string format, params object[] parameters)
		{
			Write(LogType.Warning, string.Format(format, parameters));
		}

		public static void Error(object message)
		{
			Write(LogType.Error, (message != null) ? message.ToString() : "null");
		}

		public static void Error(string message)
		{
			Write(LogType.Error, message);
#if WINDOWS
            Window.TitleSuffix = " #" + message;
#endif
		}

		public static void Error(string format, params object[] parameters)
		{
			Write(LogType.Error, string.Format(format, parameters));
		}

        public static void Warning(Exception e)
        {
            Write(LogType.Warning,e.Message+"↓");
            Write(LogType.Warning,e.ToString());
        }
        public static void Error(Exception e)
        {
            Write(LogType.Error, e.Message + "↓");
            if (e is NullReferenceException e_null)
            {
                Write(LogType.Error, $"NullReferenceException: {e_null.TargetSite?.DeclaringType?.Name}.{e_null.TargetSite?.Name} is null");
            }
            Write(LogType.Error, e.ToString());
        }
        public static void AddLogSink(ILogSink logSink)
		{
			lock (m_lock)
			{
				if (!m_logSinks.Contains(logSink))
				{
					m_logSinks.Add(logSink);
				}
			}
		}

		public static void RemoveLogSink(ILogSink logSink)
		{
			lock (m_lock)
			{
				m_logSinks.Remove(logSink);
			}
		}

		public static void RemoveAllLogSinks()
		{
			lock (m_lock)
			{
				m_logSinks.Clear();
			}
		}
	}
}