using System;

namespace KCVDB.Client
{
	public sealed class ApiDataSentEventArgs : EventArgs
	{
		public ApiDataSentEventArgs(
			Guid trackingId,
			ApiData apiData,
			byte[] requestBodyByteArray)
		{
			TrackingId = trackingId;
			ApiData = apiData;
			RequestBodyByteArray = requestBodyByteArray;
		}

		public Guid TrackingId { get; }
		public ApiData ApiData { get; }
		public byte[] RequestBodyByteArray { get; }
	}
}
