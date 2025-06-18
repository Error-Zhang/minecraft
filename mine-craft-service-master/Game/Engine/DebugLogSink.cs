namespace Engine
{
	public class DebugLogSink : ILogSink
	{
		public LogType MinimumLogType
		{
			get;
			set;
		}

		public void Log(LogType logType, string message)
		{
			if (logType > MinimumLogType)
			{
				switch (logType)
				{
					case LogType.Debug:
					case LogType.Verbose:
					case LogType.Information:
					case LogType.Warning:
					case LogType.Error:
						return;
				}
				_ = string.Empty;
			}
		}

		public void Dispose()
		{
		}
	}
}