using System;

namespace KCVDB.Client
{
	public sealed class SendingErrorEventArgs : EventArgs
	{
		public SendingErrorEventArgs(
			Guid[] trackingIds,
			string message,
			ApiData[] apiData,
			DataSendingException exception)
		{
			TrackngIds = trackingIds;
			Message = message;
			ApiData = apiData;
			Exception = exception;
		}

		/// <summary>
		/// Gets the ID to track sending data.
		/// </summary>
		public Guid[] TrackngIds { get; }

		/// <summary>
		/// Gets the api call data
		/// </summary>
		/// <remarks>
		/// This might be NULL.
		/// </remarks>
		public ApiData[] ApiData { get; }

		/// <summary>
		/// Gets the error message
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Gets the related exception.
		/// </summary>
		public DataSendingException Exception { get; }

		/// <summary>
		/// Gest the simple error reason.
		/// </summary>
		public SendingErrorReason Reason => Exception?.Reason ?? SendingErrorReason.Unknown;
	}
}
