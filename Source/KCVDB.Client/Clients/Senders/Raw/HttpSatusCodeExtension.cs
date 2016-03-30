using System.Net;

namespace KCVDB.Client.Clients.Senders.Raw
{
	static class HttpSatusCodeExtention
	{
		public static bool IsServerError(this HttpStatusCode code)
		{
			switch (code) {
				case HttpStatusCode.InternalServerError:
				case HttpStatusCode.NotImplemented:
				case HttpStatusCode.BadGateway:
				case HttpStatusCode.ServiceUnavailable:
				case HttpStatusCode.GatewayTimeout:
				case HttpStatusCode.HttpVersionNotSupported:
					return true;

				default:
					return false;
			}
		}
	}
}
