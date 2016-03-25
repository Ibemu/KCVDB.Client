using System;

namespace KCVDB.Client.Clients
{
	interface IApiParser
	{
		ApiData ParseResponse(
			Uri requestUri,
			int statusCode,
			string requestBody,
			string responseBody,
			string dateHeaderValue);
	}
}
