using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCVDB.Client.Clients.Senders
{
	[Flags]
	public enum SentApiDataRequestBodyFlags
	{
		ByteArray = 1,
		String = 2
	}
}
