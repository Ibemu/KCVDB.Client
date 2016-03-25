using System;

namespace KCVDB.Client
{
	public class ApiData
	{
		public Uri RequestUri { get; set; }

		public string RequestBody { get; set; }

		public string ResponseBody { get; set; }

		public int StatusCode { get; set; }

		public string HttpDateHeaderValue { get; set; }

		public DateTimeOffset ReceivedLocalTime { get; set; }
	}
}
