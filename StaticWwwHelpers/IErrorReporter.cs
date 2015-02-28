using System;

namespace StaticWwwHelpers
{
	public interface IErrorReporter
	{
		void Log(string message);
		void Log(string message, Exception ex);
	}
}

