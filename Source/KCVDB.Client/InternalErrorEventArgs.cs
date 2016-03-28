using System;

namespace KCVDB.Client
{
	public sealed class InternalErrorEventArgs : EventArgs
	{
		public InternalErrorEventArgs(
			Guid trackingId,
			string message,
			Exception exception,
			ApiData apiData = null)
		{
			TrackingId = trackingId;
			Message = message;
			Exception = exception;
			ApiData = ApiData;
		}

		/// <summary>
		/// Gets the ID to track sending data.
		/// </summary>
		public Guid TrackingId { get; }

		/// <summary>
		/// Gets the api call data
		/// </summary>
		/// <remarks>
		/// This might be NULL.
		/// </remarks>
		public ApiData ApiData { get; }

		/// <summary>
		/// Gets the error message
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Gets the related excetpion
		/// </summary>
		/// <remarks>
		/// This might be NULL.
		/// </remarks>
		public Exception Exception { get; }
	}
}
