using System;

namespace StaticWwwHelpers
{
	public static class Configuration
	{
		private class DefaultErrorReporter : IErrorReporter
		{
			public void Log(string message)
			{
				throw new Exception(message);
			}

			public void Log(string message, Exception ex)
			{
				throw new Exception(message, ex);
			}
		}

		public static IErrorReporter ErrorReporter { get; set; }

		public static ICultureConfiguration CultureConfiguration { get; set; }

		static Configuration()
		{
			ErrorReporter = new DefaultErrorReporter();
		}


	}
}

