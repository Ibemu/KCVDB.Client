using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSentEventArgs : EventArgs
	{
		public ApiDataSentEventArgs(
			Guid[] trackingIds,
			ApiData[] apiData,
			ISentApiData sentApiData)
		{
			TrackingIds = trackingIds;
			ApiData = apiData;
			SentApiData = sentApiData;
		}

		public Guid[] TrackingIds { get; }
		public ApiData[] ApiData { get; }
		public ISentApiData SentApiData { get; }
	}
}
