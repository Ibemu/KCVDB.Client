using System;
using System.Runtime.Serialization;

namespace KCVDB.Client.Clients.Senders.Gzip
{
	[DataContract]
	class RequestApiData
	{
		public RequestApiData()
		{ }
		public RequestApiData(ApiData apiData)
		{
			if (apiData == null) { throw new ArgumentNullException(nameof(apiData)); }

			RequestUri = apiData.RequestUri.AbsoluteUri;
			RequestBody = apiData.RequestBody;
			ResponseBody = apiData.ResponseBody;
			StatusCode = apiData.StatusCode;
			HttpDate = apiData.HttpDateHeaderValue;
			LocalTime = apiData.ReceivedLocalTime.UtcDateTime.ToString("r");
		}

		[DataMember]
		public string RequestUri { get; set; }

		[DataMember]
		public string RequestBody { get; set; }

		[DataMember]
		public string ResponseBody { get; set; }

		[DataMember]
		public int? StatusCode { get; set; }

		[DataMember]
		public string HttpDate { get; set; }

		[DataMember]
		public string LocalTime { get; set; }
	}
}
