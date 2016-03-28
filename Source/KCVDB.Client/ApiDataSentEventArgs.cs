using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSentEventArgs : EventArgs
	{
		public ApiDataSentEventArgs(
			Guid trackingId,
			ApiData apiData,
			string requestBodyText)
		{
			TrackingId = trackingId;
			ApiData = apiData;
			RequestBodyText = requestBodyText;
		}

		public Guid TrackingId { get; }
		public ApiData ApiData { get; }
		public string RequestBodyText { get; }
	}
}
