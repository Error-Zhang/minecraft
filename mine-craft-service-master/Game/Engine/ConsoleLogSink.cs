using System;
using System.IO;
using System.Text;

namespace Engine
{
	public class ConsoleLogSink : ILogSink
	{
        public ConsoleLogSink()
		{
#if WINDOWS
            Console.OutputEncoding = Encoding.UTF8;
#endif
		}
		public LogType MinimumLogType
		{
			get;
			set;
		}

		public void Log(LogType logType, string message)
		{
			if (logType >= MinimumLogType)
			{
				string value;
				TextWriter textWriter;
				switch (logType)
				{
					case LogType.Debug:
						value = "DEBUG: ";
						textWriter = Console.Out;
						break;
					case LogType.Verbose:
						value = "INFO: ";
						textWriter = Console.Out;
						break;
					case LogType.Information:
						value = "INFO: ";
						textWriter = Console.Out;
						break;
					case LogType.Warning:
						value = "WARNING: ";
						textWriter = Console.Out;
						break;
					case LogType.Error:
						value = "ERROR: ";
						textWriter = Console.Error;
						break;
					default:
						value = string.Empty;
						textWriter = Console.Out;
						break;
				}
				textWriter.Write(DateTime.Now.ToString("HH:mm:ss.fff"));
				textWriter.Write(" ");
				textWriter.Write(value);
				textWriter.WriteLine(message);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{value}{message}");
#endif
            }
		}

		public void Dispose()
		{
		}
	}
}