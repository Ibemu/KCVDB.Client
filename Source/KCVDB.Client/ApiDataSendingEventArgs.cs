using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSendingEventArgs : EventArgs
	{
		public ApiDataSendingEventArgs(
			Guid[] trackingIds,
			ApiData[] apiData)
		{
			TrackingIds = trackingIds;
			ApiData = apiData;
		}

		public Guid[] TrackingIds { get; }
		public ApiData[] ApiData { get; }
	}
}
