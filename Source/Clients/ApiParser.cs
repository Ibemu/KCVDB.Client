using System;

namespace KCVDB.Client.Clients
{
	class ApiParser : IApiParser
	{
		public ApiData ParseResponse(
			Uri requestUri,
			int statusCode,
			string requestBody,
			string responseBody,
			string dateHeaderValue)
		{
			if (requestUri == null) { throw new ArgumentNullException(nameof(requestUri)); }
			if (requestBody == null) { throw new ArgumentNullException(nameof(requestBody)); }
			if (responseBody == null) { throw new ArgumentNullException(nameof(responseBody)); }

			if (!requestUri.AbsoluteUri.Contains("kcsapi")) {
				throw new NonKancolleApiCallUriException(requestUri);
			}

			return new ApiData {
				RequestUri = requestUri,
				RequestBody = requestBody,
				ResponseBody = responseBody,
				HttpDateHeaderValue = dateHeaderValue,
				ReceivedLocalTime = DateTimeOffset.Now,
				StatusCode = statusCode,
			};
		}
	}
}
