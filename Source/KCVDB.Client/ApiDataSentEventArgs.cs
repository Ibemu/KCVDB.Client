using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSentEventArgs : EventArgs
	{
		public ApiDataSentEventArgs(
			Guid trackingId,
			ApiData apiData,
			ISentApiData sentApiData)
		{
			TrackingId = trackingId;
			ApiData = apiData;
			SentApiData = sentApiData;
		}

		public Guid TrackingId { get; }
		public ApiData ApiData { get; }
		public ISentApiData SentApiData { get; }
	}
}
