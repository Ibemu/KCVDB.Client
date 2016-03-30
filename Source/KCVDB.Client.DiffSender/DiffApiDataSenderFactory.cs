using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders.Diff
{
	public class DiffApiDataSenderFactory : IApiDataSenderFactory
	{
		public IApiDataSender CreateSender(Uri apiServerUri, string agentId, string actualSessionId)
		{
			return new DiffApiDataSender(apiServerUri, agentId, actualSessionId);
		}
	}
}
