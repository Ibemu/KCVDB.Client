using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSendingEventArgs : EventArgs
	{
		public ApiDataSendingEventArgs(
			Guid trackingId,
			ApiData apiData)
		{
			TrackingId = trackingId;
			ApiData = apiData;
		}

		public Guid TrackingId { get; }
		public ApiData ApiData { get; }
	}
}
