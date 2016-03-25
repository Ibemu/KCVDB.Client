using System;

namespace KCVDB.Client
{
	public class FatalErrorEventArgs : EventArgs
	{
		public FatalErrorEventArgs(
			string message,
			Exception exception)
		{
			Message = message;
			Exception = exception;
		}

		public string Message { get; }

		public Exception Exception { get; }
	}
}
