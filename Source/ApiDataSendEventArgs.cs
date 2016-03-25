using System;

namespace KCVDB.Client
{
	public class ApiDataSendEventArgs : EventArgs
	{
		public ApiDataSendEventArgs(
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
