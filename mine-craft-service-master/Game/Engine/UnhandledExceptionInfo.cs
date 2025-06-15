using System;

namespace Engine
{
	public class UnhandledExceptionInfo
	{
		public readonly Exception Exception;

		public bool IsHandled;

		public UnhandledExceptionInfo(Exception e)
		{
			Exception = e;
		}
	}
}