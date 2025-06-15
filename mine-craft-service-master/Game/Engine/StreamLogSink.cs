using System;
using System.IO;

namespace Engine
{
	public class StreamLogSink : ILogSink
	{
		private StreamWriter m_writer;

		public LogType MinimumLogType
		{
			get;
			set;
		}

		public StreamLogSink(Stream stream)
		{
			m_writer = new StreamWriter(stream);
			stream.Position = stream.Length;
		}

		public void Log(LogType logType, string message)
		{
			if (logType >= MinimumLogType)
			{
                string str = logType switch
                {
                    LogType.Debug => "DEBUG: ",
                    LogType.Verbose => "INFO: ",
                    LogType.Information => "INFO: ",
                    LogType.Warning => "WARNING: ",
                    LogType.Error => "ERROR: ",
                    _ => string.Empty
                };
                m_writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                m_writer.Write(" ");
                m_writer.Write(str);
                m_writer.WriteLine(message);
				m_writer.Flush();
			}
		}

		public void Dispose()
		{
			m_writer.Dispose();
		}
	}
}