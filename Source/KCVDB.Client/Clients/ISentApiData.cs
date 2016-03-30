using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders
{
	public interface ISentApiData
	{
		int RequestBodyByteCount { get; }

		string RequestBodyString { get; }

		byte[] RequestBodyByteArray { get; }

		SentApiDataRequestBodyFlags RequestBodyFlags { get; }

		ApiDataSenderType SenderType { get; }
	}
}
