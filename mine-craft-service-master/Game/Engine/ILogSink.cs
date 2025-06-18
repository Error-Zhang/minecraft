namespace Engine
{
	public interface ILogSink
	{
		void Log(LogType type, string message);
	}
}