using System;

namespace KCVDB.Client.Clients.Senders.Gzip
{
	class GzipApiDataSenderFactory : IApiDataSenderFactory
	{
		public IApiDataSender CreateSender(Uri apiServerUri, string agentId, string actualSessionId)
		{
			return new GzipApiDataSender(apiServerUri, agentId, actualSessionId);
		}
	}
}
