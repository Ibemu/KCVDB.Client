using System;

namespace KCVDB.Client
{
	public sealed class InternalErrorEventArgs : EventArgs
	{
		public InternalErrorEventArgs(
			Guid[] trackingIds,
			string message,
			Exception exception,
			ApiData[] apiData = null)
		{
			TrackingIds = trackingIds;
			Message = message;
			Exception = exception;
			ApiData = apiData;
		}

		public InternalErrorEventArgs(
			Guid trackingId,
			string message,
			Exception exception,
			ApiData apiData)
		{
			TrackingIds = new Guid[] { trackingId };
			Message = message;
			Exception = exception;
			ApiData = new ApiData[] { apiData };
		}

		/// <summary>
		/// Gets the ID to track sending data.
		/// </summary>
		public Guid[] TrackingIds { get; }

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
		/// Gets the related excetpion
		/// </summary>
		/// <remarks>
		/// This might be NULL.
		/// </remarks>
		public Exception Exception { get; }
	}
}
