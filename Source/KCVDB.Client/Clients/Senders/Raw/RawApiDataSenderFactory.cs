using System;

namespace KCVDB.Client.Clients.Senders.Raw
{
	internal class RawApiDataSenderFactory : IApiDataSenderFactory
	{
		public IApiDataSender CreateSender(Uri apiServerUri, string agentId, string actualSessionId)
		{
			return new RawApiDataSender(apiServerUri, agentId, actualSessionId);
		}
	}
}
